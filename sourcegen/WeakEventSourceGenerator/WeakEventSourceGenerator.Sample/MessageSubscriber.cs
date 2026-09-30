namespace WeakEventSourceGenerator.Sample;

/// <summary>
/// A subscriber class that can be garbage collected.
/// </summary>
public class MessageSubscriber(string name)
{
    public void HandleMessage(string message)
    {
        Console.WriteLine($"  🔔 {name} received: '{message}'");
    }

    ~MessageSubscriber()
    {
        Console.WriteLine($"  💀 {name} has been garbage collected");
    }
}
