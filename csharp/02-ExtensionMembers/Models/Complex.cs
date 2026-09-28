namespace ExtensionBlocks.Models;

/// <summary>
/// Represents a complex number demonstrating C# 14 extension members with user-defined operators.
/// </summary>
public record Complex(double Real, double Imaginary)
{
    /// <summary>
    /// String representation of the complex number.
    /// </summary>
    public override string ToString()
    {
        if (Imaginary == 0) return Real.ToString("F2");
        if (Real == 0) return $"{Imaginary:F2}i";

        var sign = Imaginary >= 0 ? "+" : "-";
        return $"{Real:F2} {sign} {Math.Abs(Imaginary):F2}i";
    }
}

