using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monbsoft.CliCoreKit.Core;

namespace Monbsoft.CliCoreKit.Hosting;

/// <summary>
/// Extension methods for IHostBuilder to integrate CLI functionality.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures CLI application services.
    /// </summary>
    public static IHostBuilder ConfigureCli(this IHostBuilder hostBuilder, Action<CliHostBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);
        ArgumentNullException.ThrowIfNull(configure);

        return hostBuilder.ConfigureServices((context, services) =>
        {
            var builder = new CliHostBuilder(services);
            configure(builder);

            // Build and register services after configuration
            builder.Build(services.BuildServiceProvider());
        });
    }

    /// <summary>
    /// Runs the CLI application.
    /// </summary>
    public static async Task<int> RunCliAsync(this IHost host, string[] args, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        var app = host.Services.GetRequiredService<CliApplication>();
        return await app.RunAsync(args, cancellationToken);
    }
}
