using System.Text.Json;
using Shipping.Domain;

var options = new JsonSerializerOptions { WriteIndented = true };

Console.WriteLine("== Serializing every case of the ShipmentStatus union ==\n");

foreach (ShipmentStatus status in SampleShipments.Timeline)
{
    // JsonSerializer knows how to write a union out of the box: it serializes
    // whichever case is currently active, with no wrapper object or "Kind" property.
    string json = JsonSerializer.Serialize(status, options);
    Console.WriteLine(json);
    Console.WriteLine($"  -> {ShipmentStatusReporter.Describe(status)}\n");
}

Console.WriteLine("== Round-tripping through JSON ==\n");

foreach (ShipmentStatus status in SampleShipments.Timeline)
{
    string json = JsonSerializer.Serialize(status);

    // Deserializing a union needs to know which case type a JSON object belongs to.
    // Because the five ShipmentStatus cases have distinct property sets, the custom
    // ShipmentStatusTypeClassifier (see Shipping.Domain/ShipmentStatus.cs) can pick the
    // right case just by looking at the property names - no "$type" discriminator needed.
    ShipmentStatus roundTripped = JsonSerializer.Deserialize<ShipmentStatus>(json)!;

    bool matches = ShipmentStatusReporter.Describe(status) == ShipmentStatusReporter.Describe(roundTripped);
    Console.WriteLine($"{json} -> round-trip matches: {matches}");
}

Console.WriteLine();
Console.WriteLine("== An unrecognized payload throws instead of guessing ==\n");

try
{
    string json = """{ "SomeUnknownProperty": true }""";
    JsonSerializer.Deserialize<ShipmentStatus>(json);
}
catch (JsonException ex)
{
    Console.WriteLine($"JsonException: {ex.Message}");
}
