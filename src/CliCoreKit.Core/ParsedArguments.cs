namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Represents parsed command-line arguments.
/// </summary>
public sealed class ParsedArguments
{
    private readonly Dictionary<string, List<string>> _options = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _positional = new();

    /// <summary>
    /// Gets all positional arguments (non-option arguments).
    /// </summary>
    public IReadOnlyList<string> Positional => _positional.AsReadOnly();

    /// <summary>
    /// Gets all option names.
    /// </summary>
    public IReadOnlyCollection<string> OptionNames => _options.Keys;

    /// <summary>
    /// Adds an option with its value(s).
    /// </summary>
    public void AddOption(string name, string? value = null)
    {
        if (!_options.ContainsKey(name))
        {
            _options[name] = new List<string>();
        }

        if (value != null)
        {
            _options[name].Add(value);
        }
    }

    /// <summary>
    /// Adds a positional argument.
    /// </summary>
    public void AddPositional(string value)
    {
        _positional.Add(value);
    }

    /// <summary>
    /// Checks if an option is present.
    /// </summary>
    public bool HasOption(string name) => _options.ContainsKey(name);

    /// <summary>
    /// Gets the value of an option.
    /// </summary>
    public string? GetOptionValue(string name)
    {
        return _options.TryGetValue(name, out var values) && values.Count > 0
            ? values[0]
            : null;
    }

    /// <summary>
    /// Gets all values of an option (for multi-value options).
    /// </summary>
    public IReadOnlyList<string> GetOptionValues(string name)
    {
        return _options.TryGetValue(name, out var values)
            ? values.AsReadOnly()
            : Array.Empty<string>();
    }

    /// <summary>
    /// Gets a positional argument by index.
    /// </summary>
    public string? GetPositional(int index)
    {
        return index >= 0 && index < _positional.Count ? _positional[index] : null;
    }

    /// <summary>
    /// Tries to get an option value as a specific type.
    /// </summary>
    public bool TryGetValue<T>(string name, out T? value)
    {
        value = default;
        var stringValue = GetOptionValue(name);

        if (stringValue == null)
        {
            return false;
        }

        try
        {
            value = (T)Convert.ChangeType(stringValue, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
