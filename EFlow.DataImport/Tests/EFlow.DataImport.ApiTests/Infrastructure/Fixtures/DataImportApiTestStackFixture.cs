using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using EFlow.DataImport.ApiTests.Infrastructure.Sessions;

namespace EFlow.DataImport.ApiTests.Infrastructure.Fixtures;

public sealed class DataImportApiTestStackFixture : IAsyncLifetime
{
    private const string AdminUsernameKey = "ADMIN_USERNAME";
    private const string AdminPasswordKey = "ADMIN_PASSWORD";
    private const string BookingApiPortKey = "BOOKING_API_PORT";
    private const string DataImportApiPortKey = "DATA_IMPORT_API_PORT";

    private readonly string _baseComposeFilePath;
    private readonly Dictionary<string, string> _composeOverrides;
    private readonly string _envFilePath;
    private readonly bool _manageStack;
    private readonly string _projectRoot;
    private readonly string _testsComposeFilePath;

    public DataImportApiTestStackFixture()
    {
        _projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
        _envFilePath = Path.Combine(_projectRoot, "docker", "api-tests.env");
        _baseComposeFilePath = Path.Combine(_projectRoot, "docker", "docker-compose.prod.yml");
        _testsComposeFilePath = Path.Combine(_projectRoot, "docker", "docker-compose.api-tests.yml");
        var env = File.Exists(_envFilePath) ? ParseEnvFile(_envFilePath) : [];

        _manageStack = Environment.GetEnvironmentVariable("EFLOW_API_TESTS_MANAGE_STACK") switch
        {
            "0" => false,
            "false" => false,
            "False" => false,
            _ => true
        };

        _composeOverrides = _manageStack ? CreateDynamicComposeOverrides() : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        BookingBaseUri = CreateBaseUri(
            Environment.GetEnvironmentVariable("EFLOW_BOOKING_API_TESTS_BASE_URL"),
            _composeOverrides,
            env,
            BookingApiPortKey,
            "8081");

        DataImportBaseUri = CreateBaseUri(
            Environment.GetEnvironmentVariable("EFLOW_DATA_IMPORT_API_TESTS_BASE_URL"),
            _composeOverrides,
            env,
            DataImportApiPortKey,
            "8083");

        AdminUsername = Environment.GetEnvironmentVariable(AdminUsernameKey)
                        ?? env.GetValueOrDefault(AdminUsernameKey, "admin");
        AdminPassword = Environment.GetEnvironmentVariable(AdminPasswordKey)
                        ?? env.GetValueOrDefault(AdminPasswordKey, "admin123");
    }

    public Uri BookingBaseUri { get; }

    public Uri DataImportBaseUri { get; }

    public string AdminUsername { get; }

    public string AdminPassword { get; }

    public async Task InitializeAsync()
    {
        if (_manageStack)
            await RunDockerComposeAsync("up", "-d", "--build");

        await WaitUntilHealthyAsync(BookingBaseUri, "/health");
        await WaitUntilHealthyAsync(DataImportBaseUri, "/openapi/v1.json");
    }

    public async Task DisposeAsync()
    {
        if (_manageStack)
            await RunDockerComposeAsync("down", "-v");
    }

    public DataImportApiSession CreateSession() =>
        new(BookingBaseUri, DataImportBaseUri);

    private async Task WaitUntilHealthyAsync(Uri baseUri, string path)
    {
        using var client = new HttpClient { BaseAddress = baseUri };
        var timeoutAt = DateTime.UtcNow.AddMinutes(2);

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                using var response = await client.GetAsync(path);

                if (response.StatusCode == HttpStatusCode.OK)
                    return;
            }
            catch
            {
                // ignored until timeout
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"API did not become healthy at {new Uri(baseUri, path)} within 120 seconds.");
    }

    private async Task RunDockerComposeAsync(params string[] args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            WorkingDirectory = _projectRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add("compose");
        startInfo.ArgumentList.Add("--env-file");
        startInfo.ArgumentList.Add(_envFilePath);
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add(_baseComposeFilePath);
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add(_testsComposeFilePath);

        foreach (var overridePair in _composeOverrides)
            startInfo.Environment[overridePair.Key] = overridePair.Value;

        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);

        using var process = Process.Start(startInfo)
                            ?? throw new InvalidOperationException("Failed to start docker compose process.");

        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"docker compose {string.Join(' ', args)} failed with exit code {process.ExitCode}.{Environment.NewLine}" +
                $"STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                $"STDERR:{Environment.NewLine}{stderr}");
    }

    private static Uri CreateBaseUri(
        string? environmentBaseUrl,
        IReadOnlyDictionary<string, string> composeOverrides,
        IReadOnlyDictionary<string, string> env,
        string portKey,
        string defaultPort)
    {
        if (!string.IsNullOrWhiteSpace(environmentBaseUrl))
            return new Uri(environmentBaseUrl, UriKind.Absolute);

        var port = composeOverrides.GetValueOrDefault(portKey) ?? env.GetValueOrDefault(portKey, defaultPort);

        return new Uri($"http://localhost:{port}", UriKind.Absolute);
    }

    private static Dictionary<string, string> ParseEnvFile(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();

            if (trimmed.Length == 0 ||
                trimmed.StartsWith('#'))
                continue;

            var separatorIndex = trimmed.IndexOf('=');

            if (separatorIndex <= 0)
                continue;

            result[trimmed[..separatorIndex].Trim()] = trimmed[(separatorIndex + 1)..].Trim();
        }

        return result;
    }

    private static Dictionary<string, string> CreateDynamicComposeOverrides()
    {
        var bookingPort = GetFreeTcpPort();
        var dataImportPort = GetFreeTcpPort();
        var bookingBaseUrl = $"http://localhost:{bookingPort}";

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["POSTGRES_PORT"] = GetFreeTcpPort().ToString(),
            ["KAFKA_PORT"] = GetFreeTcpPort().ToString(),
            [BookingApiPortKey] = bookingPort.ToString(),
            [DataImportApiPortKey] = dataImportPort.ToString(),
            ["NOTIFICATIONS_API_PORT"] = GetFreeTcpPort().ToString(),
            ["JWT_ISSUER"] = bookingBaseUrl,
            ["JWT_AUDIENCE"] = bookingBaseUrl,
            ["CORS_ALLOWED_ORIGIN_0"] = bookingBaseUrl
        };
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
