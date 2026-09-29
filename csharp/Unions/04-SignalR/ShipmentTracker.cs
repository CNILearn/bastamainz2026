using System.Collections.Concurrent;
using Shipping.Domain;

namespace Shipping.SignalR;

/// <summary>A tiny in-memory shipment store, shared by every connected SignalR client.</summary>
public sealed class ShipmentTracker
{
    private readonly ConcurrentDictionary<string, ShipmentStatus> _shipments = new()
    {
        ["order-1"] = SampleShipments.Timeline[0], // Placed
        ["order-2"] = SampleShipments.Timeline[3], // Shipped
    };

    public ShipmentStatus GetStatus(string shipmentId) =>
        _shipments.TryGetValue(shipmentId, out ShipmentStatus status) ? status : new Placed(DateTimeOffset.UtcNow);

    public ShipmentStatus Advance(string shipmentId)
    {
        ShipmentStatus fallback = new Placed(DateTimeOffset.UtcNow);
        // AddOrUpdate re-invokes the update factory if another thread wins the race, so
        // concurrent Advance calls for the same shipment never lose an update.
        return _shipments.AddOrUpdate(
            shipmentId,
            addValueFactory: _ => ShipmentWorkflow.Advance(fallback, trackingNumber: $"{shipmentId}-TRACK"),
            updateValueFactory: (_, current) => ShipmentWorkflow.Advance(current, trackingNumber: $"{shipmentId}-TRACK"));
    }
}
