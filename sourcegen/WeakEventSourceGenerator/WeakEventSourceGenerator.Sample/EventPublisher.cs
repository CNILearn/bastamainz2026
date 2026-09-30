using WeakEventSourceGenerator.Attributes;

namespace WeakEventSourceGenerator.Sample;

/// <summary>
/// Demonstrates the difference between strong events (traditional) and weak events (generated using partial events).
/// Weak events prevent memory leaks by using weak references to event handlers.
/// The source generator provides the add/remove accessors for partial events.
/// </summary>
[GenerateWeakEvents]
public partial class EventPublisher
{
    // Traditional strong event - can cause memory leaks
    public event Action<string>? StrongMessageReceived;

    // Weak event using C# 14 partial events - the source generator provides the implementation
    [WeakEvent(AutoCleanup = true, CleanupThreshold = 5)]
    public partial event Action<string> WeakMessageReceived;

    /// <summary>
    /// Publishes a message to both strong and weak event subscribers.
    /// </summary>
    public void PublishMessage(string message)
    {
        Console.WriteLine($"📢 Publishing: '{message}'");
        
        // Invoke strong event (traditional way)
        StrongMessageReceived?.Invoke(message);
        
        // Invoke weak event - generated partial event implementation
        OnWeakMessageReceived(message);

        Console.WriteLine($"   Strong subscribers: {GetStrongSubscriberCount()}");
        Console.WriteLine($"   Weak subscribers: {GetWeakMessageReceivedSubscriberCount()}");
        Console.WriteLine();
    }

    /// <summary>
    /// Gets the count of strong event subscribers (manual implementation).
    /// </summary>
    private int GetStrongSubscriberCount()
    {
        return StrongMessageReceived?.GetInvocationList().Length ?? 0;
    }
}
