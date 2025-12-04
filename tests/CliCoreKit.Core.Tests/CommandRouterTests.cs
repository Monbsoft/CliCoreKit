using AwesomeAssertions;
using Monbsoft.CliCoreKit.Core;

namespace CliCoreKit.Core.Tests;

public class CommandRouterTests
{
    [Fact]
    public void Route_SingleCommand_FindsCommand()
    {
        // Arrange
        var registry = new CommandRegistry();
        registry.Register(new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        });
        var router = new CommandRouter(registry);
        var args = new[] { "test", "--verbose" };

        // Act
        var route = router.Route(args);

        // Assert
        route.CommandDefinition.Should().NotBeNull();
        route.CommandDefinition!.Name.Should().Be("test");
        route.RemainingArgs.Should().Equal("--verbose");
        route.CommandPath.Should().Equal("test");
    }

    [Fact]
    public void Route_NestedSubcommands_FindsDeepestCommand()
    {
        // Arrange
        var registry = new CommandRegistry();
        registry.Register(new CommandDefinition
        {
            Name = "git",
            CommandType = typeof(TestCommand)
        });
        registry.Register(new CommandDefinition
        {
            Name = "remote",
            Parent = "git",
            CommandType = typeof(TestCommand)
        });
        registry.Register(new CommandDefinition
        {
            Name = "add",
            Parent = "git.remote",
            CommandType = typeof(TestCommand)
        });

        var router = new CommandRouter(registry);
        var args = new[] { "git", "remote", "add", "origin", "url" };

        // Act
        var route = router.Route(args);

        // Assert
        route.CommandDefinition.Should().NotBeNull();
        route.CommandDefinition!.Name.Should().Be("add");
        route.CommandPath.Should().Equal("git", "remote", "add");
        route.RemainingArgs.Should().Equal("origin", "url");
    }

    [Fact]
    public void Route_NoMatchingCommand_ReturnsNull()
    {
        // Arrange
        var registry = new CommandRegistry();
        var router = new CommandRouter(registry);
        var args = new[] { "unknown", "--flag" };

        // Act
        var route = router.Route(args);

        // Assert
        route.CommandDefinition.Should().BeNull();
        route.RemainingArgs.Should().Equal("unknown", "--flag");
    }

    [Fact]
    public void Route_EmptyArgs_ReturnsNull()
    {
        // Arrange
        var registry = new CommandRegistry();
        var router = new CommandRouter(registry);
        var args = Array.Empty<string>();

        // Act
        var route = router.Route(args);

        // Assert
        route.CommandDefinition.Should().BeNull();
        route.RemainingArgs.Should().BeEmpty();
    }

    [Fact]
    public void Route_OptionAsFirstArg_ReturnsNull()
    {
        // Arrange
        var registry = new CommandRegistry();
        registry.Register(new CommandDefinition
        {
            Name = "test",
            CommandType = typeof(TestCommand)
        });
        var router = new CommandRouter(registry);
        var args = new[] { "--help" };

        // Act
        var route = router.Route(args);

        // Assert
        route.CommandDefinition.Should().BeNull();
        route.RemainingArgs.Should().Equal("--help");
    }

    private class TestCommand : ICommand
    {
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
