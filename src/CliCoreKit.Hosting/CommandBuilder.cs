using Microsoft.Extensions.DependencyInjection;
using Monbsoft.CliCoreKit.Core;

namespace Monbsoft.CliCoreKit.Hosting;

/// <summary>
/// Fluent builder for configuring command definitions.
/// </summary>
public sealed class CommandBuilder
{
    private readonly IServiceCollection _services;
    private readonly CommandRegistry _registry;
    private readonly CommandDefinition _definition;

    internal CommandBuilder(IServiceCollection services, CommandRegistry registry, CommandDefinition definition)
    {
        _services = services;
        _registry = registry;
        _definition = definition;
    }

    /// <summary>
    /// Adds an argument (positional parameter) to the command.
    /// </summary>
    /// <typeparam name="T">The argument type (default: string)</typeparam>
    /// <param name="name">The argument name</param>
    /// <param name="description">The argument description</param>
    /// <param name="required">Whether the argument is required</param>
    /// <param name="defaultValue">The default value if not provided</param>
    public CommandBuilder AddArgument<T>(string name, string? description = null, bool required = false, T? defaultValue = default)
    {
        var arg = new ArgumentDefinition
        {
            Name = name,
            Description = description,
            IsRequired = required,
            ValueType = typeof(T),
            DefaultValue = defaultValue,
            Position = _definition.Arguments.Count
        };

        _definition.Arguments.Add(arg);
        return this;
    }

    /// <summary>
    /// Adds a string argument (positional parameter) to the command.
    /// </summary>
    public CommandBuilder AddArgument(string name, string? description = null, bool required = false, string? defaultValue = null)
    {
        return AddArgument<string>(name, description, required, defaultValue);
    }

    /// <summary>
    /// Adds an option to the command with type safety.
    /// </summary>
    /// <typeparam name="T">The option value type (default: string)</typeparam>
    /// <param name="name">The long option name (without --)</param>
    /// <param name="shortName">The short option name (single character, without -)</param>
    /// <param name="description">The option description</param>
    /// <param name="required">Whether the option is required</param>
    /// <param name="defaultValue">The default value if not provided</param>
    public CommandBuilder AddOption<T>(string name, char? shortName = null, string? description = null, 
        bool required = false, T? defaultValue = default)
    {
        var hasValue = typeof(T) != typeof(bool);
        
        var option = new OptionDefinition
        {
            Name = name,
            ShortName = shortName,
            Description = description,
            IsRequired = required,
            HasValue = hasValue,
            ValueType = typeof(T),
            DefaultValue = defaultValue
        };

        _definition.Options.Add(option);
        return this;
    }

    /// <summary>
    /// Adds a string option to the command.
    /// </summary>
    public CommandBuilder AddOption(string name, char? shortName = null, string? description = null, 
        bool required = false, string? defaultValue = null)
    {
        return AddOption<string>(name, shortName, description, required, defaultValue);
    }

    /// <summary>
    /// Adds a child command.
    /// </summary>
    public CommandBuilder AddCommand<TCommand>(string name, string? description = null)
        where TCommand : class, ICommand
    {
        _services.AddTransient<TCommand>();

        var childDefinition = new CommandDefinition
        {
            Name = name,
            Parent = _definition.Name,
            Description = description,
            CommandType = typeof(TCommand)
        };

        _registry.Register(childDefinition);
        return new CommandBuilder(_services, _registry, childDefinition);
    }

    /// <summary>
    /// Disables help generation for this command.
    /// The command will need to handle --help itself.
    /// </summary>
    public CommandBuilder WithoutHelp()
    {
        _definition.DisableHelp = true;
        return this;
    }
}

