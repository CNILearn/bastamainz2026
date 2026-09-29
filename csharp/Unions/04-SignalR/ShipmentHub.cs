using Microsoft.AspNetCore.SignalR;

namespace Shipping.SignalR;

/// <summary>Strongly typed client callbacks the hub can invoke.</summary>
public interface IShipmentClient
{
    /// <summary>Notifies a client that a shipment's status changed to a new union case.</summary>
    Task ShipmentUpdated(string shipmentId, Shipping.Domain.ShipmentStatus status);
}

/// <summary>
/// Broadcasts <see cref="Shipping.Domain.ShipmentStatus"/> union updates to connected clients.
/// The hub method's parameter and return types include the union directly - the default
/// SignalR JSON hub protocol serializes it with the same <c>System.Text.Json</c> union support
/// used in <c>01-JsonSerialization</c> and <c>02-MinimalApiOpenApi</c>.
/// </summary>
public sealed class ShipmentHub(ShipmentTracker tracker) : Hub<IShipmentClient>
{
    /// <summary>Returns a shipment's current status without changing it.</summary>
    public Shipping.Domain.ShipmentStatus GetStatus(string shipmentId) =>
        tracker.GetStatus(shipmentId);

    /// <summary>Advances a shipment to its next status and broadcasts the update to every client.</summary>
    public async Task Advance(string shipmentId)
    {
        Shipping.Domain.ShipmentStatus next = tracker.Advance(shipmentId);
        await Clients.All.ShipmentUpdated(shipmentId, next);
    }
}
