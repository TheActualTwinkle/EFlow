using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using EFlow.Booking.ApiTests.Infrastructure.Sessions;

namespace EFlow.Booking.ApiTests.Infrastructure.Fixtures;

/// <summary>
/// Boots the external infrastructure required by Booking API tests and exposes isolated HTTP sessions.
/// </summary>
public sealed class ApiTestStackFixture : IAsyncLifetime
{
    private readonly string _baseComposeFilePath;
    private readonly Dictionary<string, string> _composeOverrides;
    private readonly string _envFilePath;
    private readonly bool _manageStack;
    private readonly string _projectRoot;
    private readonly string _testsComposeFilePath;

    public ApiTestStackFixture()
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

        var baseUrl = Environment.GetEnvironmentVariable("EFLOW_API_TESTS_BASE_URL");

        if (string.IsNullOrWhiteSpace(baseUrl) &&
            _composeOverrides.TryGetValue("BOOKING_API_PORT", out var dynamicBookingPort))
            baseUrl = $"http://localhost:{dynamicBookingPort}";

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            var bookingApiPort = env.GetValueOrDefault("BOOKING_API_PORT", "8081");
            baseUrl = $"http://localhost:{bookingApiPort}";
        }

        BaseUri = new Uri(baseUrl, UriKind.Absolute);
    }

    /// <summary>
    /// Gets the base address of the Booking API instance used by the tests.
    /// </summary>
    private Uri BaseUri { get; }

    /// <summary>
    /// Starts the dockerized API stack when the fixture is configured to manage it automatically.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_manageStack)
            await RunDockerComposeAsync("up", "-d", "--build");

        await WaitUntilHealthyAsync();
    }

    /// <summary>
    /// Stops the managed dockerized API stack after the test collection completes.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_manageStack)
            await RunDockerComposeAsync("down", "-v");
    }

    /// <summary>
    /// Creates a new HTTP session with its own cookie container for an individual test actor.
    /// </summary>
    public ApiSession CreateSession() =>
        new(BaseUri);

    private async Task WaitUntilHealthyAsync()
    {
        // ReSharper disable once UsingStatementResourceInitialization
        // ReSharper disable once ShortLivedHttpClient
        using var client = new HttpClient { BaseAddress = BaseUri };
        var timeoutAt = DateTime.UtcNow.AddMinutes(2);

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                using var response = await client.GetAsync("/health");

                if (response.StatusCode == HttpStatusCode.OK)
                    return;
            }
            catch
            {
                // ignored
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"API did not become healthy at {new Uri(BaseUri, "/health")} within 120 seconds.");
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

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            result[key] = value;
        }

        return result;
    }

    private static Dictionary<string, string> CreateDynamicComposeOverrides()
    {
        var bookingPort = GetFreeTcpPort();
        var notificationsPort = GetFreeTcpPort();

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["POSTGRES_PORT"] = GetFreeTcpPort().ToString(),
            ["KAFKA_PORT"] = GetFreeTcpPort().ToString(),
            ["BOOKING_API_PORT"] = bookingPort.ToString(),
            ["NOTIFICATIONS_API_PORT"] = notificationsPort.ToString(),
            ["JWT_ISSUER"] = $"http://localhost:{bookingPort}",
            ["JWT_AUDIENCE"] = $"http://localhost:{bookingPort}"
        };
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}