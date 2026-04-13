using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Sanyappc.Extensions.AppOptions.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
public sealed class IpAddressAttribute()
    : ValidationAttribute("The {0} field must be a valid IP address.")
{
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        return value is string ipString && IPAddress.TryParse(ipString, out _);
    }
}