# Stage 2: File Integration Data Source Generator

## Overview

Stage 2 builds upon Stage 1 by adding external file integration capabilities. This demonstrates how to combine multiple data sources in a source generator pipeline, specifically combining attribute-based configuration with external JSON configuration files.

## Key Features

- **Multiple Data Sources**: Combines attribute configuration with external JSON files
- **AdditionalFiles Integration**: Uses the AdditionalFiles provider to read external configuration
- **Template-Based Generation**: Supports external templates for more realistic data generation
- **Graceful Fallback**: Falls back to basic generation when no external configuration is found
- **No Caching**: Still recalculates everything on every build (benchmark baseline for Stage 3)

## External Configuration

Stage 2 introduces JSON configuration files with the `.datasource.json` extension:

```json
{
  "entityType": "User",
  "defaultCount": 7,
  "templates": {
    "Name": ["Alice Johnson", "Bob Smith", "Carol Williams"],
    "Email": ["alice@example.com", "bob@company.org", "carol@business.net"]
  }
}
```

## Generated Code

For a class with external configuration:

```csharp
[DataSource(EntityName = "User", Count = 5)]
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
}
```

The generator produces enhanced data factories that use external templates when available:

```csharp
public static class UserDataFactory
{
    public static User CreateSample()
    {
        return new User
        {
            Name = new[] { "Alice Johnson", "Bob Smith", "Carol Williams" }[_random.Next(3)],
            Email = new[] { "alice@example.com", "bob@company.org", "carol@business.net" }[_random.Next(3)]
        };
    }
    
    public static string GetConfigurationInfo()
    {
        return "External configuration: User.datasource.json, Templates: 2";
    }
}
```

## Performance Characteristics

- **File I/O Operations**: Reads and parses JSON files every build
- **Multiple Data Sources**: Processes both attributes and external files
- **No Caching**: All operations recalculated (baseline for Stage 3 comparison)

## Building and Running

```bash
# Build the generator
dotnet build Stage2.FileIntegration/Stage2.FileIntegration.csproj

# Build and run the sample
dotnet run --project Stage2.FileIntegration.Sample/Stage2.FileIntegration.Sample.csproj

# Run tests
dotnet test Stage2.FileIntegration.Tests/Stage2.FileIntegration.Tests.csproj
```

## Next Stage

Stage 3 will introduce caching mechanisms to avoid redundant file operations and JSON parsing, demonstrating measurable performance improvements in the source generation pipeline.