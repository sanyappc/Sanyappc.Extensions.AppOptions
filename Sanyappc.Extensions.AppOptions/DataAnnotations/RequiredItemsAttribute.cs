using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Sanyappc.Extensions.AppOptions.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class RequiredItemsAttribute()
    : ValidationAttribute("The {0} field must not contain null items.")
{
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is IEnumerable enumerable)
        {
            foreach (object? item in enumerable)
            {
                if (item is null)
                    return false;
            }
        }

        return true;
    }
}
