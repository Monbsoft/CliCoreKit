namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Provides context for command execution.
/// </summary>
public sealed class CommandContext
{
    private readonly ParsedArguments _arguments;

    /// <summary>
    /// Initializes a new instance of CommandContext.
    /// </summary>
    public CommandContext(ParsedArguments arguments, string[] rawArgs, string commandName, CommandDefinition? commandDefinition = null)
    {
        _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        RawArgs = rawArgs ?? throw new ArgumentNullException(nameof(rawArgs));
        CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        CommandDefinition = commandDefinition;
    }

    /// <summary>
    /// Gets the raw command line arguments.
    /// </summary>
    public string[] RawArgs { get; }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// Gets the command definition (for accessing argument/option metadata).
    /// </summary>
    public CommandDefinition? CommandDefinition { get; }

    /// <summary>
    /// Gets additional data that can be used by commands.
    /// </summary>
    public Dictionary<string, object> Data { get; } = new();

    /// <summary>
    /// Gets all positional arguments (non-option arguments).
    /// </summary>
    public IReadOnlyList<string> Positional => _arguments.Positional;

    /// <summary>
    /// Gets a named argument as a specific type.
    /// Uses the default value from the argument definition if not provided.
    /// </summary>
    public T? GetArgument<T>(string name)
    {
        // Try to get the named argument directly
        var stringValue = _arguments.GetNamedArgument(name);
        
        if (stringValue != null)
        {
            try
            {
                return ParsedArguments.ConvertValue<T>(stringValue);
            }
            catch
            {
                // Fall through to default value
            }
        }

        // Use default value from argument definition
        if (CommandDefinition?.Arguments.FirstOrDefault(a => 
            string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)) is { } argDef)
        {
            if (argDef.DefaultValue != null)
            {
                try
                {
                    return (T)Convert.ChangeType(argDef.DefaultValue, typeof(T));
                }
                catch
                {
                    // Unable to convert, return default
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Gets an option value as a specific type.
    /// Uses the default value from the option definition if not provided.
    /// </summary>
    public T? GetOption<T>(string name)
    {
        // Try to get the value
        if (_arguments.TryGetValue<T>(name, out var value))
        {
            return value;
        }

        // Special handling for bool - if option exists without value, it's true
        if (typeof(T) == typeof(bool) && _arguments.HasOption(name))
        {
            return (T)(object)true;
        }

        // Use default value from option definition
        if (CommandDefinition?.Options.FirstOrDefault(o => 
            string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase)) is { } optDef)
        {
            if (optDef.DefaultValue != null)
            {
                try
                {
                    var targetType = typeof(T);
                    var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    
                    var convertedValue = Convert.ChangeType(optDef.DefaultValue, underlyingType);
                    return (T)convertedValue;
                }
                catch
                {
                    // Unable to convert, return default
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Tries to get an option value as a specific type.
    /// </summary>
    public bool TryGetOption<T>(string name, out T? value)
    {
        return _arguments.TryGetValue(name, out value);
    }

    /// <summary>
    /// Gets all values of an option as a specific type.
    /// </summary>
    public IReadOnlyList<T> GetOptionValues<T>(string name)
    {
        return _arguments.GetOptionValues<T>(name);
    }

    /// <summary>
    /// Checks if an option is present.
    /// </summary>
    public bool HasOption(string name)
    {
        return _arguments.HasOption(name);
    }

    /// <summary>
    /// Gets the option value as string (for backward compatibility).
    /// </summary>
    public string? GetOptionValue(string name)
    {
        return _arguments.GetOptionValue(name);
    }

    // Internal access for the framework
    internal ParsedArguments Arguments => _arguments;
}
