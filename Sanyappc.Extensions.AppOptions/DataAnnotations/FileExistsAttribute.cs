using System.ComponentModel.DataAnnotations;

namespace Sanyappc.Extensions.AppOptions.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class FileExistsAttribute : DataTypeAttribute
{
    public FileExistsAttribute()
        : base(DataType.Upload)
    {
        ErrorMessage = "The {0} field must be a valid file path to an existing file.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        return value is string filePath
            && File.Exists(filePath);
    }
}
