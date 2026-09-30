using System.Diagnostics;
using System.Text;

using ActivitySourceGenerator.Attributes;

namespace ActivitySourceGenerator.Sample;

public class BusinessService
{
    [Activity(ActivitySourceName = "BusinessService", ActivityName = "ProcessData")]
    public static string ProcessData(string input)
    {
        // Simulate some work
        Thread.Sleep(100);
        
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }
        
        return $"Processed: {input}";
    }

    [Activity]
    public static async Task<int> ProcessAsync(string data)
    {
        // Simulate async work
        await Task.Delay(200);
        return data.Length;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Configure ActivityListener to see the activities
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => Console.WriteLine($"🔄 Started: {activity.DisplayName} | Source: {activity.Source.Name}"),
            ActivityStopped = activity => Console.WriteLine($"✅ Stopped: {activity.DisplayName} | Status: {activity.Status} | Duration: {activity.Duration.TotalMilliseconds}ms")
        };
        
        ActivitySource.AddActivityListener(listener);
        
        Console.WriteLine("=== ActivityAttribute Source Generator Demo ===");
        Console.WriteLine("This demo shows how the source generator creates wrapper methods with automatic activity tracing.");
        Console.WriteLine();
        
        try
        {
            // Test method with ActivityAttribute using generated wrapper
            Console.WriteLine("1. Testing ProcessData method using generated wrapper:");
            var result = BusinessService.ProcessData("Hello World");
            Console.WriteLine($"Result: {result}");
            Console.WriteLine();
            
            // Test async method with ActivityAttribute using generated wrapper
            Console.WriteLine("2. Testing ProcessAsync method using generated wrapper:");
            var length = await BusinessService.ProcessAsync("Testing async");
            Console.WriteLine($"Length: {length}");
            Console.WriteLine();
            
            // Compare with direct call (no activity tracing)
            Console.WriteLine("3. Direct method call (no activity tracing):");
            var directResult = BusinessService.ProcessData("Direct call");
            Console.WriteLine($"Result: {directResult}");
            Console.WriteLine();
            
            // Test exception handling
            Console.WriteLine("4. Testing exception handling with wrapper:");
            try
            {
                BusinessService.ProcessData("");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Caught expected exception: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Demo completed! The source generator created wrapper methods that automatically");
        Console.WriteLine("add activity tracing around your original methods.");
        Console.WriteLine();
        Console.WriteLine("Generated wrapper methods are available as:");
        Console.WriteLine("- BusinessServiceActivityWrapper.ProcessDataWithActivity()");
        Console.WriteLine("- BusinessServiceActivityWrapper.ProcessAsyncWithActivity()");
    }
}
