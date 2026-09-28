namespace ExtensionBlocks.Models;

/// <summary>
/// Represents a 3D vector demonstrating C# 14 extension members with mathematical operators.
/// 
/// </summary>
public record Vector3D(double X, double Y, double Z)
{
    /// <summary>
    /// String representation of the 3D vector.
    /// </summary>
    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
}
