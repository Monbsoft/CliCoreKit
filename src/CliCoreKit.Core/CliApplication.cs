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
            var route = _router.Route(args);

            if (route.CommandDefinition == null)
            {
                Console.Error.WriteLine("No command specified. Use --help for available commands.");
                return 1;
            }

            var parsedArgs = _router.ParseArguments(route.RemainingArgs);

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
