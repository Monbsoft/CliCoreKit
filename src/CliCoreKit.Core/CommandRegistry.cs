namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Registry for managing command definitions.
/// </summary>
public sealed class CommandRegistry
{
    private readonly Dictionary<string, CommandDefinition> _commands = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all registered commands.
    /// </summary>
    public IReadOnlyCollection<CommandDefinition> Commands => _commands.Values;

    /// <summary>
    /// Registers a command.
    /// </summary>
    public void Register(CommandDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_commands.ContainsKey(definition.Name))
        {
            throw new InvalidOperationException($"Command '{definition.Name}' is already registered.");
        }

        _commands[definition.Name] = definition;

        // Also register aliases
        foreach (var alias in definition.Aliases)
        {
            if (_commands.ContainsKey(alias))
            {
                throw new InvalidOperationException($"Command alias '{alias}' is already registered.");
            }
            _commands[alias] = definition;
        }
    }

    /// <summary>
    /// Tries to find a command by name.
    /// </summary>
    public bool TryGetCommand(string name, out CommandDefinition? definition)
    {
        return _commands.TryGetValue(name, out definition);
    }

    /// <summary>
    /// Gets a command by name.
    /// </summary>
    public CommandDefinition GetCommand(string name)
    {
        if (!_commands.TryGetValue(name, out var definition))
        {
            throw new InvalidOperationException($"Command '{name}' not found.");
        }

        return definition;
    }

    /// <summary>
    /// Gets all root commands (commands without a parent).
    /// </summary>
    public IEnumerable<CommandDefinition> GetRootCommands()
    {
        return _commands.Values
            .Where(c => string.IsNullOrEmpty(c.Parent))
            .DistinctBy(c => c.Name);
    }

    /// <summary>
    /// Gets subcommands of a parent command.
    /// </summary>
    public IEnumerable<CommandDefinition> GetSubcommands(string parentName)
    {
        return _commands.Values
            .Where(c => string.Equals(c.Parent, parentName, StringComparison.OrdinalIgnoreCase))
            .DistinctBy(c => c.Name);
    }
}
