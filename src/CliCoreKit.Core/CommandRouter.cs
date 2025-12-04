namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Routes command-line arguments to the appropriate command.
/// </summary>
public sealed class CommandRouter
{
    private readonly CommandRegistry _registry;
    private readonly ArgumentParser _parser;

    public CommandRouter(CommandRegistry registry, ArgumentParser? parser = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _parser = parser ?? new ArgumentParser();
    }

    /// <summary>
    /// Routes the arguments to find the appropriate command and parsed arguments.
    /// </summary>
    public CommandRoute Route(string[] args)
    {
        if (args.Length == 0)
        {
            return new CommandRoute
            {
                CommandDefinition = null,
                RemainingArgs = args,
                CommandPath = Array.Empty<string>()
            };
        }

        var commandPath = new List<string>();
        var currentArgs = args;
        CommandDefinition? currentCommand = null;
        var argIndex = 0;

        // Try to find the deepest matching command
        while (argIndex < currentArgs.Length)
        {
            var potentialCommand = currentArgs[argIndex];

            // Check if it's an option rather than a command
            if (potentialCommand.StartsWith("-") || potentialCommand.StartsWith("/"))
            {
                break;
            }

            // Try to find command
            var parentPath = string.Join(".", commandPath);
            var found = false;

            foreach (var cmd in _registry.Commands)
            {
                var expectedParent = commandPath.Count == 0 ? null : parentPath;

                if (cmd.Matches(potentialCommand) &&
                    string.Equals(cmd.Parent, expectedParent, StringComparison.OrdinalIgnoreCase))
                {
                    currentCommand = cmd;
                    commandPath.Add(cmd.Name);
                    argIndex++;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                break;
            }
        }

        var remainingArgs = currentArgs.Skip(argIndex).ToArray();

        return new CommandRoute
        {
            CommandDefinition = currentCommand,
            RemainingArgs = remainingArgs,
            CommandPath = commandPath.ToArray()
        };
    }

    /// <summary>
    /// Parses the remaining arguments after routing.
    /// </summary>
    public ParsedArguments ParseArguments(string[] args)
    {
        return _parser.Parse(args);
    }
}

/// <summary>
/// Represents the result of command routing.
/// </summary>
public sealed class CommandRoute
{
    /// <summary>
    /// Gets the matched command definition, or null if no command was found.
    /// </summary>
    public CommandDefinition? CommandDefinition { get; init; }

    /// <summary>
    /// Gets the remaining arguments after command extraction.
    /// </summary>
    public required string[] RemainingArgs { get; init; }

    /// <summary>
    /// Gets the full command path (e.g., ["git", "remote", "add"]).
    /// </summary>
    public required string[] CommandPath { get; init; }
}
