using System.Collections.Concurrent;
using System.Text.Json;

namespace OpenSpeed.Core.Controller
{
  public interface IKeyValueStore
  {
    T? GetValue<T>(string key, T? defaultValue = default);

    void SetValue<T>(string key, T value);
  }

  public class JsonKeyValueStore : IKeyValueStore
  {
    private readonly Lazy<ConcurrentDictionary<string, object?>> _data;
    private readonly string _directoryPath;
    private readonly string _settingsFilePath;

    public JsonKeyValueStore(string path)
    {
      _directoryPath = path;
      _settingsFilePath = Path.Combine(_directoryPath, "Settings.json");
      _data = new(ValueFactory);
    }

    private ConcurrentDictionary<string, object?> ValueFactory()
    {
      if (!Directory.Exists(_directoryPath))
        Directory.CreateDirectory(_directoryPath);

      return File.Exists(_settingsFilePath) ? JsonSerializer.Deserialize<ConcurrentDictionary<string, object?>>(File.ReadAllText(_settingsFilePath)) ?? new() : new();
    }

    public T? GetValue<T>(string key, T? defaultValue = default)
    {
      if (!_data.Value.TryGetValue(key, out var value))
        return defaultValue ?? default;
      
      if (value is JsonElement element)
        return element.Deserialize<T>();
      
      return (T?)value;
    }

    public void SetValue<T>(string key, T value)
    {
      _data.Value[key] = value;
      File.WriteAllText(_settingsFilePath, JsonSerializer.Serialize(_data.Value, new JsonSerializerOptions { WriteIndented = true }));
    }
  }
}