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
        /// Converts the complex number to polar form.
        /// Extension method returning tuple for polar representation.
        /// </summary>
        public (double Magnitude, double Phase) ToPolar() => (complex.Magnitude, complex.Phase);

        /// <summary>
        /// Raises the complex number to a power.
        /// Extension method for complex exponentiation using De Moivre's theorem.
        /// </summary>
        public Complex Power(double exponent)
        {
            var (magnitude, phase) = complex.ToPolar();
            var newMagnitude = Math.Pow(magnitude, exponent);
            var newPhase = phase * exponent;
            return Complex.FromPolar(newMagnitude, newPhase);
        }

        /// <summary>
        /// Calculates the square root of the complex number.
        /// Extension method for complex square root calculation.
        /// </summary>
        public Complex SquareRoot()
        {
            var magnitude = complex.Magnitude;
            var phase = complex.Phase;
            var sqrtMagnitude = Math.Sqrt(magnitude);
            return Complex.FromPolar(sqrtMagnitude, phase / 2);
        }

        /// <summary>
        /// Returns the complex conjugate of the number.
        /// Extension method demonstrating complex number operations.
        /// </summary>
        public Complex Conjugate() => new(complex.Real, -complex.Imaginary);
    }
}