using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Sanyappc.Extensions.AppOptions;

public static class AppOptionsHostApplicationBuilderExtensions
{
    private static TAppOptions? AddAppOptionsPrivate<TAppOptions, TAppOptionsValidator>(
        IHostApplicationBuilder builder,
        string? name,
        string? section)
        where TAppOptions : AppOptions
        where TAppOptionsValidator : AppOptionsValidator<TAppOptions>
    {
        IConfiguration configuration = section is not null
            ? builder.Configuration.GetSection(section)
            : builder.Configuration;

        builder.Services
            .AddOptions<TAppOptions>(name)
            .Bind(configuration)
            .ValidateOnStart();

        ObjectFactory<TAppOptionsValidator> appOptionsValidatorFactory = ActivatorUtilities.CreateFactory<TAppOptionsValidator>([typeof(string)]);

        builder.Services.AddSingleton<IValidateOptions<TAppOptions>>(
            serviceProvider => appOptionsValidatorFactory.Invoke(serviceProvider, [name]));

        return configuration.Get<TAppOptions>();
    }

    public static IHostApplicationBuilder AddAppOptions<TAppOptions, TAppOptionsValidator>(
        this IHostApplicationBuilder builder,
        string? name,
        string? section,
        out TAppOptions options)
        where TAppOptions : AppOptions
        where TAppOptionsValidator : AppOptionsValidator<TAppOptions>
    {
        ArgumentNullException.ThrowIfNull(builder);

        options = AddAppOptionsPrivate<TAppOptions, TAppOptionsValidator>(builder, name, section)
            ?? throw new InvalidOperationException($"Configuration for {typeof(TAppOptions).Name} is missing.");

        TAppOptionsValidator optionsValidator = (TAppOptionsValidator)(Activator.CreateInstance(typeof(TAppOptionsValidator), name)
            ?? throw new InvalidOperationException($"Could not create an instance of {typeof(TAppOptionsValidator).Name}. Ensure it has a constructor with a single string? parameter for the options name."));

        ValidateOptionsResult validateOptionsResult = optionsValidator.Validate(name, options);

        if (validateOptionsResult.Failed)
        {
            throw new OptionsValidationException(
                name ?? string.Empty,
                typeof(TAppOptions),
                validateOptionsResult.Failures);
        }

        return builder;
    }

    public static IHostApplicationBuilder AddAppOptions<TAppOptions>(
        this IHostApplicationBuilder builder,
        string? name,
        string? section,
        out TAppOptions options)
        where TAppOptions : AppOptions
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddAppOptions<TAppOptions, DefaultAppOptionsValidator<TAppOptions>>(name, section, out options);
    }

    public static IHostApplicationBuilder AddAppOptions<TAppOptions, TAppOptionsValidator>(
        this IHostApplicationBuilder builder,
        out TAppOptions options)
        where TAppOptions : AppOptions
        where TAppOptionsValidator : AppOptionsValidator<TAppOptions>
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddAppOptions<TAppOptions, TAppOptionsValidator>(null, null, out options);
    }

    public static IHostApplicationBuilder AddAppOptions<TAppOptions>(
        this IHostApplicationBuilder builder,
        out TAppOptions options)
        where TAppOptions : AppOptions
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddAppOptions<TAppOptions, DefaultAppOptionsValidator<TAppOptions>>(null, null, out options);
    }

    public static IHostApplicationBuilder AddAppOptions<TAppOptions, TAppOptionsValidator>(
        this IHostApplicationBuilder builder,
        string? name = null,
        string? section = null)
        where TAppOptions : AppOptions
        where TAppOptionsValidator : AppOptionsValidator<TAppOptions>
    {
        ArgumentNullException.ThrowIfNull(builder);

        AddAppOptionsPrivate<TAppOptions, TAppOptionsValidator>(builder, name, section);

        return builder;
    }

    public static IHostApplicationBuilder AddAppOptions<TAppOptions>(
        this IHostApplicationBuilder builder,
        string? name = null,
        string? section = null)
        where TAppOptions : AppOptions
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddAppOptions<TAppOptions, DefaultAppOptionsValidator<TAppOptions>>(name, section);
    }

    private sealed class DefaultAppOptionsValidator<TAppOptions>(string? name) : AppOptionsValidator<TAppOptions>(name)
        where TAppOptions : AppOptions
    { }
}
