using FluentAssertions;
using NetArchTest.Rules;

namespace EFlow.ArchitectureTests.Domain;

public class DomainTests
{
    private const string DomainNamespace = "EFlow.Domain";
    private const string TestNamespace = "EFlow.ArchitectureTests";
    
    [Fact]
    public void Domain_Should_Not_Have_Project_Dependencies()
    {
        // Arrange & Act
        var domainAssembly = typeof(EFlow.Domain.IEntity).Assembly;

        var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../.."));
        var csprojFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);
        
        var forbiddenProjects = csprojFiles
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != DomainNamespace && name != TestNamespace)
            .ToArray();

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();
        
        // Assert
        result.IsSuccessful.Should().BeTrue("Domain should not have dependencies on any other projects");
    }
    
    // TODO: Domain events should be sealed
    // TODO: Domain events should has 'DomainEvent' suffix
    // TODO: Entities should have private parameterless constructor
}