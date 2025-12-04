namespace Monbsoft.CliCoreKit.Core;

/// <summary>
/// Represents a command that can be executed.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="context">The command execution context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Exit code.</returns>
    Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
}
