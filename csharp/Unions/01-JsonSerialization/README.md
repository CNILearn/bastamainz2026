# 01 · JSON Serialization with Union Types

This sample shows how `System.Text.Json` serializes and deserializes a C# 15 **union type**,
`Shipping.Domain.ShipmentStatus`. A shipment is in exactly one of five states — `Placed`,
`Processing`, `Shipped`, `Delivered`, or `Cancelled` — modeled as a union instead of a shared
base class, a `Kind` enum, or a bag of nullable properties.

## What it demonstrates

- **Serialization** just works: `JsonSerializer.Serialize(status)` writes whichever case is
  currently active, with no wrapper object.
- **Deserialization** needs to know which case type a JSON object maps to. Because the five
  cases have distinct property sets, this sample plugs in a custom
  [`JsonTypeClassifierFactory`](../Shipping.Domain/ShipmentStatus.cs) that looks at the
  property names in the payload — no `"$type"` discriminator property required.
- **Exhaustive pattern matching** in `ShipmentStatusReporter.Describe` (in `Shipping.Domain`):
  the `switch` expression must cover every case; the compiler rejects it if a case is missing.

## Run it

```bash
dotnet run --project 01-JsonSerialization
```

> Requires the **.NET 11 preview SDK** (union types are a C# 15 preview feature). See the
> [top-level README](../README.md) for setup instructions.
