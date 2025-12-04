namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Defines metadata for a command.
/// </summary>
public sealed class CommandDefinition
{
    /// <summary>
    /// Gets or sets the command name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the command description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets command aliases.
    /// </summary>
    public string[] Aliases { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the parent command (for subcommands).
    /// </summary>
    public string? Parent { get; init; }

    /// <summary>
    /// Gets or sets the command implementation type.
    /// </summary>
    public required Type CommandType { get; init; }

    /// <summary>
    /// Gets or sets option definitions for this command.
    /// </summary>
    public List<OptionDefinition> Options { get; init; } = new();

    /// <summary>
    /// Gets or sets whether this command is hidden from help.
    /// </summary>
    public bool IsHidden { get; init; }

    /// <summary>
    /// Checks if this command matches a given name.
    /// </summary>
    public bool Matches(string name)
    {
        return string.Equals(Name, name, StringComparison.OrdinalIgnoreCase) ||
               Aliases.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Defines metadata for a command option.
/// </summary>
public sealed class OptionDefinition
{
    /// <summary>
    /// Gets or sets the option name (long form, without --)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the short name (single character, without -)
    /// </summary>
    public char? ShortName { get; init; }

    /// <summary>
    /// Gets or sets the option description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets whether this option is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Gets or sets the value type.
    /// </summary>
    public Type? ValueType { get; init; }

    /// <summary>
    /// Gets or sets whether this option can have multiple values.
    /// </summary>
    public bool AllowMultiple { get; init; }
}
