using EFlow.Common.Domain;
using FluentAssertions;
using NetArchTest.Rules;

namespace EFlow.Booking.ArchitectureTests.Domain;

public class DomainTests
{
    private const string DomainNamespace = "EFlow.Booking.Domain";
    private const string TestNamespace = "EFlow.Booking.ArchitectureTests";

    private static readonly string[] IgnoredDirectories = ["scripts"];

    [Fact]
    public void Domain_ShouldNotHaveProjectDependencies()
    {
        // Arrange
        var domainAssembly = typeof(Entity).Assembly;

        var solutionDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
        var csprojFiles = GetCsprojFilesExcludingDirectories(solutionDirectory);

        var forbiddenProjects = csprojFiles
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != DomainNamespace && name != TestNamespace)
            .ToArray();

        // Act
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Domain should not have dependencies on any other projects");
    }

    private static IEnumerable<string> GetCsprojFilesExcludingDirectories(string rootDirectory)
    {
        var stack = new Stack<string>();
        stack.Push(rootDirectory);

        while (stack.Count > 0)
        {
            var currentDirectory = stack.Pop();
            var directoryName = Path.GetFileName(currentDirectory);

            if (IgnoredDirectories.Any(excludedDirectory => string.Equals(directoryName, excludedDirectory, StringComparison.OrdinalIgnoreCase)))
                continue;

            foreach (var file in Directory.GetFiles(currentDirectory, "*.csproj"))
                yield return file;

            foreach (var dir in Directory.GetDirectories(currentDirectory))
                stack.Push(dir);
        }
    }

    // TODO: Domain events should be sealed
    // TODO: Domain events should has 'DomainEvent' suffix
    // TODO: Entities should have private parameterless constructor
}