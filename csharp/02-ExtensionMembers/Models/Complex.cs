namespace ExtensionBlocks.Models;

/// <summary>
/// Represents a complex number demonstrating C# 15 extension members with user-defined operators.
/// Implemented as a value type to minimize allocations in arithmetic-heavy scenarios.
/// </summary>
public record struct Complex(double Real, double Imaginary)
{
    /// <summary>
    /// String representation of the complex number.
    /// </summary>
    public override readonly string ToString()
    {
        if (Imaginary == 0) return Real.ToString("F2");
        if (Real == 0) return $"{Imaginary:F2}i";

        var sign = Imaginary >= 0 ? "+" : "-";
        return $"{Real:F2} {sign} {Math.Abs(Imaginary):F2}i";
    }
}

