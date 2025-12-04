using Monbsoft.CliCoreKit.Core.Validation;

namespace Monbsoft.CliCoreKit.Core.Middleware;

/// <summary>
/// Middleware that validates command arguments.
/// </summary>
public sealed class ValidationMiddleware : ICommandMiddleware
{
    private readonly IArgumentValidator _validator;

    public ValidationMiddleware(IArgumentValidator? validator = null)
    {
        _validator = validator ?? new DefaultArgumentValidator();
    }

    public async Task<int> InvokeAsync(
        CommandContext context,
        Func<CommandContext, CancellationToken, Task<int>> next,
        CancellationToken cancellationToken = default)
    {
        // Get command definition from context
        if (context.Data.TryGetValue("CommandDefinition", out var defObj) &&
            defObj is CommandDefinition definition)
        {
            var result = _validator.Validate(context.Arguments, definition);

            if (!result.IsValid)
            {
                Console.Error.WriteLine("Validation errors:");
                foreach (var error in result.Errors)
                {
                    Console.Error.WriteLine($"  - {error.Message}");
                }
                return 1;
            }
        }

        return await next(context, cancellationToken);
    }
}
