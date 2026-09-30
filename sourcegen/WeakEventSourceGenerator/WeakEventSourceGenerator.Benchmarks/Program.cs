using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using WeakEventSourceGenerator.Attributes;

namespace WeakEventSourceGenerator.Benchmarks;

/// <summary>
/// Benchmarks comparing memory usage and performance between strong and weak events.
/// Demonstrates the memory benefits of weak events over traditional strong events.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class WeakVsStrongEventsBenchmark
{
    private StrongEventPublisher? _strongPublisher;
    private WeakEventPublisher? _weakPublisher;
    private readonly List<EventSubscriber> _subscribers = [];

    [Params(100, 500, 1000, 5000)]
    public int SubscriberCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _strongPublisher = new StrongEventPublisher();
        _weakPublisher = new WeakEventPublisher();
        
        // Create subscribers
        _subscribers.Clear();
        for (int i = 0; i < SubscriberCount; i++)
        {
            _subscribers.Add(new EventSubscriber($"Subscriber_{i}"));
        }
    }

    [Benchmark(Baseline = true)]
    public void StrongEvents_SubscribeAndPublish()
    {
        // Subscribe all handlers
        foreach (var subscriber in _subscribers)
        {
            _strongPublisher!.MessageReceived += subscriber.HandleMessage;
        }

        // Publish message
        _strongPublisher!.PublishMessage("Test message");

        // Unsubscribe all handlers (cleanup)
        foreach (var subscriber in _subscribers)
        {
            _strongPublisher!.MessageReceived -= subscriber.HandleMessage;
        }
    }

    [Benchmark]
    public void WeakEvents_SubscribeAndPublish()
    {
        // Subscribe all handlers
        foreach (var subscriber in _subscribers)
        {
            _weakPublisher!.MessageReceived += subscriber.HandleMessage;
        }

        // Publish message
        _weakPublisher!.PublishMessage("Test message");

        // Unsubscribe all handlers (cleanup)
        foreach (var subscriber in _subscribers)
        {
            _weakPublisher!.MessageReceived -= subscriber.HandleMessage;
        }
    }

    [Benchmark]
    public void StrongEvents_MemoryLeak_Simulation()
    {
        // Simulate memory leak by subscribing without unsubscribing
        var tempSubscribers = new List<EventSubscriber>();
        
        for (int i = 0; i < SubscriberCount / 10; i++) // Use fewer for memory leak test
        {
            var subscriber = new EventSubscriber($"TempSubscriber_{i}");
            tempSubscribers.Add(subscriber);
            _strongPublisher!.MessageReceived += subscriber.HandleMessage;
        }

        _strongPublisher!.PublishMessage("Memory leak test");

        // Simulate objects going out of scope (but events still hold references)
        tempSubscribers.Clear();
        
        // Force GC - strong events will prevent collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    [Benchmark]
    public void WeakEvents_NoMemoryLeak_Simulation()
    {
        // Simulate the same scenario with weak events
        var tempSubscribers = new List<EventSubscriber>();
        
        for (int i = 0; i < SubscriberCount / 10; i++)
        {
            var subscriber = new EventSubscriber($"TempSubscriber_{i}");
            tempSubscribers.Add(subscriber);
            _weakPublisher!.MessageReceived += subscriber.HandleMessage;
        }

        _weakPublisher!.PublishMessage("No memory leak test");

        // Simulate objects going out of scope
        tempSubscribers.Clear();
        
        // Force GC - weak events allow collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Publish again to see automatic cleanup
        _weakPublisher!.PublishMessage("After GC");
    }

    [Benchmark]
    public void WeakEvents_ManualCleanup()
    {
        // Subscribe some handlers
        var tempSubscribers = new List<EventSubscriber>();
        for (int i = 0; i < 50; i++)
        {
            var subscriber = new EventSubscriber($"CleanupTest_{i}");
            tempSubscribers.Add(subscriber);
            _weakPublisher!.MessageReceived += subscriber.HandleMessage;
        }

        // Clear references and force GC
        tempSubscribers.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Manual cleanup
        _weakPublisher!.CleanupMessageReceived();
        _weakPublisher!.PublishMessage("After manual cleanup");
    }
}

/// <summary>
/// Traditional event publisher using strong events.
/// </summary>
public class StrongEventPublisher
{
    public event Action<string>? MessageReceived;

    public void PublishMessage(string message)
    {
        MessageReceived?.Invoke(message);
    }

    public int SubscriberCount => MessageReceived?.GetInvocationList().Length ?? 0;
}

/// <summary>
/// Weak event publisher using C# 14 partial events with source generator implementation.
/// </summary>
[GenerateWeakEvents]
public partial class WeakEventPublisher
{
    [WeakEvent(AutoCleanup = true, CleanupThreshold = 5)]
    public partial event Action<string> MessageReceived;

    public void PublishMessage(string message)
    {
        OnMessageReceived(message);
    }
}

/// <summary>
/// Event subscriber that can be garbage collected.
/// </summary>
public class EventSubscriber
{
    private readonly string _name;
    private static int _handledCount;

    public EventSubscriber(string name)
    {
        _name = name;
    }

    public void HandleMessage(string message)
    {
        // Simple increment to prevent optimization
        Interlocked.Increment(ref _handledCount);
    }
}

/// <summary>
/// Memory usage comparison program.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Weak vs Strong Events Memory Benchmark ===");
        Console.WriteLine("This benchmark compares memory usage between strong and weak events.\n");

        if (args.Length > 0 && args[0] == "--benchmark")
        {
            // Run BenchmarkDotNet
            BenchmarkRunner.Run<WeakVsStrongEventsBenchmark>();
        }
        else
        {
            // Run simple memory demonstration
            RunMemoryDemo();
        }
    }

    static void RunMemoryDemo()
    {
        Console.WriteLine("Running memory usage demonstration...\n");

        var strongPublisher = new StrongEventPublisher();
        var weakPublisher = new WeakEventPublisher();

        Console.WriteLine("📊 Phase 1: Initial memory usage");
        PrintMemoryUsage();

        Console.WriteLine("\n📊 Phase 2: Creating 1000 subscribers for strong events");
        var strongSubscribers = CreateSubscribers(1000, "Strong");
        foreach (var subscriber in strongSubscribers)
        {
            strongPublisher.MessageReceived += subscriber.HandleMessage;
        }
        Console.WriteLine($"Strong event subscribers: {strongPublisher.SubscriberCount}");
        PrintMemoryUsage();

        Console.WriteLine("\n📊 Phase 3: Creating 1000 subscribers for weak events");
        var weakSubscribers = CreateSubscribers(1000, "Weak");
        foreach (var subscriber in weakSubscribers)
        {
            weakPublisher.MessageReceived += subscriber.HandleMessage;
        }
        Console.WriteLine($"Weak event subscribers: {weakPublisher.GetMessageReceivedSubscriberCount()}");
        PrintMemoryUsage();

        Console.WriteLine("\n📊 Phase 4: Clearing subscriber references (simulating scope exit)");
        strongSubscribers.Clear();
        weakSubscribers.Clear();
        PrintMemoryUsage();

        Console.WriteLine("\n📊 Phase 5: After garbage collection");
        ForceGarbageCollection();
        Console.WriteLine($"Strong event subscribers (should still be 1000): {strongPublisher.SubscriberCount}");
        Console.WriteLine($"Weak event subscribers (should decrease): {weakPublisher.GetMessageReceivedSubscriberCount()}");
        PrintMemoryUsage();

        Console.WriteLine("\n📊 Phase 6: After manual cleanup of weak events");
        weakPublisher.CleanupMessageReceived();
        Console.WriteLine($"Weak event subscribers after cleanup: {weakPublisher.GetMessageReceivedSubscriberCount()}");
        PrintMemoryUsage();

        Console.WriteLine("\n✅ Memory demonstration completed!");
        Console.WriteLine("\nKey observations:");
        Console.WriteLine("• Strong events keep objects alive, increasing memory usage");
        Console.WriteLine("• Weak events allow garbage collection, reducing memory usage");
        Console.WriteLine("• Weak events provide automatic and manual cleanup capabilities");
    }

    static List<EventSubscriber> CreateSubscribers(int count, string prefix)
    {
        var subscribers = new List<EventSubscriber>(count);
        for (int i = 0; i < count; i++)
        {
            subscribers.Add(new EventSubscriber($"{prefix}Subscriber_{i}"));
        }
        return subscribers;
    }

    static void PrintMemoryUsage()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memory = GC.GetTotalMemory(false);
        Console.WriteLine($"  💾 Memory usage: {memory:N0} bytes ({memory / 1024.0 / 1024.0:F2} MB)");
    }

    static void ForceGarbageCollection()
    {
        Console.WriteLine("  🧹 Forcing garbage collection...");
        for (int i = 0; i < 3; i++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
        Console.WriteLine("  ✅ Garbage collection completed");
    }
}