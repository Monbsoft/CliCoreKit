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

            // Map positional arguments to their names based on command definition
            var orderedArgs = route.CommandDefinition.Arguments.OrderBy(a => a.Position).ToList();
            for (int i = 0; i < Math.Min(orderedArgs.Count, parsedArgs.Positional.Count); i++)
            {
                parsedArgs.AddNamedArgument(orderedArgs[i].Name, parsedArgs.Positional[i]);
            }

            // Map short option names to long names
            foreach (var option in route.CommandDefinition.Options)
            {
                if (option.ShortName.HasValue)
                {
                    // If the short name was used (e.g., -g), map it to the long name (e.g., greeting)
                    if (parsedArgs.HasOption(option.ShortName.ToString()!))
                    {
                        var values = parsedArgs.GetOptionValues(option.ShortName.ToString()!);
                        foreach (var value in values)
                        {
                            parsedArgs.AddOption(option.Name, value);
                        }
                        
                        // If it's a flag (no value), also add it with the long name
                        if (values.Count == 0)
                        {
                            parsedArgs.AddOption(option.Name);
                        }
                    }
                }
            }

            // Check if help is requested
            var helpRequested = parsedArgs.HasOption("help") || parsedArgs.HasOption("h");
            
            // If help is requested and help is not disabled
            if (helpRequested && !route.CommandDefinition.DisableHelp)
            {
                ShowCommandHelp(route.CommandDefinition, route.CommandPath);
                return 0;
            }

            var context = new CommandContext(
                parsedArgs,
                args,
                string.Join(" ", route.CommandPath),
                route.CommandDefinition
            );

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

        ShowCommandsRecursive(null, 0);

        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, --help          Show this help message");
        Console.WriteLine();
        Console.WriteLine("Run '[command] --help' for more information on a command.");
    }

    private void ShowCommandsRecursive(string? parent, int indentLevel)
    {
        var commands = _registry.Commands
            .Where(c => string.Equals(c.Parent, parent, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name);

        foreach (var cmd in commands)
        {
            var indent = new string(' ', indentLevel * 2);
            var description = !string.IsNullOrEmpty(cmd.Description) ? cmd.Description : "No description";
            var nameWidth = 20 - (indentLevel * 2);
            var paddedName = cmd.Name.PadRight(nameWidth);
            Console.WriteLine($"  {indent}{paddedName} {description}");

            // Show child commands
            ShowCommandsRecursive(cmd.Name, indentLevel + 1);
        }
    }

    private void ShowCommandHelp(CommandDefinition command, string[] commandPath)
    {
        var fullCommandName = string.Join(" ", commandPath);
        
        // Check if this command has child commands
        var childCommands = _registry.Commands
            .Where(c => string.Equals(c.Parent, command.Name, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name)
            .ToList();

        if (childCommands.Any())
        {
            // Show help for a command with child commands
            Console.WriteLine($"Usage: {fullCommandName} <command> [options]");
            Console.WriteLine();
            
            if (!string.IsNullOrEmpty(command.Description))
            {
                Console.WriteLine(command.Description);
                Console.WriteLine();
            }

            Console.WriteLine("Commands:");
            Console.WriteLine();

            foreach (var child in childCommands)
            {
                var childDescription = !string.IsNullOrEmpty(child.Description) ? child.Description : "No description";
                Console.WriteLine($"  {child.Name,-20} {childDescription}");
            }

            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, --help          Show this help message");
            Console.WriteLine();
            Console.WriteLine($"Run '{fullCommandName} <command> --help' for more information on a command.");
        }
        else
        {
            // Show basic help for a leaf command (without child commands)
            var usageParts = new List<string> { fullCommandName };
            
            // Add arguments to usage
            if (command.Arguments.Any())
            {
                foreach (var arg in command.Arguments.OrderBy(a => a.Position))
                {
                    var argDisplay = arg.IsRequired ? $"<{arg.Name}>" : $"[{arg.Name}]";
                    usageParts.Add(argDisplay);
                }
            }
            
            usageParts.Add("[options]");
            Console.WriteLine($"Usage: {string.Join(" ", usageParts)}");
            Console.WriteLine();
            
            if (!string.IsNullOrEmpty(command.Description))
            {
                Console.WriteLine(command.Description);
                Console.WriteLine();
            }

            // Show arguments
            if (command.Arguments.Any())
            {
                Console.WriteLine("Arguments:");
                foreach (var arg in command.Arguments.OrderBy(a => a.Position))
                {
                    var argName = arg.Name;
                    var argDesc = !string.IsNullOrEmpty(arg.Description) ? arg.Description : "No description";
                    var required = arg.IsRequired ? " (required)" : "";
                    var typeInfo = arg.ValueType != typeof(string) ? $" [{GetTypeName(arg.ValueType)}]" : "";
                    var defaultInfo = arg.DefaultValue != null ? $" (default: {arg.DefaultValue})" : "";
                    Console.WriteLine($"  {argName,-25} {argDesc}{typeInfo}{required}{defaultInfo}");
                }
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
                    
                    if (option.HasValue && option.ValueType != typeof(bool))
                    {
                        var typeName = GetTypeName(option.ValueType);
                        optionDisplay += $" <{typeName.ToLower()}>";
                    }
                    
                    var description = !string.IsNullOrEmpty(option.Description) 
                        ? option.Description 
                        : "No description";
                    
                    var required = option.IsRequired ? " (required)" : "";
                    var defaultInfo = option.DefaultValue != null ? $" (default: {option.DefaultValue})" : "";
                    Console.WriteLine($"  {optionDisplay,-30} {description}{required}{defaultInfo}");
                }
                Console.WriteLine($"  {"-h, --help",-30} Show this help message");
            }
            else
            {
                Console.WriteLine("Options:");
                Console.WriteLine("  -h, --help          Show this help message");
            }
        }
    }

    private static string GetTypeName(Type? type)
    {
        if (type == null) return "string";
        
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        return underlyingType.Name.ToLower() switch
        {
            "int32" => "int",
            "int64" => "long",
            "single" => "float",
            "double" => "double",
            "boolean" => "bool",
            "string" => "string",
            _ => underlyingType.Name
        };
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
