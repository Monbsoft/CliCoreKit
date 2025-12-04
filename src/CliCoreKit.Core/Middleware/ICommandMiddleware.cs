namespace Monbsoft.CliCoreKit.Core.Middleware;

/// <summary>
/// Middleware for command execution pipeline (Open/Closed Principle).
/// </summary>
public interface ICommandMiddleware
{
    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Exit code.</returns>
    Task<int> InvokeAsync(
        CommandContext context,
        Func<CommandContext, CancellationToken, Task<int>> next,
        CancellationToken cancellationToken = default);
}
