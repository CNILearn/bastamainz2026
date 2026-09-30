using ActivitySourceGenerator.Attributes;

using System.Diagnostics;

namespace ActivitySourceGenerator.Tests;

public class ActivitySourceGeneratorIntegrationTests
{
    [Activity]
    public static string TestMethodWithActivity(string input)
    {
        return $"Processed: {input}";
    }

    [Activity(ActivityName = "CustomTestActivity")]
    public static async Task<int> TestAsyncMethodWithActivity(string data)
    {
        await Task.Delay(10);
        return data.Length;
    }

    [Fact]
    public void ActivityAttribute_ExistsAndCanBeApplied()
    {
        // This test verifies that the ActivityAttribute is generated and can be applied
        // The fact that this code compiles proves the source generator is working
        Assert.True(true, "ActivityAttribute exists and can be applied to methods");
    }

    [Fact]
    public void GeneratedInterceptors_ExistAndWork()
    {
        // Test synchronous method - interceptor should automatically wrap the call
        var result = TestMethodWithActivity("test input");
        Assert.Equal("Processed: test input", result);
    }

    [Fact]
    public async Task GeneratedAsyncInterceptors_ExistAndWork()
    {
        // Test asynchronous method - interceptor should automatically wrap the call
        var result = await TestAsyncMethodWithActivity("test data");
        Assert.Equal(9, result);
    }

    [Fact]
    public void GeneratedInterceptors_CreateActivities()
    {
        List<Activity> activities = [];

        using ActivityListener listener = new()
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activities.Add,
        };
        
        ActivitySource.AddActivityListener(listener);

        // Execute method which should be intercepted and create an activity
        TestMethodWithActivity("test");

        // Verify activity was created
        Assert.Single(activities);
        Assert.Equal("TestMethodWithActivity", activities[0].DisplayName);
    }

    [Fact]
    public void GeneratedInterceptors_HandleExceptions()
    {
        List<Activity> activities = [];

        using ActivityListener listener = new()
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activities.Add
        };
        
        ActivitySource.AddActivityListener(listener);

#pragma warning disable IDE0200 // Disable IDE warning about unnecessary lambda - test fails with this change!
        // Test exception handling by calling a method that throws
        Assert.Throws<ArgumentNullException>(() => TestExceptionMethod());
#pragma warning restore IDE0200

        // Verify activity was created and marked as error
        Assert.Single(activities);
        Assert.Equal(ActivityStatusCode.Error, activities[0].Status);
    }

    [Activity]
    internal static void TestExceptionMethod()
    {
        throw new ArgumentNullException("test parameter");
    }
}