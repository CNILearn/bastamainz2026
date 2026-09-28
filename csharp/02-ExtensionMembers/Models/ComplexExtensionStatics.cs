namespace ExtensionBlocks.Models;

/// <summary>
/// Additional utility extension methods for Complex numbers.
/// Demonstrates advanced mathematical operations using C# 14 extension members.
/// </summary>
public static partial class ComplexExtensions
{
    extension (Complex) // extension receiver type only
    {
        /// <summary>
        /// Extension operator for complex number addition.
        /// Demonstrates basic arithmetic operators in extension members.
        /// </summary>
        public static Complex operator +(Complex left, Complex right)
            => new(left.Real + right.Real, left.Imaginary + right.Imaginary);

        /// <summary>
        /// Extension operator for complex number subtraction.
        /// Shows how multiple operators can be defined in the same extension block.
        /// </summary>
        public static Complex operator -(Complex left, Complex right)
            => new(left.Real - right.Real, left.Imaginary - right.Imaginary);

        /// <summary>
        /// Extension operator for complex number multiplication.
        /// Demonstrates more complex mathematical operations in extension operators.
        /// Formula: (a + bi)(c + di) = (ac - bd) + (ad + bc)i
        /// </summary>
        public static Complex operator *(Complex left, Complex right)
            => new(
                left.Real * right.Real - left.Imaginary * right.Imaginary,
                left.Real * right.Imaginary + left.Imaginary * right.Real
            );

        /// <summary>
        /// Extension operator for complex number division.
        /// Shows advanced mathematical operations with proper error handling.
        /// Formula: (a + bi)/(c + di) = [(ac + bd) + (bc - ad)i] / (c² + d²)
        /// </summary>
        public static Complex operator /(Complex left, Complex right)
        {
            var denominator = right.Real * right.Real + right.Imaginary * right.Imaginary;

            if (Math.Abs(denominator) < double.Epsilon)
                throw new DivideByZeroException("Cannot divide by zero complex number");

            return new(
                (left.Real * right.Real + left.Imaginary * right.Imaginary) / denominator,
                (left.Imaginary * right.Real - left.Real * right.Imaginary) / denominator
            );
        }

        /// <summary>
        /// Extension operator for scalar multiplication.
        /// Demonstrates operators with different parameter types.
        /// </summary>
        public static Complex operator *(Complex complex, double scalar)
            => new(complex.Real * scalar, complex.Imaginary * scalar);

        /// <summary>
        /// Extension operator for scalar multiplication (commutative).
        /// Shows how to make operations commutative with multiple operator overloads.
        /// </summary>
        public static Complex operator *(double scalar, Complex complex)
            => complex * scalar;

        /// <summary>
        /// Extension operator for unary negation.
        /// Demonstrates unary operators in extension members.
        /// </summary>
        public static Complex operator -(Complex complex)
            => new(-complex.Real, -complex.Imaginary);

        /// <summary>
        /// Creates a complex number from polar coordinates.
        /// Static method demonstrating polar to rectangular conversion.
        /// </summary>
        public static Complex FromPolar(double magnitude, double phase)
            => new(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
    }

    extension (ref Complex complex)
    {
        /// <summary>
        /// Compound assignment operator for complex addition.
        /// Performs in-place component updates to minimize temporary values.
        /// </summary>
        public void operator +=(Complex right)
        {
            complex.Real += right.Real;
            complex.Imaginary += right.Imaginary;
        }

        /// <summary>
        /// Compound assignment operator for scalar multiplication.
        /// Performs in-place scaling to avoid creating intermediate results.
        /// </summary>
        public void operator *=(double scalar)
        {
            complex.Real *= scalar;
            complex.Imaginary *= scalar;
        }
    }
}
