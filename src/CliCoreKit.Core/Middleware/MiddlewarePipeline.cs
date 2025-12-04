namespace Monbsoft.CliCoreKit.Core.Middleware;

/// <summary>
/// Builds and executes a middleware pipeline.
/// </summary>
public sealed class MiddlewarePipeline
{
    private readonly List<ICommandMiddleware> _middlewares = new();

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    public MiddlewarePipeline Use(ICommandMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// Builds the pipeline execution function.
    /// </summary>
    public Func<CommandContext, CancellationToken, Task<int>> Build(
        Func<CommandContext, CancellationToken, Task<int>> finalHandler)
    {
        Func<CommandContext, CancellationToken, Task<int>> pipeline = finalHandler;

        // Build pipeline in reverse order
        foreach (var middleware in _middlewares.AsEnumerable().Reverse())
        {
            var currentMiddleware = middleware;
            var next = pipeline;
            pipeline = (context, ct) => currentMiddleware.InvokeAsync(context, next, ct);
        }

        return pipeline;
    }
}
