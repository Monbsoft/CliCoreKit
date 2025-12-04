namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Represents parsed command-line arguments.
/// </summary>
public sealed class ParsedArguments
{
    private readonly Dictionary<string, List<string>> _options = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _positional = new();
    private readonly Dictionary<string, string> _namedArguments = new(StringComparer.OrdinalIgnoreCase);

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
    /// Adds a named argument directly (no position mapping needed).
    /// </summary>
    public void AddNamedArgument(string name, string value)
    {
        _namedArguments[name] = value;
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
    /// Gets a named argument value.
    /// </summary>
    internal string? GetNamedArgument(string name)
    {
        return _namedArguments.TryGetValue(name, out var value) ? value : null;
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
            value = ConvertValue<T>(stringValue);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets an option value as a specific type with a default value.
    /// </summary>
    public T GetOption<T>(string name, T defaultValue = default!)
    {
        // Special handling for bool - if option exists without value, it's true
        if (typeof(T) == typeof(bool))
        {
            if (HasOption(name))
            {
                var stringValue = GetOptionValue(name);
                if (string.IsNullOrEmpty(stringValue))
                {
                    return (T)(object)true; // Flag option present = true
                }
                try
                {
                    return (T)(object)ConvertValue<bool>(stringValue);
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        if (TryGetValue<T>(name, out var value))
        {
            return value!;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets a positional argument as a specific type (internal use only).
    /// </summary>
    internal T? GetArgument<T>(int index)
    {
        var stringValue = GetPositional(index);
        
        if (stringValue == null)
        {
            return default(T);
        }

        try
        {
            return ConvertValue<T>(stringValue);
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Gets a named argument as a specific type.
    /// </summary>
    internal T? GetNamedArgument<T>(string name)
    {
        var stringValue = GetNamedArgument(name);
        
        if (stringValue == null)
        {
            return default(T);
        }

        try
        {
            return ConvertValue<T>(stringValue);
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Gets all values of an option as a specific type.
    /// </summary>
    public IReadOnlyList<T> GetOptionValues<T>(string name)
    {
        if (!_options.TryGetValue(name, out var values))
        {
            return Array.Empty<T>();
        }

        var result = new List<T>();
        foreach (var value in values)
        {
            try
            {
                result.Add(ConvertValue<T>(value));
            }
            catch
            {
                // Skip invalid values
            }
        }

        return result.AsReadOnly();
    }

    internal static T ConvertValue<T>(string value)
    {
        var targetType = typeof(T);

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            targetType = underlyingType;
        }

        // Special handling for bool
        if (targetType == typeof(bool))
        {
            if (bool.TryParse(value, out var boolResult))
            {
                return (T)(object)boolResult;
            }
            // Treat presence as true, or values like "1", "yes", "on" as true
            if (string.IsNullOrEmpty(value) || 
                value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                return (T)(object)true;
            }
            return (T)(object)false;
        }

        // Handle enums
        if (targetType.IsEnum)
        {
            return (T)Enum.Parse(targetType, value, ignoreCase: true);
        }

        // Special handling for floating-point types with invariant culture
        if (targetType == typeof(double))
        {
            return (T)(object)double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        if (targetType == typeof(float))
        {
            return (T)(object)float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        if (targetType == typeof(decimal))
        {
            return (T)(object)decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Use Convert for other primitive types
        return (T)Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
    }
}
