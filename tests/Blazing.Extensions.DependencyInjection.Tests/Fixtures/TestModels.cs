namespace Blazing.Extensions.DependencyInjection.Tests.Fixtures;

// ---------------------------------------------------------------------------
// Core service interfaces and implementations
// ---------------------------------------------------------------------------

/// <summary>Simple test service interface.</summary>
public interface ITestService
{
    string GetMessage();
}

/// <summary>Primary implementation of <see cref="ITestService"/>.</summary>
public class TestService : ITestService
{
    public string GetMessage() => "Hello from TestService";
}

/// <summary>Alternate implementation of <see cref="ITestService"/>.</summary>
public class AnotherTestService : ITestService
{
    public string GetMessage() => "Hello from AnotherTestService";
}

/// <summary>Service that depends on <see cref="ITestService"/>.</summary>
public interface IDependentService
{
    string GetValue();
}

/// <summary>Implementation of <see cref="IDependentService"/> using primary constructor.</summary>
public class DependentService(ITestService testService) : IDependentService
{
    public string GetValue() => $"Dependent: {testService.GetMessage()}";
}

// ---------------------------------------------------------------------------
// Keyed service types
// ---------------------------------------------------------------------------

/// <summary>Interface for keyed service tests.</summary>
public interface IKeyedTestService
{
    string GetMessage();
}

/// <summary>Primary keyed implementation.</summary>
public class PrimaryKeyedService : IKeyedTestService
{
    public string GetMessage() => "Primary Service";
}

/// <summary>Secondary keyed implementation.</summary>
public class SecondaryKeyedService : IKeyedTestService
{
    public string GetMessage() => "Secondary Service";
}

/// <summary>Database keyed implementation.</summary>
public class DatabaseService : IKeyedTestService
{
    public string GetMessage() => "Database Service";
}

// ---------------------------------------------------------------------------
// Host / container placeholder
// ---------------------------------------------------------------------------

/// <summary>Minimal host class used as a DI container owner in tests.</summary>
public class TestHost { }

// ---------------------------------------------------------------------------
// Disposable service
// ---------------------------------------------------------------------------

/// <summary>Service that tracks its own disposal.</summary>
public class DisposableService : IDisposable
{
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

// ---------------------------------------------------------------------------
// Scoped service for scope tests
// ---------------------------------------------------------------------------

/// <summary>Interface for scoped service tests.</summary>
public interface IScopedTestService
{
    string GetValue();
}

/// <summary>Implementation of <see cref="IScopedTestService"/>.</summary>
public class ScopedTestService : IScopedTestService
{
    public string GetValue() => "Scoped Value";
}

// ---------------------------------------------------------------------------
// Decoratable service for decoration tests
// ---------------------------------------------------------------------------

/// <summary>Interface suitable for decorator tests.</summary>
public interface IDecoratableService
{
    string Execute();
}

/// <summary>Base implementation decorated in tests.</summary>
public class DecoratableService : IDecoratableService
{
    public string Execute() => "base";
}

/// <summary>Decorator that prefixes the inner result.</summary>
public class PrefixDecorator(IDecoratableService inner) : IDecoratableService
{
    public string Execute() => $"[decorated] {inner.Execute()}";
}

// ---------------------------------------------------------------------------
// Open-generic repository for generic service tests
// ---------------------------------------------------------------------------

/// <summary>Generic repository interface.</summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IRepository<T>
{
    T? GetById(int id);
}

/// <summary>In-memory repository implementation.</summary>
/// <typeparam name="T">Entity type.</typeparam>
public class InMemoryRepository<T> : IRepository<T>
{
    public T? GetById(int id) => default;
}

/// <summary>Dummy entity used with <see cref="IRepository{T}"/>.</summary>
public class UserEntity { }

/// <summary>Another dummy entity for generic service tests.</summary>
public class ProductEntity { }
