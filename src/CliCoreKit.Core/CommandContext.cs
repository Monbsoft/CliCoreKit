namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Provides context for command execution.
/// </summary>
public sealed class CommandContext
{
    /// <summary>
    /// Gets the parsed arguments.
    /// </summary>
    public required ParsedArguments Arguments { get; init; }

    /// <summary>
    /// Gets the raw command line arguments.
    /// </summary>
    public required string[] RawArgs { get; init; }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    public required string CommandName { get; init; }

    /// <summary>
    /// Gets additional data that can be used by commands.
    /// </summary>
    public Dictionary<string, object> Data { get; } = new();
}
