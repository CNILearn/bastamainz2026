namespace Shipping.Domain;

/// <summary>Sample data shared by the union samples so each one tells the same story.</summary>
public static class SampleShipments
{
    /// <summary>The lifecycle of a single order, one <see cref="ShipmentStatus"/> update at a time.</summary>
    public static IReadOnlyList<ShipmentStatus> Timeline { get; } =
    [
        new Placed(new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero)),
        new Processing(35),
        new Processing(80),
        new Shipped("DHL Express", "DHL-4711-DE", new DateTimeOffset(2026, 9, 2, 14, 30, 0, TimeSpan.Zero)),
        new Delivered(new DateTimeOffset(2026, 9, 4, 11, 15, 0, TimeSpan.Zero)),
    ];

    /// <summary>An order that didn't make it to delivery, for the "closed set" cases that end early.</summary>
    public static ShipmentStatus Cancelled { get; } = new Cancelled("Item no longer in stock");
}
