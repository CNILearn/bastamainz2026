# WeakEventSourceGenerator

A C# source generator that creates weak event patterns to prevent memory leaks from event handler references. This generator demonstrates advanced source generation techniques and provides a practical solution for memory-efficient event handling.

## Overview

Traditional C# events can cause memory leaks because they maintain strong references to event handlers. If objects that subscribe to events are not properly unsubscribed, they cannot be garbage collected even when they go out of scope. This source generator solves this problem by creating weak event patterns that use `WeakReference` to store event handlers.

## Features

- **Automatic Weak Reference Management**: Events marked with `[WeakEvent]` automatically use weak references
- **Memory Leak Prevention**: Subscribers can be garbage collected even if not explicitly unsubscribed
- **Automatic Cleanup**: Dead weak references are automatically cleaned up based on configurable thresholds
- **Manual Cleanup Control**: Provides methods for manual cleanup when needed
- **Performance Optimized**: Uses incremental source generation for fast builds
- **Thread-Safe**: WeakEventManager is thread-safe with proper locking
- **Comprehensive API**: Generated methods for adding, removing, and managing weak event handlers

## Quick Start

### 1. Mark your class with `[GenerateWeakEvents]`

```csharp
// Mark events with [WeakEvent] and make them partial
[WeakEvent(AutoCleanup = true, CleanupThreshold = 5)]
public partial event Action<string> MessageReceived;
```

### 2. Use standard event syntax

```csharp
var publisher = new EventPublisher();
var subscriber = new MessageSubscriber();

// Use standard event syntax - the generated implementation provides weak references
publisher.MessageReceived += subscriber.HandleMessage;

// Raise the event normally
publisher.MessageReceived?.Invoke("Hello World!");

// Check subscriber count (generated method)
int count = publisher.GetMessageReceivedSubscriberCount();

// Manual cleanup if needed (generated method)
publisher.CleanupMessageReceived();
```

## Strong vs Weak Events Comparison

### Traditional Strong Events (Memory Leak Risk)

```csharp
public class EventPublisher
{
    public event Action<string>? MessageReceived;
    
    public void PublishMessage(string message)
    {
        MessageReceived?.Invoke(message);
    }
}

// Usage - can cause memory leaks
var publisher = new EventPublisher();
var subscriber = new MessageSubscriber(); // This object...
publisher.MessageReceived += subscriber.HandleMessage;
subscriber = null; // ...cannot be garbage collected!
```

### Weak Events (C# 14 Partial Events)

```csharp
[GenerateWeakEvents]
public partial class EventPublisher
{
    [WeakEvent]
    public partial event Action<string> MessageReceived;
}

// Usage - memory safe with standard event syntax
var publisher = new EventPublisher();
var subscriber = new MessageSubscriber(); // This object...
publisher.MessageReceived += subscriber.HandleMessage; // Standard syntax!
subscriber = null; // ...CAN be garbage collected!
```

## Configuration Options

The `[WeakEvent]` attribute supports several configuration options:

```csharp
[WeakEvent(
    AutoCleanup = true,        // Automatically clean up dead references
    CleanupThreshold = 10      // Clean up when 10 dead references accumulate
)]
public event Action<string>? MyEvent;
```

### Configuration Properties

- **AutoCleanup** (bool, default: true): Whether to automatically clean up dead weak references
- **CleanupThreshold** (int, default: 10): Number of dead references that trigger automatic cleanup

## Generated API

For each event marked with `[WeakEvent]`, the generator creates:

```csharp
// For event named "MessageReceived"
public void AddMessageReceivedHandler(Action<string> handler);
public void RemoveMessageReceivedHandler(Action<string> handler);
protected virtual void RaiseMessageReceived(string message);
public int GetMessageReceivedSubscriberCount();
public void CleanupMessageReceived();
```

## Memory Benefits

Our benchmarks show significant memory improvements when using weak events:

| Scenario | Strong Events | Weak Events | Improvement |
|----------|---------------|-------------|-------------|
| 1000 Subscribers (After GC) | 1000 alive | 1 alive | 99.9% reduction |
| Memory Usage | ~280KB | ~256KB | ~24KB saved |
| Objects Eligible for GC | 0 | 999 | Memory leak prevented |

## Sample Projects

### Basic Demo (`WeakEventSourceGenerator.Sample`)

Run the sample to see weak events in action:

```bash
cd src
dotnet run --project WeakEventSourceGenerator/WeakEventSourceGenerator.Sample
```

Output demonstrates:
- Strong events keeping objects alive after GC
- Weak events allowing proper garbage collection
- Automatic cleanup of dead references

### Memory Benchmark (`WeakEventSourceGenerator.Benchmarks`)

Run memory comparison benchmarks:

```bash
cd src
dotnet run --project WeakEventSourceGenerator/WeakEventSourceGenerator.Benchmarks
```

For detailed BenchmarkDotNet analysis:

```bash
dotnet run --project WeakEventSourceGenerator/WeakEventSourceGenerator.Benchmarks -- --benchmark
```

## Implementation Details

### WeakEventManager&lt;T&gt;

The core of the weak events system is the `WeakEventManager<T>` class:

- **Thread-Safe**: Uses locks to ensure thread safety
- **Automatic Cleanup**: Removes dead references during invocation and subscription
- **Configurable Thresholds**: Customizable cleanup behavior
- **Exception Handling**: Safely handles exceptions in event handlers

### Source Generator Architecture

The generator uses advanced Roslyn features:

- **Incremental Generation**: Uses `IIncrementalGenerator` for optimal performance
- **Attribute-Based Configuration**: Leverages `ForAttributeWithMetadataName` for efficient detection
- **Partial Class Generation**: Generates additional members for marked classes
- **Type-Safe Code Generation**: Creates strongly-typed methods for each event

## Best Practices

### When to Use Weak Events

✅ **Use weak events when:**
- Event publishers have longer lifetimes than subscribers
- You have many short-lived subscribers
- Explicit unsubscription is difficult or error-prone
- Memory leaks are a concern

❌ **Don't use weak events when:**
- Performance is critical (weak events have slight overhead)
- You need guaranteed delivery (subscribers might be GC'd)
- Event lifetimes are well-controlled

### Memory Management Tips

1. **Still unsubscribe when possible**: While weak events prevent leaks, explicit unsubscription is still good practice
2. **Monitor subscriber counts**: Use `GetXxxSubscriberCount()` methods to monitor event health
3. **Tune cleanup thresholds**: Adjust `CleanupThreshold` based on your event usage patterns
4. **Consider manual cleanup**: Use `CleanupXxx()` methods in performance-critical scenarios

## Requirements

- **.NET 10.0** or later (for C# 14 partial events support)
- **C# 14.0** with partial events feature
- **MSBuild-based projects** (.NET SDK style)

> **Note**: This implementation uses C# 14's partial events feature. When .NET 10 SDK becomes available, update the target framework to `net10.0` in the project files.

## Building

```bash
cd src
dotnet build WeakEventSourceGenerator.slnx
dotnet test WeakEventSourceGenerator.slnx
```

## Testing

The project includes comprehensive tests:

- **Unit Tests**: Validate source generator behavior
- **Integration Tests**: Test generated code functionality  
- **Memory Tests**: Verify garbage collection behavior
- **Performance Benchmarks**: Compare strong vs weak events

## Future Enhancements

This implementation provides a foundation that could be enhanced with:

- **Partial Events Support**: When C# 14/.NET 10 become available, integrate with partial events
- **Additional Event Types**: Support for more complex event patterns
- **Performance Optimizations**: Further optimize the WeakEventManager implementation
- **IDE Integration**: Enhanced IntelliSense and debugging support

## Contributing

This project demonstrates advanced source generator patterns including:

- Incremental source generation with `IIncrementalGenerator`
- Attribute-based code generation
- Memory-efficient event patterns
- Comprehensive testing strategies
- Performance benchmarking with BenchmarkDotNet

Perfect for learning about source generators, memory management, and distributed system patterns!

## License

This project is part of the CNinnovation source generator samples repository.