# ActivitySourceGenerator

An advanced C# source generator that uses C# 12+ interceptors to automatically replace method calls with activity-instrumented versions. This provides seamless tracing without requiring wrapper method calls.

## Features

- **Transparent Method Interception**: Uses C# 12+ interceptors for seamless integration
- **No Wrapper Methods**: Original calls are automatically intercepted at compile time
- **Advanced Code Replacement**: Compile-time method call replacement
- **Exception Handling**: Captures and records exceptions as activity errors
- **Async/Await Support**: Full support for asynchronous methods
- **Zero Runtime Dependencies**: Pure compile-time generation with no runtime overhead
- **OpenTelemetry Compatible**: Uses standard `ActivitySource` and `Activity` APIs

## How It Works

The interceptor-based generator automatically replaces your method calls with activity-instrumented versions at compile time. No wrapper methods needed - your original code remains unchanged!

## Usage

### 1. Mark Methods with [Activity] Attribute

```csharp
using ActivitySourceGenerator.Attributes;

public class BusinessService
{
    [Activity]
    public static string ProcessData(string input)
    {
        // Your business logic here
        return $"Processed: {input}";
    }

    [Activity(ActivityName = "CustomOperation")]
    public static async Task<int> ProcessAsync(string data)
    {
        await Task.Delay(100);
        return data.Length;
    }
}
```

### 2. Call Methods Normally - They Are Automatically Intercepted!

```csharp
// Your original code - no changes needed!
var result = BusinessService.ProcessData("Hello World");
var length = await BusinessService.ProcessAsync("Test Data");

// At compile time, these calls are automatically intercepted and become:
// var result = BusinessService_ProcessData_Interceptor("Hello World");
// var length = await BusinessService_ProcessAsync_Interceptor("Test Data");
```

## Generated Interceptor Code

The generator creates interceptor methods that replace your original calls:

```csharp
// Auto-generated interceptor methods
public static partial class BusinessService_Interceptors
{
    private static readonly ActivitySource _activitySource = new("BusinessService");

    [InterceptsLocation("BusinessService.cs", 45, 23)]
    public static string ProcessData_Interceptor(string input)
    {
        using var activity = _activitySource.StartActivity("ProcessData");
        try
        {
            var result = BusinessService.ProcessData(input);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
```

## Configuration

### ActivityAttribute Properties

- **ActivitySourceName**: Custom name for the ActivitySource (defaults to class name)
- **ActivityName**: Custom name for the activity (defaults to method name)
- **RecordExceptions**: Whether to record exceptions (default: true)
- **Tags**: Custom tags in "key1=value1;key2=value2" format

### Example with Custom Configuration

```csharp
[Activity(
    ActivitySourceName = "MyApplication.Orders", 
    ActivityName = "ProcessOrder",
    RecordExceptions = true,
    Tags = "component=order-service;version=1.0"
)]
public static void ProcessOrder(Order order)
{
    // Processing logic
}
```

## Installation

1. Add the interceptor source generator to your project:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/ActivitySourceGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

2. Use the `[Activity]` attribute on your methods
3. Call your original methods normally - they will be automatically intercepted!

## Integration with OpenTelemetry

This generator works seamlessly with OpenTelemetry:

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

// Configure OpenTelemetry
using var tracerProvider = TracerProvider.CreateBuilder()
    .AddSource("BusinessService")  // Add your ActivitySource
    .AddConsoleExporter()
    .Build();

// Use your original methods - they are automatically traced!
var result = BusinessService.ProcessData("Hello World");
```

## How Interceptors Work

1. **Compilation Time**: The source generator analyzes your code for `[Activity]` attributes and method calls
2. **Code Generation**: Creates interceptor methods with activity instrumentation
3. **Interception**: Original method calls are replaced with interceptor calls at compile time
4. **Zero Overhead**: No runtime dependencies, transparent interception

## Sample Output

```
🔄 Started: ProcessData | Source: BusinessService
✅ Stopped: ProcessData | Status: Ok | Duration: 101.28ms

🔄 Started: ProcessAsync | Source: BusinessService  
✅ Stopped: ProcessAsync | Status: Ok | Duration: 202.23ms

🔄 Started: ProcessData | Source: BusinessService
✅ Stopped: ProcessData | Status: Error | Duration: 102.73ms
```

## Requirements

- **.NET 9.0** or later (required for interceptor support)
- **C# 12.0** or later for interceptor features
- **MSBuild-based projects** (.NET SDK style)

## Benefits

- **Transparent Integration**: No need to change your method calls
- **Standardized Tracing**: Consistent activity creation across your application
- **Reduced Boilerplate**: No need to manually create activities
- **Exception Safety**: Automatic exception handling and recording
- **Performance**: Zero runtime overhead, compile-time code generation  
- **OpenTelemetry Ready**: Works with any OpenTelemetry-compatible tracing system
- **Advanced Technology**: Uses cutting-edge C# interceptor features

## Differences from SimpleActivitySourceGenerator

| Feature | SimpleActivitySourceGenerator | ActivitySourceGenerator |
|---------|------------------------------|-------------------------|
| Method Calls | Wrapper methods (`*WithActivity`) | Original methods (transparent) |
| .NET Version | .NET 8.0+ | .NET 9.0+ (interceptors) |
| Code Changes | Call wrapper methods | No changes needed |
| Integration | Manual wrapper calls | Automatic interception |
| Complexity | Simple wrapper approach | Advanced interceptor technology |

## Examples Repository

Check out the [ActivitySourceGenerator.Sample](ActivitySourceGenerator.Sample/) project for complete working examples demonstrating all features.

---

Part of the [sourcegenerators-samples](../../README.md) repository demonstrating advanced C# source generator techniques.