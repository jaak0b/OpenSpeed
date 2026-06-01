using OpenSpeed.Core.Controller;

namespace OpenSpeed.Core.UnitTests;

internal sealed class InMemoryKeyValueStore : IKeyValueStore
{
    private readonly Dictionary<string, object?> _data = new();

    public InMemoryKeyValueStore Set(string key, object? value)
    {
        _data[key] = value;
        return this;
    }

    public T? GetValue<T>(string key, T? defaultValue = default)
        => _data.TryGetValue(key, out var v) ? (T?)v : defaultValue;

    public void SetValue<T>(string key, T value) => _data[key] = value;
}
