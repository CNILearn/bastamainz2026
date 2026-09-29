using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.HttpResults;
using Shipping.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

// A tiny in-memory store, seeded from the same sample timeline used by the other samples.
// ConcurrentDictionary lets /advance update a shipment atomically even under concurrent requests.
ConcurrentDictionary<string, ShipmentStatus> shipments = new()
{
    ["order-1"] = SampleShipments.Timeline[^1],   // Delivered
    ["order-2"] = SampleShipments.Timeline[3],    // Shipped
    ["order-3"] = SampleShipments.Cancelled,      // Cancelled
};

// The union type is the endpoint's return type: ASP.NET Core serializes whichever case is
// active with the same JSON support shown in 01-JsonSerialization, and Microsoft.AspNetCore.OpenApi
// documents the response schema as "anyOf" the five case types - see /openapi/v1.json.
app.MapGet("/shipments/{id}", Results<Ok<ShipmentStatus>, NotFound> (string id) =>
    shipments.TryGetValue(id, out ShipmentStatus status)
        ? TypedResults.Ok(status)
        : TypedResults.NotFound())
    .WithName("GetShipmentStatus")
    .WithSummary("Gets the current status of a shipment as a ShipmentStatus union.");

app.MapGet("/shipments-new/{id}", ShipmentStatusResult (string id) =>
    shipments.TryGetValue(id, out ShipmentStatus status)
        ? TypedResults.Ok(status)
        : TypedResults.NotFound())
    .WithName("GetShipmentStatusNew")
    .WithSummary("Gets the current status of a shipment as a ShipmentStatus union.");


// A second endpoint that reads the union with exhaustive pattern matching (see
// ShipmentStatusReporter.Describe in Shipping.Domain) to render a human-readable summary.
app.MapGet("/shipments/{id}/summary", Results<Ok<string>, NotFound> (string id) =>
    shipments.TryGetValue(id, out ShipmentStatus status)
        ? TypedResults.Ok(ShipmentStatusReporter.Describe(status))
        : TypedResults.NotFound())
    .WithName("GetShipmentSummary")
    .WithSummary("Gets a human-readable summary of a shipment's status.");

// Advances a shipment to its next state and returns the new ShipmentStatus union case directly.
app.MapPost("/shipments/{id}/advance", Results<Ok<ShipmentStatus>, NotFound, BadRequest<string>> (string id) =>
{
    // TryUpdate's compare-and-swap makes the read-compute-write sequence atomic: if another
    // request changes the shipment first, the loop retries against the fresh value instead of
    // silently overwriting it.
    while (true)
    {
        if (!shipments.TryGetValue(id, out ShipmentStatus current))
            return TypedResults.NotFound();

        if (ShipmentStatusReporter.IsFinal(current))
            return TypedResults.BadRequest($"Shipment '{id}' is already {ShipmentStatusReporter.Describe(current)}");

        ShipmentStatus next = ShipmentWorkflow.Advance(current, trackingNumber: $"{id}-TRACK");

        if (shipments.TryUpdate(id, next, current))
            return TypedResults.Ok(next);
    }
})
    .WithName("AdvanceShipment")
    .WithSummary("Advances a shipment to its next ShipmentStatus.");

app.Run();

internal union ShipmentStatusResult(Ok<ShipmentStatus>, NotFound);