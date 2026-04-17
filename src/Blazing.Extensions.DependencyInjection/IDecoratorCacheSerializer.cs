namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Serialization seam used by <see cref="DistributedCacheDecoratorCache"/> to convert cached
/// values to and from the raw <c>byte[]</c> format required by
/// <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>.
/// </summary>
/// <remarks>
/// The default implementation <see cref="SystemTextJsonDecoratorCacheSerializer"/> uses
/// <c>System.Text.Json</c> reflection-based serialization. For AOT / trimming scenarios,
/// replace it with a source-generated <c>JsonSerializerContext</c>-based implementation and
/// register it before calling <c>services.Register()</c>:
/// <code>
/// services.AddSingleton&lt;IDecoratorCacheSerializer, MyAotSerializer&gt;();
/// services.Register();
/// </code>
/// </remarks>
public interface IDecoratorCacheSerializer
{
    /// <summary>Serializes <paramref name="value"/> to a byte array.</summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize. May be <see langword="null"/>.</param>
    /// <returns>The serialized bytes.</returns>
    byte[] Serialize<T>(T? value);

    /// <summary>Deserializes a value of type <typeparamref name="T"/> from <paramref name="bytes"/>.</summary>
    /// <typeparam name="T">The expected type of the deserialized value.</typeparam>
    /// <param name="bytes">The bytes to deserialize.</param>
    /// <returns>The deserialized value, or <see langword="default"/> if <paramref name="bytes"/> is empty.</returns>
    T? Deserialize<T>(byte[] bytes);
}
