using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Sanyappc.Extensions.AppOptions.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class JsonAttribute()
    : ValidationAttribute("The {0} field must be a valid JSON.")
{
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is string json)
        {
            try
            {
                using JsonDocument jsonDocument = JsonDocument.Parse(json);

                return true;
            }
            catch (JsonException) { }
        }

        return false;
    }
}
