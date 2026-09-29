# 02 · Minimal APIs + OpenAPI with Union Types

This sample exposes `Shipping.Domain.ShipmentStatus` — the same C# 15 **union type** used in
`01-JsonSerialization` — directly as a minimal API response type, and shows how
`Microsoft.AspNetCore.OpenApi` documents it.

## Endpoints

| Method | Route                        | Returns                                            |
|--------|------------------------------|-----------------------------------------------------|
| GET    | `/shipments/{id}`            | `Results<Ok<ShipmentStatus>, NotFound>`             |
| GET    | `/shipments/{id}/summary`    | Human-readable text from `ShipmentStatusReporter`   |
| POST   | `/shipments/{id}/advance`    | The shipment's next `ShipmentStatus` union case     |

Try it, e.g. with the seeded `order-2` shipment (currently `Shipped`):

```bash
dotnet run --project 02-MinimalApiOpenApi
curl http://localhost:<port>/shipments/order-2
curl http://localhost:<port>/shipments/order-2/summary
curl -X POST http://localhost:<port>/shipments/order-2/advance
```

## What it demonstrates

- **Union types as Minimal API return types.** The endpoint delegate's return type includes
  `ShipmentStatus` directly (via `Ok<ShipmentStatus>`); ASP.NET Core serializes whichever case
  is active using the same `System.Text.Json` union support as `01-JsonSerialization`.
- **OpenAPI schema generation for unions.** `GET /openapi/v1.json` documents the `ShipmentStatus`
  schema as an `anyOf` of its five case types:

  ```json
  "ShipmentStatus": {
    "type": "object",
    "anyOf": [
      { "$ref": "#/components/schemas/Placed" },
      { "$ref": "#/components/schemas/Processing" },
      { "$ref": "#/components/schemas/Shipped" },
      { "$ref": "#/components/schemas/Delivered" },
      { "$ref": "#/components/schemas/Cancelled" }
    ]
  }
  ```

- **Exhaustive pattern matching to advance state.** `AdvanceShipment` switches over the current
  `ShipmentStatus` to compute the next one; the compiler enforces that every case (including the
  `when` guard on `Processing`) is handled.

> Requires the **.NET 11 preview SDK**. See the [top-level README](../README.md) for setup.
