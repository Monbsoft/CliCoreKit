using AwesomeAssertions;
using Monbsoft.CliCoreKit.Core;
using Monbsoft.CliCoreKit.Core.Middleware;

namespace CliCoreKit.Core.Tests;

public class AutoHelpTests
{
    [Fact]
    public async Task RunAsync_WithHelpOption_ShowsHelpAndReturnsZero()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            Description = "Test command",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "name",
            ShortName = 'n',
            Description = "Your name",
            ValueType = typeof(string)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "test", "--help" });

        // Assert
        exitCode.Should().Be(0);
        var help = output.ToString();
        help.Should().Contain("Usage:");
        help.Should().Contain("test");
        help.Should().Contain("Test command");
        help.Should().Contain("-n, --name");
    }

    [Fact]
    public async Task RunAsync_WithShortHelpOption_ShowsHelp()
    {
        // Arrange
        var registry = new CommandRegistry();
        registry.Register(new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        });

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "test", "-h" });

        // Assert
        exitCode.Should().Be(0);
        var help = output.ToString();
        help.Should().Contain("Usage:");
    }

    [Fact]
    public async Task RunAsync_WithDisableHelp_DoesNotShowAutomaticHelp()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "custom",
            CommandType = typeof(CustomHelpCommand),
            DisableHelp = true
        };
        registry.Register(definition);

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "custom", "--help" });

        // Assert
        exitCode.Should().Be(0);
        var help = output.ToString();
        help.Should().Contain("CUSTOM HELP");
        help.Should().NotContain("Usage:"); // Should not show automatic help
    }

    [Fact]
    public async Task RunAsync_WithArguments_ShowsArgumentsInHelp()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "deploy",
            CommandType = typeof(TestCommand)
        };
        definition.Arguments.Add(new ArgumentDefinition
        {
            Name = "environment",
            Description = "Target environment",
            IsRequired = true,
            ValueType = typeof(string),
            Position = 0
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "deploy", "--help" });

        // Assert
        exitCode.Should().Be(0);
        var help = output.ToString();
        help.Should().Contain("Arguments:");
        help.Should().Contain("environment");
        help.Should().Contain("Target environment");
        help.Should().Contain("(required)");
    }

    [Fact]
    public async Task RunAsync_WithTypedOptions_ShowsTypesInHelp()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "serve",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "port",
            ShortName = 'p',
            Description = "Port number",
            ValueType = typeof(int),
            DefaultValue = 8080
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "serve", "--help" });

        // Assert
        exitCode.Should().Be(0);
        var help = output.ToString();
        help.Should().Contain("<int>");
        help.Should().Contain("(default: 8080)");
    }

    [Fact]
    public async Task RunAsync_WithChildCommands_ShowsChildCommandsInHelp()
    {
        // Arrange
        var registry = new CommandRegistry();
        registry.Register(new CommandDefinition
        {
            Name = "git",
            Description = "Git operations",
            CommandType = typeof(TestCommand)
        });
        registry.Register(new CommandDefinition
        {
            Name = "commit",
            Parent = "git",
            Description = "Commit changes",
            CommandType = typeof(TestCommand)
        });
        registry.Register(new CommandDefinition
        {
            Name = "push",
            Parent = "git",
            Description = "Push commits",
            CommandType = typeof(TestCommand)
        });

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "git", "--help" });

        // Assert
        exitCode.Should().Be(0);
        var help = output.ToString();
        help.Should().Contain("Commands:");
        help.Should().Contain("commit");
        help.Should().Contain("push");
    }

    [Fact]
    public async Task RunAsync_WithoutHelpOption_ExecutesCommand()
    {
        // Arrange
        var registry = new CommandRegistry();
        registry.Register(new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        });

        var app = new CliApplication(registry);
        var output = new StringWriter();
        Console.SetOut(output);

        // Act
        var exitCode = await app.RunAsync(new[] { "test" });

        // Assert
        exitCode.Should().Be(0);
        var result = output.ToString();
        result.Should().Contain("Command executed");
    }

    private class TestCommand : ICommand
    {
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Command executed");
            return Task.FromResult(0);
        }
    }

    private class CustomHelpCommand : ICommand
    {
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            if (context.Arguments.HasOption("help"))
            {
                Console.WriteLine("CUSTOM HELP");
            }
            return Task.FromResult(0);
        }
    }
}
