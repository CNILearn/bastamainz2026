# Stage 1: Basic Data Source Generator

## Overview

Stage 1 demonstrates the simplest implementation of a source generator that creates data factories from attribute-decorated classes. This stage serves as the baseline for comparing performance improvements in later stages.

## Key Features

- **Incremental Source Generation**: Uses `IIncrementalGenerator` for basic optimization
- **Attribute-Based Configuration**: Simple `[DataSource]` attribute for marking classes
- **No Caching**: All generation logic runs on every compilation
- **Single Data Source**: Only processes attribute configuration
- **Random Data Generation**: Creates realistic test data with random values

## Generated Code

For a class decorated with `[DataSource]`:

```csharp
[DataSource(EntityName = "User", Count = 5)]
public class User
{
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
}
```

The generator produces:

```csharp
public static class UserDataFactory
{
    public static User CreateSample() { /* implementation */ }
    public static List<User> CreateSamples(int count = 5) { /* implementation */ }
    public static string GetGeneratorInfo() { /* stage information */ }
}
```

## Performance Characteristics

- **No Caching**: All generation logic recalculated every build
- **Single Data Source**: Only attribute data processed
- **Baseline Performance**: Establishes comparison point for later stages

## Usage Example

```csharp
// Generate single instance
var user = UserDataFactory.CreateSample();

// Generate multiple instances
var users = UserDataFactory.CreateSamples(10);

// Get generator information
Console.WriteLine(UserDataFactory.GetGeneratorInfo());
```

## Building and Running

```bash
# Build the generator
dotnet build Stage1.Basic/Stage1.Basic.csproj

# Build and run the sample
dotnet run --project Stage1.Basic.Sample/Stage1.Basic.Sample.csproj

# Run tests
dotnet test Stage1.Basic.Tests/Stage1.Basic.Tests.csproj
```

## Next Stage

Stage 2 will add external file integration, demonstrating how to combine multiple data sources in the source generator pipeline.