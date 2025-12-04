namespace Monbsoft.CliCoreKit.Core.Validation;

/// <summary>
/// Default implementation of argument validator.
/// </summary>
public sealed class DefaultArgumentValidator : IArgumentValidator
{
    public ValidationResult Validate(ParsedArguments arguments, CommandDefinition definition)
    {
        var result = new ValidationResult();

        // Validate required options
        foreach (var option in definition.Options.Where(o => o.IsRequired))
        {
            var hasValue = arguments.HasOption(option.Name) ||
                          (option.ShortName.HasValue && arguments.HasOption(option.ShortName.Value.ToString()));

            if (!hasValue)
            {
                var optionDisplay = option.ShortName.HasValue
                    ? $"--{option.Name}/-{option.ShortName}"
                    : $"--{option.Name}";
                result.AddError($"Required option '{optionDisplay}' is missing.", option.Name);
            }
        }

        return result;
    }
}
