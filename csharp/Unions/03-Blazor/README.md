# 03 · Blazor with Union Types

An interactive Blazor Server page (`Components/Pages/Home.razor`) drives its UI entirely from
the same C# 15 **union type**, `Shipping.Domain.ShipmentStatus`, used in the other samples.

## What it demonstrates

- **Recursive patterns in markup.** `@if (_current is Processing(var percentComplete))` and
  `@if (_current is Shipped(var carrier, var trackingNumber, _))` unwrap the union's contents
  directly inside Razor markup to conditionally render a progress bar or tracking details.
- **Exhaustive pattern matching for view logic.** `StatusCssClass` is a switch expression over
  all five cases (`Placed`, `Processing`, `Shipped`, `Delivered`, `Cancelled`) that picks a
  Bootstrap border class; the compiler rejects the switch if a case is left out.
- **Shared domain model.** The component reuses `ShipmentStatusReporter.Describe` and
  `SampleShipments.Timeline` from `Shipping.Domain`, so the same union that's serialized to JSON
  in `01-JsonSerialization` and returned from Minimal APIs in `02-MinimalApiOpenApi` also powers
  the UI here.

## Run it

```bash
dotnet run --project 03-Blazor
```

Open the app, then click **Advance** to step the shipment through `Placed` → `Processing` →
`Shipped` → `Delivered`, watching the card and progress bar update for each union case.

> Requires the **.NET 11 preview SDK**. See the [top-level README](../README.md) for setup.
