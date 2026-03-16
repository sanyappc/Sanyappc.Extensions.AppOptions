using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Sanyappc.Extensions.AppOptions.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class ValidateItemsAttribute : ValidationAttribute
{
    private readonly ValidationAttribute itemValidationAttribute;

    public ValidateItemsAttribute(Type validationAttributeType, params object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(validationAttributeType);

        itemValidationAttribute = (ValidationAttribute)Activator.CreateInstance(validationAttributeType, args)!;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is IEnumerable enumerable)
        {
            int index = 0;
            foreach (object? item in enumerable)
            {
                if (!itemValidationAttribute.IsValid(item))
                {
                    string errorMessage = itemValidationAttribute.FormatErrorMessage(validationContext.DisplayName);
                    return new ValidationResult(
                        $"{errorMessage} (item at index {index})",
                        validationContext.MemberName is not null ? [validationContext.MemberName] : null);
                }
                index++;
            }
        }

        return ValidationResult.Success;
    }
}
