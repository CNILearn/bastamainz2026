using Microsoft.AspNetCore.SignalR.Client;
using Shipping.Domain;

string url = args.Length > 0 ? args[0] : "https://localhost:5285/hubs/shipments";

await using HubConnection connection = new HubConnectionBuilder()
    .WithUrl(url)
    .WithAutomaticReconnect()
    .Build();

// The hub broadcasts a (shipmentId, ShipmentStatus) pair. Because ShipmentStatus is a union,
// SignalR's default JSON hub protocol serializes it the same way System.Text.Json does in
// 01-JsonSerialization: whichever case is active, with no wrapper type.
connection.On<string, ShipmentStatus>("ShipmentUpdated", (shipmentId, status) =>
{
    Console.WriteLine($"[{shipmentId}] {ShipmentStatusReporter.Describe(status)}");
});

await connection.StartAsync();
Console.WriteLine($"Connected to {url}");

const string shipmentId = "order-1";

// A hub method can also return a union directly.
ShipmentStatus initial = await connection.InvokeAsync<ShipmentStatus>("GetStatus", shipmentId);
Console.WriteLine($"[{shipmentId}] starting point: {ShipmentStatusReporter.Describe(initial)}");

// Advance the shipment until it reaches a final state (Delivered or Cancelled), letting the
// exhaustive ShipmentStatusReporter.IsFinal switch decide when to stop.
ShipmentStatus current = initial;
while (!ShipmentStatusReporter.IsFinal(current))
{
    await connection.InvokeAsync("Advance", shipmentId);
    await Task.Delay(TimeSpan.FromSeconds(1));
    current = await connection.InvokeAsync<ShipmentStatus>("GetStatus", shipmentId);
}

Console.WriteLine("Shipment reached a final state. Press Enter to exit.");
Console.ReadLine();
