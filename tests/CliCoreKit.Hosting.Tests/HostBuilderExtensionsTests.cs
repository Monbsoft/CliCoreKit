using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monbsoft.CliCoreKit.Core;
using Monbsoft.CliCoreKit.Hosting;

namespace CliCoreKit.Hosting.Tests;

public class HostBuilderExtensionsTests
{
    [Fact]
    public void ConfigureCli_RegistersCliServices()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();

        // Act
        builder.ConfigureCli(cli =>
        {
            cli.AddCommand<TestCommand>("test", "Test command");
        });

        var host = builder.Build();

        // Assert
        var app = host.Services.GetService<CliApplication>();
        app.Should().NotBeNull();

        var registry = host.Services.GetService<CommandRegistry>();
        registry.Should().NotBeNull();
        registry!.TryGetCommand("test", out var cmd).Should().BeTrue();
    }

    [Fact]
    public void AddCommand_RegistersCommandInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CliHostBuilder(services);

        // Act
        builder.AddCommand<TestCommand>("test");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var command = serviceProvider.GetService<TestCommand>();
        command.Should().NotBeNull();
    }

    [Fact]
    public void AddSubCommand_RegistersSubcommand()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new CliHostBuilder(services);

        // Act
        builder.AddCommand<TestCommand>("parent");
        builder.AddSubCommand<TestCommand>("child", "parent");
        builder.Build(services.BuildServiceProvider());

        // Assert
        var registry = services.BuildServiceProvider().GetRequiredService<CommandRegistry>();
        registry.TryGetCommand("child", out var cmd).Should().BeTrue();
        cmd!.Parent.Should().Be("parent");
    }

    private class TestCommand : ICommand
    {
        public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }
    }
}
