using System.Globalization;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace TaskFlow.Localization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly string _resourcesPath;
    private readonly string _cultureName;
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();

    public JsonStringLocalizer(string resourcesPath, string cultureName)
    {
        _resourcesPath = resourcesPath;
        _cultureName = cultureName ?? CultureInfo.CurrentUICulture.Name;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = GetString(name);
            var value = format == null ? name : string.Format(CultureInfo.CurrentCulture, format, arguments);
            return new LocalizedString(name, value, format == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var dict = LoadDictionary(_cultureName);
        if (dict == null) return Array.Empty<LocalizedString>();
        return dict.Select(kv => new LocalizedString(kv.Key, kv.Value, false));
    }

    private string? GetString(string name)
    {
        var dict = LoadDictionary(_cultureName);
        if (dict != null && dict.TryGetValue(name, out var value))
            return value;
        var baseName = _cultureName.Length > 2 ? _cultureName[..2] : _cultureName;
        if (baseName != _cultureName)
        {
            dict = LoadDictionary(baseName);
            if (dict != null && dict.TryGetValue(name, out var fallback))
                return fallback;
        }
        dict = LoadDictionary("pl");
        if (dict != null && dict.TryGetValue(name, out var pl))
            return pl;
        return null;
    }

    private Dictionary<string, string>? LoadDictionary(string culture)
    {
        var key = culture;
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var path = Path.Combine(_resourcesPath, $"{culture}.json");
        if (!File.Exists(path))
            return _cache.GetOrAdd(key, _ => new Dictionary<string, string>());

        try
        {
            var json = File.ReadAllText(path);
            var flat = Flatten(JsonSerializer.Deserialize<JsonElement>(json));
            _cache.TryAdd(key, flat);
            return flat;
        }
        catch
        {
            return _cache.GetOrAdd(key, _ => new Dictionary<string, string>());
        }
    }

    private static Dictionary<string, string> Flatten(JsonElement element, string prefix = "")
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        FlattenRecursive(element, prefix, result);
        return result;
    }

    private static void FlattenRecursive(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    FlattenRecursive(prop.Value, key, result);
                }
                break;
            case JsonValueKind.String:
                result[prefix] = element.GetString() ?? "";
                break;
            default:
                break;
        }
    }
}
