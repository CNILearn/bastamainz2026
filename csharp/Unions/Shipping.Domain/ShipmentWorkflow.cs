namespace Shipping.Domain;

/// <summary>
/// Transitions a <see cref="ShipmentStatus"/> forward through its lifecycle. Shared by the
/// Minimal API and SignalR samples so both advance shipments the same way.
/// </summary>
public static class ShipmentWorkflow
{
    /// <summary>
    /// Computes the next <see cref="ShipmentStatus"/> for a shipment, or returns the same
    /// status unchanged if it's already final (<see cref="Delivered"/> or <see cref="Cancelled"/>).
    /// </summary>
    public static ShipmentStatus Advance(ShipmentStatus current, string trackingNumber) => current switch
    {
        Placed => new Processing(0),
        Processing(var percentComplete) when percentComplete < 100 =>
            new Processing(Math.Min(100, percentComplete + 50)),
        Processing => new Shipped("DHL Express", trackingNumber, DateTimeOffset.UtcNow),
        Shipped => new Delivered(DateTimeOffset.UtcNow),
        Delivered or Cancelled => current,
    };
}
