using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shipping.Domain;

/// <summary>The order was placed and is waiting to enter fulfillment.</summary>
public sealed record Placed(DateTimeOffset PlacedAt);

/// <summary>The order is being picked, packed, or otherwise prepared for shipment.</summary>
public sealed record Processing(int PercentComplete);

/// <summary>The order has left the warehouse and is on its way to the customer.</summary>
public sealed record Shipped(string Carrier, string TrackingNumber, DateTimeOffset ShippedAt);

/// <summary>The order has arrived at its destination.</summary>
public sealed record Delivered(DateTimeOffset DeliveredAt);

/// <summary>The order was cancelled before delivery.</summary>
public sealed record Cancelled(string Reason);

/// <summary>
/// A shipment is in exactly one of these five states at any time. Modeling that as a
/// C# 15 union removes the need for a shared base type, a "Kind" discriminator enum, or
/// nullable properties that are only valid for some states.
/// </summary>
/// <remarks>
/// The <see cref="JsonUnionAttribute"/> plugs in <see cref="ShipmentStatusTypeClassifier"/>
/// so <c>System.Text.Json</c> can tell the five case types apart when deserializing a plain
/// JSON object (their property sets don't otherwise overlap, so no "$type" wrapper is needed).
/// </remarks>
[JsonUnion(TypeClassifier = typeof(ShipmentStatusTypeClassifier))]
public union ShipmentStatus(Placed, Processing, Shipped, Delivered, Cancelled);

/// <summary>
/// Tells <c>System.Text.Json</c> which <see cref="ShipmentStatus"/> case a JSON object belongs
/// to by inspecting its property names, since every case has a distinct shape.
/// </summary>
public sealed class ShipmentStatusTypeClassifier : JsonTypeClassifierFactory
{
    public override bool CanClassify(JsonTypeClassifierContext context) =>
        context.DeclaringType == typeof(ShipmentStatus);

    public override JsonTypeClassifier CreateJsonClassifier(JsonTypeClassifierContext context, JsonSerializerOptions options) =>
        (ref Utf8JsonReader reader) => Classify(reader);

    private static Type Classify(Utf8JsonReader reader)
    {
        reader.Read();
        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            // Property names are matched case-insensitively so this classifier works whether
            // the payload came from JsonSerializer's default PascalCase (01-JsonSerialization,
            // 02-MinimalApiOpenApi) or SignalR's camelCase JSON hub protocol (04-SignalR).
            Type? match = reader.GetString() switch
            {
                { } name when name.Equals(nameof(Placed.PlacedAt), StringComparison.OrdinalIgnoreCase) => typeof(Placed),
                { } name when name.Equals(nameof(Processing.PercentComplete), StringComparison.OrdinalIgnoreCase) => typeof(Processing),
                { } name when name.Equals(nameof(Shipped.TrackingNumber), StringComparison.OrdinalIgnoreCase) => typeof(Shipped),
                { } name when name.Equals(nameof(Delivered.DeliveredAt), StringComparison.OrdinalIgnoreCase) => typeof(Delivered),
                { } name when name.Equals(nameof(Cancelled.Reason), StringComparison.OrdinalIgnoreCase) => typeof(Cancelled),
                _ => null,
            };
            if (match is not null)
                return match;

            reader.Skip();
            reader.Read();
        }

        throw new JsonException("Unable to classify ShipmentStatus payload.");
    }
}
