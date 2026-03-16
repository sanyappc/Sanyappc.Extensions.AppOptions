# Sanyappc.Extensions.AppOptions

[![NuGet](https://img.shields.io/nuget/v/Sanyappc.Extensions.AppOptions)](https://www.nuget.org/packages/Sanyappc.Extensions.AppOptions)

A .NET library for typed, validated application configuration. Integrates with `IHostApplicationBuilder` to bind options from configuration, register them in DI, and validate them — both eagerly at startup and lazily on first use — using Data Annotations and optional custom validation logic.

## Installation

```
dotnet add package Sanyappc.Extensions.AppOptions
```

## Usage

### 1. Define your options class

Inherit from `AppOptions` and annotate properties with standard Data Annotation attributes:

```csharp
public class DatabaseOptions : AppOptions
{
    [Required]
    [MinLength(1)]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 100)]
    public int MaxPoolSize { get; set; } = 10;
}
```

### 2. Add to configuration

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=mydb",
    "MaxPoolSize": 20
  }
}
```

### 3. Register

Call `AddAppOptions` on your `IHostApplicationBuilder`. The options are bound from the specified configuration section, registered in DI, and validated on startup.

```csharp
builder.AddAppOptions<DatabaseOptions>(section: "Database");
```

To also receive the validated options instance at registration time (useful for consuming values before the host starts):

```csharp
builder.AddAppOptions<DatabaseOptions>(section: "Database", out DatabaseOptions dbOptions);
```

### 4. Inject

```csharp
public class MyService(IOptions<DatabaseOptions> options)
{
    public void DoWork()
    {
        string cs = options.Value.ConnectionString;
    }
}
```

## Named options

To register multiple instances of the same options type, use the `name` parameter:

```csharp
builder.AddAppOptions<DatabaseOptions>(name: "primary",   section: "Database:Primary");
builder.AddAppOptions<DatabaseOptions>(name: "secondary", section: "Database:Secondary");
```

Inject by name using `IOptionsSnapshot<T>` or `IOptionsMonitor<T>`:

```csharp
public class MyService(IOptionsMonitor<DatabaseOptions> monitor)
{
    public void DoWork()
    {
        DatabaseOptions primary = monitor.Get("primary");
    }
}
```

## Custom validation

Override `OnValidate` to add validation logic beyond Data Annotations:

```csharp
public class DatabaseOptionsValidator(string? name)
    : AppOptionsValidator<DatabaseOptions>(name)
{
    protected override void OnValidate(
        string? name,
        DatabaseOptions options,
        ValidateOptionsResultBuilder builder)
    {
        if (options.MaxPoolSize > 50 && options.ConnectionString.Contains("localhost"))
            builder.AddError("MaxPoolSize above 50 is not supported for local connections.");
    }
}
```

Register with the custom validator:

```csharp
builder.AddAppOptions<DatabaseOptions, DatabaseOptionsValidator>(section: "Database");
```

## DataAnnotations

The library ships extra validation attributes in the `Sanyappc.Extensions.AppOptions.DataAnnotations` namespace.

### `FileExistsAttribute`

Validates that the value is a path to an existing file.

```csharp
[FileExists]
public string CertificatePath { get; set; } = string.Empty;
```

### `JsonAttribute`

Validates that the value is well-formed JSON.

```csharp
[Json]
public string ExtraPayload { get; set; } = string.Empty;
```

### `RequiredItemsAttribute`

Validates that a collection contains no `null` items.

```csharp
[RequiredItems]
public List<string?> Endpoints { get; set; } = [];
```

### `ValidateItemsAttribute`

Validates each item in a collection with a specified `ValidationAttribute`. Arguments are forwarded to the attribute's constructor.

```csharp
[ValidateItems(typeof(MinLengthAttribute), 1)]
public List<string> Hostnames { get; set; } = [];

[ValidateItems(typeof(RangeAttribute), 1, 65535)]
public List<int> Ports { get; set; } = [];
```

All attributes treat `null` values as valid — combine with `[Required]` when the field itself must not be null.

## API reference

### `AddAppOptions` overloads

All overloads are extension methods on `IHostApplicationBuilder`. `name` defaults to `null` (unnamed options); `section` defaults to `null` (binds from the root).

| Overload | Description |
|---|---|
| `AddAppOptions<T>(name?, section?)` | Register with DataAnnotation validation only. |
| `AddAppOptions<T, TValidator>(name?, section?)` | Register with a custom validator. |
| `AddAppOptions<T>(name?, section?, out T options)` | Register and eagerly validate; returns the bound instance. |
| `AddAppOptions<T, TValidator>(name?, section?, out T options)` | Register with a custom validator and eagerly validate. |

### `AppOptionsValidator<T>`

| Member | Description |
|---|---|
| `Name` | The options name this validator is bound to. |
| `Validate(name, options)` | Runs DataAnnotation validation, then calls `OnValidate`. |
| `OnValidate(name, options, builder)` | Override to add custom validation failures. |
