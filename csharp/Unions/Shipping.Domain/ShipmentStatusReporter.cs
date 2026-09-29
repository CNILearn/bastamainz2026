namespace Shipping.Domain;

/// <summary>
/// Renders a <see cref="ShipmentStatus"/> for humans by switching over its case types.
/// Because <see cref="ShipmentStatus"/> is a union with a fixed, closed set of cases, the
/// compiler flags this switch as an error if a case is missing an arm - no "default" needed.
/// </summary>
public static class ShipmentStatusReporter
{
    public static string Describe(ShipmentStatus status) => status switch
    {
        Placed(var placedAt) =>
            $"Order placed on {placedAt:d}.",
        Processing(var percentComplete) =>
            $"Preparing your order ({percentComplete}% complete).",
        Shipped(var carrier, var trackingNumber, var shippedAt) =>
            $"Shipped via {carrier} on {shippedAt:d} (tracking #{trackingNumber}).",
        Delivered(var deliveredAt) =>
            $"Delivered on {deliveredAt:d}.",
        Cancelled(var reason) =>
            $"Cancelled: {reason}",
    };

    /// <summary>True once a shipment can no longer change state.</summary>
    public static bool IsFinal(ShipmentStatus status) => status switch
    {
        Delivered => true,
        Cancelled => true,
        Placed or Processing or Shipped => false,
    };
}
