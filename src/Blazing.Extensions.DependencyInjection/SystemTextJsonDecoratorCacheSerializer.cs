using System.Text.Json;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Default <see cref="IDecoratorCacheSerializer"/> implementation using
/// <c>System.Text.Json</c> reflection-based serialization.
/// </summary>
/// <remarks>
/// This implementation is suitable for most scenarios. For Native AOT or aggressive trimming,
/// register a custom <see cref="IDecoratorCacheSerializer"/> backed by a
/// source-generated <c>JsonSerializerContext</c> before calling <c>services.Register()</c>.
/// </remarks>
public sealed class SystemTextJsonDecoratorCacheSerializer : IDecoratorCacheSerializer
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    /// <inheritdoc/>
    public byte[] Serialize<T>(T? value)
        => JsonSerializer.SerializeToUtf8Bytes(value, _options);

    /// <inheritdoc/>
    public T? Deserialize<T>(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0)
            return default;

        return JsonSerializer.Deserialize<T>(bytes, _options);
    }
}
