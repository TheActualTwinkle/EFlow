using EFlow.Booking.Domain;
using EFlow.Common.Domain;
using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace EFlow.Booking.ArchitectureTests.Domain;

public class DomainTests
{
    private readonly string[] _allowedProjects =
    [
        "EFlow.Common.Domain",
    ];

    [Fact]
    public void Domain_ShouldNotHaveForbiddenAssemblyReferences()
    {
        // Arrange & Act
        var domainAssembly = typeof(Identity).Assembly;
        
        var forbiddenProjects = domainAssembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name!.StartsWith("EFlow.", StringComparison.Ordinal))
            .Where(name => !_allowedProjects.Contains(name))
            .Distinct()
            .ToArray();

        // Assert
        forbiddenProjects.Should().BeEmpty(
            $"Domain should not have dependencies on any other projects (except: {string.Join(", ", _allowedProjects)}) but found " +
            $"\n{string.Join(",\n", forbiddenProjects)} dependencies.");
    }

    [Fact]
    public void DomainEvents_ShouldBeSealed()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(Identity).Assembly)
            .That()
            .AreClasses()
            .And()
            .Inherit(typeof(DomainEvent))
            .Should()
            .BeSealed()
            .GetResult();

        var failingTypes = string.Join("\n", result.FailingTypes?.Select(type => type.FullName ?? type.Name) ?? []);

        // Assert
        result.IsSuccessful.Should().BeTrue($"All domain events should be sealed, but found:\n{failingTypes}");
    }

    [Fact]
    public void DomainEvents_ShouldHaveDomainEventSuffix()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(Identity).Assembly)
            .That()
            .AreClasses()
            .And()
            .Inherit(typeof(DomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult();

        var failingTypes = string.Join("\n", result.FailingTypes?.Select(type => type.FullName ?? type.Name) ?? []);

        // Assert
        result.IsSuccessful.Should().BeTrue($"All domain events should have the 'DomainEvent' suffix, but found:\n{failingTypes}");
    }

    [Fact]
    public void Entities_ShouldBeSealed()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(Identity).Assembly)
            .That()
            .AreClasses()
            .And()
            .Inherit(typeof(Entity))
            .Should()
            .BeSealed()
            .GetResult();

        var failingTypes = string.Join("\n", result.FailingTypes?.Select(type => type.FullName ?? type.Name) ?? []);

        // Assert
        result.IsSuccessful.Should().BeTrue($"All entities should be sealed, but found:\n{failingTypes}");
    }

    [Fact]
    public void Entities_ShouldHavePrivateParameterlessConstructor()
    {
        // Arrange & Act
        var invalidEntities = GetTypesImplementing<Entity>()
            .Where(type =>
            {
                var parameterlessCtor = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    types: Type.EmptyTypes,
                    modifiers: null);

                return parameterlessCtor is null || !parameterlessCtor.IsPrivate;
            })
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name)
            .ToArray();

        // Assert
        invalidEntities.Should().BeEmpty(
            "Entities should define a private parameterless constructor, but found:\n{0}",
            string.Join("\n", invalidEntities));
    }

    [Fact]
    public void BusinessRules_ShouldBeSealed()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(Identity).Assembly)
            .That()
            .AreClasses()
            .And()
            .ImplementInterface(typeof(IBusinessRule))
            .Should()
            .BeSealed()
            .GetResult();

        var failingTypes = string.Join("\n", result.FailingTypes?.Select(type => type.FullName ?? type.Name) ?? []);

        // Assert
        result.IsSuccessful.Should().BeTrue($"All business rule implementations should be sealed, but found:\n{failingTypes}");
    }

    [Fact]
    public void BusinessRules_ShouldHaveOnlyInternalConstructors()
    {
        // Arrange & Act
        var invalidRules = GetTypesImplementing<IBusinessRule>()
            .Where(type =>
            {
                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return constructors.Length == 0 || constructors.Any(ctor => !ctor.IsAssembly);
            })
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name)
            .ToArray();

        // Assert
        invalidRules.Should().BeEmpty(
            "IBusinessRule implementations should have only internal constructors, but found:\n{0}",
            string.Join("\n", invalidRules));
    }

    private static IEnumerable<Type> GetTypesImplementing<TBaseType>() =>
        typeof(Identity)
            .Assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false }
                && type.IsAssignableTo(typeof(TBaseType)));
}