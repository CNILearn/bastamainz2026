# 04 · SignalR with Union Types

Two projects show a C# 15 **union type** (`Shipping.Domain.ShipmentStatus`) travelling over a
SignalR hub:

- **`04-SignalR`** hosts `ShipmentHub` at `/hubs/shipments`. Its `GetStatus` method *returns* a
  `ShipmentStatus`, and its `Advance` method updates a shipment and *broadcasts* the new
  `ShipmentStatus` to every connected client through the strongly typed `IShipmentClient.ShipmentUpdated`
  callback.
- **`04-SignalR.Client`** is a console client that connects, subscribes to `ShipmentUpdated`, and
  repeatedly calls `Advance` until `ShipmentStatusReporter.IsFinal` says the shipment is done —
  printing each union case as it arrives.

## What it demonstrates

- **Unions as hub method parameters and return types.** `ShipmentUpdated(string, ShipmentStatus)`
  and `Task<ShipmentStatus> GetStatus(...)` pass the union across the wire like any other type.
- **The default JSON hub protocol serializes unions too**, using the same `[JsonUnion]` /
  `ShipmentStatusTypeClassifier` support from `Shipping.Domain` shown in `01-JsonSerialization`.
  SignalR's JSON protocol uses camelCase property names, so the classifier compares names
  case-insensitively to work with both.
- **Shared, exhaustive workflow logic.** `Shipping.Domain.ShipmentWorkflow.Advance` — the same
  switch expression used by `02-MinimalApiOpenApi` — computes each shipment's next state.

## Run it

```bash
# Terminal 1
dotnet run --project 04-SignalR

# Terminal 2 (pass the hub URL printed by the server, if different)
dotnet run --project 04-SignalR.Client -- http://localhost:<port>/hubs/shipments
```

> Requires the **.NET 11 preview SDK**. See the [top-level README](../README.md) for setup.
