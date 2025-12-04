using AwesomeAssertions;
using Monbsoft.CliCoreKit.Core;

namespace CliCoreKit.Core.Tests;

public class ShortNameMappingTests
{
    [Fact]
    public async Task GetOption_WithShortName_ReturnsValueAccessibleByLongName()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "greeting",
            ShortName = 'g',
            Description = "Greeting message",
            ValueType = typeof(string)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Use short name -g
        var exitCode = await app.RunAsync(new[] { "test", "-g", "Bonjour" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        // Should be accessible by long name "greeting"
        var greeting = TestCommand.LastContext!.GetOption<string>("greeting");
        greeting.Should().Be("Bonjour");
    }

    [Fact]
    public async Task GetOption_WithLongName_ReturnsValue()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "greeting",
            ShortName = 'g',
            Description = "Greeting message",
            ValueType = typeof(string)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Use long name --greeting
        var exitCode = await app.RunAsync(new[] { "test", "--greeting", "Hello" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        var greeting = TestCommand.LastContext!.GetOption<string>("greeting");
        greeting.Should().Be("Hello");
    }

    [Fact]
    public async Task GetOption_WithShortNameFlag_AccessibleByLongName()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "verbose",
            ShortName = 'v',
            Description = "Verbose output",
            ValueType = typeof(bool)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Use short flag -v
        var exitCode = await app.RunAsync(new[] { "test", "-v" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        // Should be accessible by long name "verbose"
        var verbose = TestCommand.LastContext!.GetOption<bool>("verbose");
        verbose.Should().BeTrue();
    }

    [Fact]
    public async Task GetOption_WithMultipleShortNames_AllAccessibleByLongNames()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "greeting",
            ShortName = 'g',
            ValueType = typeof(string)
        });
        definition.Options.Add(new OptionDefinition
        {
            Name = "formal",
            ShortName = 'f',
            ValueType = typeof(bool)
        });
        definition.Options.Add(new OptionDefinition
        {
            Name = "repeat",
            ShortName = 'r',
            ValueType = typeof(int)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Use short names
        var exitCode = await app.RunAsync(new[] { "test", "-g", "Hi", "-f", "-r", "3" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        // All should be accessible by long names
        var greeting = TestCommand.LastContext!.GetOption<string>("greeting");
        var formal = TestCommand.LastContext!.GetOption<bool>("formal");
        var repeat = TestCommand.LastContext!.GetOption<int?>("repeat");

        greeting.Should().Be("Hi");
        formal.Should().BeTrue();
        repeat.Should().Be(3);
    }

    [Fact]
    public async Task GetOption_MixedShortAndLongNames_BothAccessible()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "input",
            ShortName = 'i',
            ValueType = typeof(string)
        });
        definition.Options.Add(new OptionDefinition
        {
            Name = "output",
            ShortName = 'o',
            ValueType = typeof(string)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Mix short and long names
        var exitCode = await app.RunAsync(new[] { "test", "-i", "input.txt", "--output", "output.txt" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        var input = TestCommand.LastContext!.GetOption<string>("input");
        var output = TestCommand.LastContext!.GetOption<string>("output");

        input.Should().Be("input.txt");
        output.Should().Be("output.txt");
    }

    [Fact]
    public async Task GetOption_WithShortNameAndDefaultValue_ReturnsCorrectValue()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "port",
            ShortName = 'p',
            ValueType = typeof(int),
            DefaultValue = 8080
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Override default with short name
        var exitCode = await app.RunAsync(new[] { "test", "-p", "3000" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        var port = TestCommand.LastContext!.GetOption<int?>("port");
        port.Should().Be(3000);
    }

    [Fact]
    public async Task GetOption_WithoutProvidingShortName_ReturnsDefaultValue()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "port",
            ShortName = 'p',
            ValueType = typeof(int),
            DefaultValue = 8080
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Don't provide the option
        var exitCode = await app.RunAsync(new[] { "test" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        var port = TestCommand.LastContext!.GetOption<int?>("port");
        port.Should().Be(8080);
    }

    [Fact]
    public async Task GetOption_CombinedShortOptions_AllAccessibleByLongNames()
    {
        // Arrange
        var registry = new CommandRegistry();
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        definition.Options.Add(new OptionDefinition
        {
            Name = "verbose",
            ShortName = 'v',
            ValueType = typeof(bool)
        });
        definition.Options.Add(new OptionDefinition
        {
            Name = "force",
            ShortName = 'f',
            ValueType = typeof(bool)
        });
        definition.Options.Add(new OptionDefinition
        {
            Name = "recursive",
            ShortName = 'r',
            ValueType = typeof(bool)
        });
        registry.Register(definition);

        var app = new CliApplication(registry);
        TestCommand.LastContext = null;

        // Act - Combined short options -vfr
        var exitCode = await app.RunAsync(new[] { "test", "-vfr" });

        // Assert
        exitCode.Should().Be(0);
        TestCommand.LastContext.Should().NotBeNull();
        
        // All should be accessible by long names
        var verbose = TestCommand.LastContext!.GetOption<bool>("verbose");
        var force = TestCommand.LastContext!.GetOption<bool>("force");
        var recursive = TestCommand.LastContext!.GetOption<bool>("recursive");

        verbose.Should().BeTrue();
        force.Should().BeTrue();
        recursive.Should().BeTrue();
    }

    private class TestCommand : ICommand
    {
        public static CommandContext? LastContext { get; set; }

        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            LastContext = context;
            return Task.FromResult(0);
        }
    }
}
