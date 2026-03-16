using Microsoft.Extensions.Options;

namespace Sanyappc.Extensions.AppOptions;

public abstract class AppOptionsValidator<TAppOptions>(string? name) : IValidateOptions<TAppOptions>
    where TAppOptions : AppOptions
{
    public string? Name { get; } = name;

    protected virtual void OnValidate(string? name, TAppOptions options, ValidateOptionsResultBuilder validateOptionsResultBuilder) { }

    public ValidateOptionsResult Validate(string? name, TAppOptions options)
    {
        if (Name is not null && Name != name)
            return ValidateOptionsResult.Skip;

        ValidateOptionsResultBuilder validateOptionsResultBuilder = new();

        ValidateOptionsResult validateOptionsResult = new DataAnnotationValidateOptions<TAppOptions>(name)
            .Validate(name, options);

        validateOptionsResultBuilder.AddResult(validateOptionsResult);

        OnValidate(name, options, validateOptionsResultBuilder);

        return validateOptionsResultBuilder.Build();
    }
}
