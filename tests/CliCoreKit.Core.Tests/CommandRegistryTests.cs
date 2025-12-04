using AwesomeAssertions;
using Monbsoft.CliCoreKit.Core;

namespace CliCoreKit.Core.Tests;

public class CommandRegistryTests
{
    private readonly CommandRegistry _registry = new();

    [Fact]
    public void Register_ValidCommand_Succeeds()
    {
        // Arrange
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };

        // Act
        _registry.Register(definition);

        // Assert
        _registry.TryGetCommand("test", out var result).Should().BeTrue();
        result.Should().NotBeNull();
        result!.Name.Should().Be("test");
    }

    [Fact]
    public void Register_DuplicateCommand_Throws()
    {
        // Arrange
        var definition = new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        };
        _registry.Register(definition);

        // Act
        var act = () => _registry.Register(definition);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public void Register_WithAliases_CanFindByAlias()
    {
        // Arrange
        var definition = new CommandDefinition
        {
            Name = "list",
            Aliases = new[] { "ls", "l" },
            CommandType = typeof(TestCommand)
        };

        // Act
        _registry.Register(definition);

        // Assert
        _registry.TryGetCommand("ls", out var result).Should().BeTrue();
        result!.Name.Should().Be("list");
    }

    [Fact]
    public void GetRootCommands_ReturnsOnlyRootCommands()
    {
        // Arrange
        _registry.Register(new CommandDefinition
        {
            Name = "root1",
            CommandType = typeof(TestCommand)
        });
        _registry.Register(new CommandDefinition
        {
            Name = "sub1",
            Parent = "root1",
            CommandType = typeof(TestCommand)
        });

        // Act
        var roots = _registry.GetRootCommands().ToList();

        // Assert
        roots.Should().ContainSingle().Which.Name.Should().Be("root1");
    }

    [Fact]
    public void GetSubcommands_ReturnsChildCommands()
    {
        // Arrange
        _registry.Register(new CommandDefinition
        {
            Name = "git",
            CommandType = typeof(TestCommand)
        });
        _registry.Register(new CommandDefinition
        {
            Name = "commit",
            Parent = "git",
            CommandType = typeof(TestCommand)
        });
        _registry.Register(new CommandDefinition
        {
            Name = "push",
            Parent = "git",
            CommandType = typeof(TestCommand)
        });

        // Act
        var subcommands = _registry.GetSubcommands("git").ToList();

        // Assert
        subcommands.Should().HaveCount(2);
        subcommands.Select(c => c.Name).Should().Contain(new[] { "commit", "push" });
    }

    private class TestCommand : ICommand
    {
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
