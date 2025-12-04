using Microsoft.Extensions.Hosting;
using Monbsoft.CliCoreKit.Core;

namespace Monbsoft.CliCoreKit.Hosting;

/// <summary>
/// Hosted service that runs CLI commands and stops the host.
/// </summary>
public sealed class CliHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly CliApplication _cliApp;
    private readonly string[] _args;
    private int _exitCode;

    public CliHostedService(
        IHostApplicationLifetime lifetime,
        CliApplication cliApp,
        string[] args)
    {
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        _cliApp = cliApp ?? throw new ArgumentNullException(nameof(cliApp));
        _args = args ?? throw new ArgumentNullException(nameof(args));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    _exitCode = await _cliApp.RunAsync(_args, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
                    _exitCode = 1;
                }
                finally
                {
                    _lifetime.StopApplication();
                }
            }, cancellationToken);
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Environment.ExitCode = _exitCode;
        return Task.CompletedTask;
    }
}
