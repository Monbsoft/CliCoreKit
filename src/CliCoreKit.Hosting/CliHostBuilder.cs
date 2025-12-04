using Microsoft.Extensions.DependencyInjection;
using Monbsoft.CliCoreKit.Core;
using Monbsoft.CliCoreKit.Core.Middleware;

namespace Monbsoft.CliCoreKit.Hosting;

/// <summary>
/// Builder for configuring CLI applications with IHost integration.
/// </summary>
public sealed class CliHostBuilder
{
    private readonly IServiceCollection _services;
    private readonly CommandRegistry _registry;
    private readonly MiddlewarePipeline _pipeline;
    private readonly List<Action<IServiceProvider, CommandRegistry>> _commandConfigurations = new();

    public CliHostBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _registry = new CommandRegistry();
        _pipeline = new MiddlewarePipeline();
    }

    /// <summary>
    /// Registers a command.
    /// </summary>
    public CliHostBuilder AddCommand<TCommand>(string name, string? description = null, string[]? aliases = null)
        where TCommand : class, ICommand
    {
        _services.AddTransient<TCommand>();

        var definition = new CommandDefinition
        {
            Name = name,
            Description = description,
            Aliases = aliases ?? Array.Empty<string>(),
            CommandType = typeof(TCommand)
        };

        _registry.Register(definition);
        return this;
    }

    /// <summary>
    /// Registers a subcommand.
    /// </summary>
    public CliHostBuilder AddSubCommand<TCommand>(string name, string parent, string? description = null)
        where TCommand : class, ICommand
    {
        _services.AddTransient<TCommand>();

        var definition = new CommandDefinition
        {
            Name = name,
            Parent = parent,
            Description = description,
            CommandType = typeof(TCommand)
        };

        _registry.Register(definition);
        return this;
    }

    /// <summary>
    /// Adds middleware to the pipeline.
    /// </summary>
    public CliHostBuilder UseMiddleware<TMiddleware>() where TMiddleware : class, ICommandMiddleware
    {
        _services.AddSingleton<TMiddleware>();
        return this;
    }

    /// <summary>
    /// Adds middleware instance to the pipeline.
    /// </summary>
    public CliHostBuilder UseMiddleware(ICommandMiddleware middleware)
    {
        _pipeline.Use(middleware);
        return this;
    }

    /// <summary>
    /// Adds validation middleware.
    /// </summary>
    public CliHostBuilder UseValidation()
    {
        _pipeline.Use(new ValidationMiddleware());
        return this;
    }

    /// <summary>
    /// Configures commands using dependency injection.
    /// </summary>
    public CliHostBuilder ConfigureCommands(Action<IServiceProvider, CommandRegistry> configure)
    {
        _commandConfigurations.Add(configure);
        return this;
    }

    public void Build(IServiceProvider serviceProvider)
    {
        // Execute all command configurations
        foreach (var configure in _commandConfigurations)
        {
            configure(serviceProvider, _registry);
        }

        // Register core services
        _services.AddSingleton(_registry);
        _services.AddSingleton(_pipeline);
        _services.AddSingleton(new CommandRouter(_registry));
        _services.AddSingleton(sp => new CliApplication(
            sp.GetRequiredService<CommandRegistry>(),
            sp.GetRequiredService<CommandRouter>(),
            sp.GetRequiredService<MiddlewarePipeline>(),
            type => (ICommand)sp.GetRequiredService(type)
        ));
    }
}
