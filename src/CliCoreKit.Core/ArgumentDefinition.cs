namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Defines metadata for a command argument (positional parameter).
/// </summary>
public sealed class ArgumentDefinition
{
    /// <summary>
    /// Gets or sets the argument name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the argument description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the value type (e.g., string, int, bool).
    /// </summary>
    public Type ValueType { get; init; } = typeof(string);

    /// <summary>
    /// Gets or sets whether this argument is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets or sets the default value if not provided.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Gets or sets the position of this argument (0-based).
    /// </summary>
    public int Position { get; init; }
}
