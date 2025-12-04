using Monbsoft.CliCoreKit.Core.Middleware;

namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Main entry point for CLI application execution.
/// </summary>
public sealed class CliApplication
{
    private readonly CommandRegistry _registry;
    private readonly CommandRouter _router;
    private readonly MiddlewarePipeline _pipeline;
    private readonly Func<Type, ICommand>? _commandFactory;

    public CliApplication(
        CommandRegistry registry,
        CommandRouter? router = null,
        MiddlewarePipeline? pipeline = null,
        Func<Type, ICommand>? commandFactory = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _router = router ?? new CommandRouter(registry);
        _pipeline = pipeline ?? new MiddlewarePipeline();
        _commandFactory = commandFactory;
    }

    /// <summary>
    /// Runs the CLI application.
    /// </summary>
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for global help option first
            if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h"))
            {
                ShowHelp();
                return 0;
            }

            var route = _router.Route(args);

            if (route.CommandDefinition == null)
            {
                Console.Error.WriteLine("No command specified. Use --help for available commands.");
                return 1;
            }

            var parsedArgs = _router.ParseArguments(route.RemainingArgs);

            // Check if help is requested AND the command has no subcommands
            // If it has subcommands, show the system-generated help
            // Otherwise, let the command handle --help itself for custom help
            var hasSubCommands = _registry.Commands
                .Any(c => string.Equals(c.Parent, route.CommandDefinition.Name, StringComparison.OrdinalIgnoreCase));
            
            if ((parsedArgs.HasOption("help") || parsedArgs.HasOption("h")) && hasSubCommands)
            {
                ShowCommandHelp(route.CommandDefinition, route.CommandPath);
                return 0;
            }

            var context = new CommandContext
            {
                Arguments = parsedArgs,
                RawArgs = args,
                CommandName = string.Join(" ", route.CommandPath)
            };

            // Add command definition to context for middleware
            context.Data["CommandDefinition"] = route.CommandDefinition;

            // Build and execute pipeline
            var handler = _pipeline.Build(async (ctx, ct) =>
            {
                var command = CreateCommand(route.CommandDefinition.CommandType);
                return await command.ExecuteAsync(ctx, ct);
            });

            return await handler(context, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private void ShowHelp()
    {
        Console.WriteLine("Usage: [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Available commands:");
        Console.WriteLine();

        var rootCommands = _registry.Commands
            .Where(c => string.IsNullOrEmpty(c.Parent))
            .OrderBy(c => c.Name);

        foreach (var cmd in rootCommands)
        {
            var description = !string.IsNullOrEmpty(cmd.Description) ? cmd.Description : "No description";
            Console.WriteLine($"  {cmd.Name,-20} {description}");

            // Show subcommands
            var subCommands = _registry.Commands
                .Where(c => string.Equals(c.Parent, cmd.Name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Name);

            foreach (var sub in subCommands)
            {
                var subDescription = !string.IsNullOrEmpty(sub.Description) ? sub.Description : "No description";
                Console.WriteLine($"    {sub.Name,-18} {subDescription}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, --help          Show this help message");
        Console.WriteLine();
        Console.WriteLine("Run '[command] --help' for more information on a command.");
    }

    private void ShowCommandHelp(CommandDefinition command, string[] commandPath)
    {
        var fullCommandName = string.Join(" ", commandPath);
        
        // Check if this command has subcommands
        var subCommands = _registry.Commands
            .Where(c => string.Equals(c.Parent, command.Name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name)
            .ToList();

        if (subCommands.Any())
        {
            // Show help for a command with subcommands
            Console.WriteLine($"Usage: {fullCommandName} [subcommand] [options]");
            Console.WriteLine();
            
            if (!string.IsNullOrEmpty(command.Description))
            {
                Console.WriteLine(command.Description);
                Console.WriteLine();
            }

            Console.WriteLine("Available subcommands:");
            Console.WriteLine();

            foreach (var sub in subCommands)
            {
                var subDescription = !string.IsNullOrEmpty(sub.Description) ? sub.Description : "No description";
                Console.WriteLine($"  {sub.Name,-20} {subDescription}");
            }

            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help          Show this help message");
            Console.WriteLine();
            Console.WriteLine($"Run '{fullCommandName} [subcommand] --help' for more information on a subcommand.");
        }
        else
        {
            // Show basic help for a leaf command (without subcommands)
            Console.WriteLine($"Usage: {fullCommandName} [options]");
            Console.WriteLine();
            
            if (!string.IsNullOrEmpty(command.Description))
            {
                Console.WriteLine(command.Description);
                Console.WriteLine();
            }

            if (command.Options.Any())
            {
                Console.WriteLine("Options:");
                foreach (var option in command.Options)
                {
                    var optionDisplay = option.ShortName.HasValue
                        ? $"-{option.ShortName}, --{option.Name}"
                        : $"--{option.Name}";
                    
                    var description = !string.IsNullOrEmpty(option.Description) 
                        ? option.Description 
                        : "No description";
                    
                    var required = option.IsRequired ? " (required)" : "";
                    Console.WriteLine($"  {optionDisplay,-25} {description}{required}");
                }
                Console.WriteLine($"  {"-h, --help",-25} Show this help message");
            }
            else
            {
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help          Show this help message");
            }
        }
    }

    private ICommand CreateCommand(Type commandType)
    {
        if (_commandFactory != null)
        {
            return _commandFactory(commandType);
        }

        // Default: use Activator
        return (ICommand)(Activator.CreateInstance(commandType)
            ?? throw new InvalidOperationException($"Failed to create instance of {commandType}"));
    }
}
