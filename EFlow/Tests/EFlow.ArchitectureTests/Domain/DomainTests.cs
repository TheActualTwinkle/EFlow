using EFlow.Domain;
using FluentAssertions;
using NetArchTest.Rules;

namespace EFlow.ArchitectureTests.Domain;

public class DomainTests
{
    private const string DomainNamespace = "EFlow.Domain";
    private const string TestNamespace = "EFlow.ArchitectureTests";

    private static readonly string[] DirectoryExclusions = ["scripts"];

    [Fact]
    public void Domain_ShouldNotHaveProjectDependencies()
    {
        // Arrange
        var domainAssembly = typeof(IEntity).Assembly;

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

            if (DirectoryExclusions.Any(excludedDirectory => string.Equals(directoryName, excludedDirectory, StringComparison.OrdinalIgnoreCase)))
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