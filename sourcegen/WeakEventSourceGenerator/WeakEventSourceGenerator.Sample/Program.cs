using WeakEventSourceGenerator.Sample;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Weak Events Source Generator Demo ===");
Console.WriteLine("This demo shows the difference between strong and weak events.");
Console.WriteLine("Weak events prevent memory leaks by using weak references.\n");

var publisher = new EventPublisher();

Console.WriteLine("📌 Phase 1: Subscribing to events");
DemonstrateEventSubscription(publisher);

Console.WriteLine("\n📌 Phase 2: Publishing messages");
publisher.PublishMessage("Hello World!");

Console.WriteLine("📌 Phase 3: Forcing garbage collection");
ForceGarbageCollection();

Console.WriteLine("\n📌 Phase 4: Publishing after GC (notice weak event cleanup)");
publisher.PublishMessage("After GC");

Console.WriteLine("📌 Phase 5: Manual cleanup of weak events");
publisher.CleanupWeakMessageReceived();
publisher.PublishMessage("After manual cleanup");

Console.WriteLine("\n📌 Demonstration completed!");
Console.WriteLine("Key observations:");
Console.WriteLine("• Strong events keep objects alive even after they go out of scope");
Console.WriteLine("• Weak events allow objects to be garbage collected");
Console.WriteLine("• Weak events automatically clean up dead references");
Console.WriteLine("• Weak events provide manual cleanup control");

static void DemonstrateEventSubscription(EventPublisher publisher)
{
    // Create subscribers in a separate scope so they can be garbage collected
    CreateAndSubscribeHandlers(publisher);
}

static void CreateAndSubscribeHandlers(EventPublisher publisher)
{
    var subscriber1 = new MessageSubscriber("Alice");
    var subscriber2 = new MessageSubscriber("Bob");
    var subscriber3 = new MessageSubscriber("Carol");

    // Subscribe to strong event
    publisher.StrongMessageReceived += subscriber1.HandleMessage;
    publisher.StrongMessageReceived += subscriber2.HandleMessage;
    publisher.StrongMessageReceived += subscriber3.HandleMessage;

    // Subscribe to weak event using standard event syntax
    publisher.WeakMessageReceived += subscriber1.HandleMessage;
    publisher.WeakMessageReceived += subscriber2.HandleMessage;
    publisher.WeakMessageReceived += subscriber3.HandleMessage;

    Console.WriteLine("  ✅ Subscribed 3 handlers to both strong and weak events");

    // Objects go out of scope here and become eligible for GC
}

static void ForceGarbageCollection()
{
    Console.WriteLine("  🧹 Forcing garbage collection...");
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    Console.WriteLine("  ✅ Garbage collection completed");
}
