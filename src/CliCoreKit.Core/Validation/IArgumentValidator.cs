namespace Monbsoft.CliCoreKit.Core.Validation;

/// <summary>
/// Validates parsed arguments.
/// </summary>
public interface IArgumentValidator
{
    /// <summary>
    /// Validates the arguments.
    /// </summary>
    /// <param name="arguments">The parsed arguments.</param>
    /// <param name="definition">The command definition.</param>
    /// <returns>Validation result.</returns>
    ValidationResult Validate(ParsedArguments arguments, CommandDefinition definition);
}

/// <summary>
/// Represents the result of validation.
/// </summary>
public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    /// <summary>
    /// Gets whether the validation succeeded.
    /// </summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>
    /// Gets validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    public void AddError(string message, string? parameterName = null)
    {
        _errors.Add(new ValidationError(message, parameterName));
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new();
}

/// <summary>
/// Represents a validation error.
/// </summary>
public sealed record ValidationError(string Message, string? ParameterName = null);
