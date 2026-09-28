namespace ExtensionBlocks.Models;

/// <summary>
/// Additional utility extension methods for Complex numbers.
/// Demonstrates advanced mathematical operations using C# 14 extension members.
/// </summary>
public static partial class ComplexExtensions
{
    extension (Complex complex)
    {
        /// <summary>
        /// Calculates the magnitude (absolute value) of the complex number.
        /// Extension property for complex magnitude calculation.
        /// </summary>
        public double Magnitude => Math.Sqrt(complex.Real * complex.Real + complex.Imaginary * complex.Imaginary);

        /// <summary>
        /// Calculates the phase (argument) of the complex number.
        /// Extension property for complex number phase calculation.
        /// </summary>
        public double Phase => Math.Atan2(complex.Imaginary, complex.Real);
    }
}