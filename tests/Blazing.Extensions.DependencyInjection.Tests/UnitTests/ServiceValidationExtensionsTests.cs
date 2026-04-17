using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ServiceValidationExtensions"/> — covering
/// duplicate detection, lifetime compatibility, dependency graph, and diagnostics.
/// </summary>
public class ServiceValidationExtensionsTests
{
    // ---------------------------------------------------------------------------
    // ValidateDuplicateRegistrations Tests
    // ---------------------------------------------------------------------------

    #region ValidateDuplicateRegistrations Tests

    [Fact]
    public void ValidateDuplicateRegistrations_Should_ReturnEmptyWhenNoduplicates()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddTransient<IDependentService, DependentService>();

        // Act
        var duplicates = services.ValidateDuplicateRegistrations();

        // Assert
        duplicates.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateDuplicateRegistrations_Should_DetectDuplicateRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<ITestService, AnotherTestService>(); // second registration
        services.AddTransient<IDependentService, DependentService>();

        // Act
        var duplicates = services.ValidateDuplicateRegistrations().ToList();

        // Assert
        duplicates.ShouldNotBeEmpty();
        duplicates.ShouldContain(d => d.Registrations.Count == 2);
    }

    [Fact]
    public void ValidateDuplicateRegistrations_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.ValidateDuplicateRegistrations());
    }

    #endregion

    // ---------------------------------------------------------------------------
    // ValidateLifetimeCompatibility Tests
    // ---------------------------------------------------------------------------

    #region ValidateLifetimeCompatibility Tests

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based validation.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based validation.")]
    [Fact]
    public void ValidateLifetimeCompatibility_Should_ReturnEmptyWhenNoViolations()
    {
        // Arrange — all services have compatible lifetimes
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        var violations = services.ValidateLifetimeCompatibility();

        // Assert
        violations.ShouldBeEmpty();
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based validation.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based validation.")]
    [Fact]
    public void ValidateLifetimeCompatibility_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.ValidateLifetimeCompatibility());
    }

    #endregion

    // ---------------------------------------------------------------------------
    // GetServiceDependencyGraph Tests
    // ---------------------------------------------------------------------------

    #region GetServiceDependencyGraph Tests

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based graph inspection.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based graph inspection.")]
    [Fact]
    public void GetServiceDependencyGraph_Should_ReturnGraphWithRegisteredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddScoped<IDependentService, DependentService>();

        // Act
        var graph = services.GetServiceDependencyGraph();

        // Assert
        graph.ShouldNotBeNull();
        graph.Services.ShouldNotBeEmpty();
        graph.Services.Count.ShouldBe(2);
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based graph inspection.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based graph inspection.")]
    [Fact]
    public void GetServiceDependencyGraph_Should_ReturnEmptyGraphForEmptyCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var graph = services.GetServiceDependencyGraph();

        // Assert
        graph.ShouldNotBeNull();
        graph.Services.ShouldBeEmpty();
    }

    [Fact]
    public void GetServiceDependencyGraph_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.GetServiceDependencyGraph());
    }

    #endregion

    // ---------------------------------------------------------------------------
    // GetDiagnostics Tests
    // ---------------------------------------------------------------------------

    #region GetDiagnostics Tests

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based diagnostics.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based diagnostics.")]
    [Fact]
    public void GetDiagnostics_Should_ReturnCorrectServiceCounts()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddScoped<IScopedTestService, ScopedTestService>();
        services.AddTransient<IDependentService, DependentService>();

        // Act
        var diagnostics = services.GetDiagnostics();

        // Assert
        diagnostics.TotalServices.ShouldBe(3);
        diagnostics.SingletonCount.ShouldBe(1);
        diagnostics.ScopedCount.ShouldBe(1);
        diagnostics.TransientCount.ShouldBe(1);
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based diagnostics.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based diagnostics.")]
    [Fact]
    public void GetDiagnostics_Should_ReportDuplicatesInWarnings()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<ITestService, AnotherTestService>(); // duplicate

        // Act
        var diagnostics = services.GetDiagnostics();

        // Assert
        diagnostics.Duplicates.Count.ShouldBeGreaterThan(0);
        diagnostics.Warnings.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GetDiagnostics_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.GetDiagnostics());
    }

    #endregion

    // ---------------------------------------------------------------------------
    // ThrowIfInvalid Tests
    // ---------------------------------------------------------------------------

    #region ThrowIfInvalid Tests

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026",
        Justification = "Test code: intentionally uses reflection-based validation.")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050",
        Justification = "Test code: intentionally uses reflection-based validation.")]
    [Fact]
    public void ThrowIfInvalid_Should_NotThrowWhenCollectionIsValid()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act & Assert — should not throw
        Should.NotThrow(() => services.ThrowIfInvalid());
    }

    [Fact]
    public void ThrowIfInvalid_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.ThrowIfInvalid());
    }

    #endregion
}
