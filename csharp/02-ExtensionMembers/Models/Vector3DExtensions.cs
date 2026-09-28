namespace ExtensionBlocks.Models;

/// <summary>
/// Represents a 3D vector demonstrating C# 14 extension members with mathematical operators.
/// 
/// </summary>
public static class Vector3DOperators
{
    extension(Vector3D)
    {

        /// <summary>
        /// Extension operator for vector addition.
        /// Basic vector arithmetic demonstrating component-wise operations.
        /// </summary>
        public static Vector3D operator +(Vector3D left, Vector3D right)
            => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        /// <summary>
        /// Extension operator for vector subtraction.
        /// Shows component-wise subtraction in extension operators.
        /// </summary>
        public static Vector3D operator -(Vector3D left, Vector3D right)
            => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        /// <summary>
        /// Extension operator for scalar multiplication.
        /// Demonstrates scaling vectors with scalar values.
        /// </summary>
        public static Vector3D operator *(Vector3D vector, double scalar)
            => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);

        /// <summary>
        /// Extension operator for scalar multiplication (commutative).
        /// Shows commutative scalar multiplication.
        /// </summary>
        public static Vector3D operator *(double scalar, Vector3D vector)
            => vector * scalar;

        /// <summary>
        /// Extension operator for scalar division.
        /// Demonstrates vector scaling with division and error handling.
        /// </summary>
        public static Vector3D operator /(Vector3D vector, double scalar)
        {
            if (Math.Abs(scalar) < double.Epsilon)
                throw new DivideByZeroException("Cannot divide vector by zero");

            return new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
        }

        /// <summary>
        /// Extension operator for unary negation.
        /// Shows unary operators for vector direction reversal.
        /// </summary>
        public static Vector3D operator -(Vector3D vector)
            => new(-vector.X, -vector.Y, -vector.Z);

        /// <summary>
        /// Extension operator for dot product.
        /// Demonstrates custom mathematical operations for vectors.
        /// </summary>
        public static double DotProduct(Vector3D left, Vector3D right)
            => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

        /// <summary>
        /// Extension operator for cross product.
        /// Shows complex vector operations in extension methods.
        /// </summary>
        public static Vector3D CrossProduct(Vector3D left, Vector3D right)
            => new(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X
            );
    }
}

/// <summary>
/// Extension methods for Vector3D demonstrating geometric properties and methods.
/// Shows how multiple extension classes can extend the same type with different concerns.
/// 
/// Uses the actual C# 14 extension syntax that compiles and runs in .NET 10 RC 1.
/// Extension properties like Magnitude, IsUnit, IsZero are accessed as true properties:
/// vector.Magnitude (not vector.Magnitude()).
/// </summary>
public static class Vector3DGeometry
{
    extension(Vector3D vector)
    {
        /// <summary>
        /// Extension property that calculates the magnitude (length) of the vector.
        /// Demonstrates extension properties with mathematical calculations.
        /// </summary>
        public double Magnitude => Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);

        /// <summary>
        /// Extension property that calculates the squared magnitude.
        /// Useful for performance when only relative magnitudes are needed.
        /// </summary>
        public double MagnitudeSquared => vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;

        /// <summary>
        /// Extension property that checks if the vector is a unit vector.
        /// Boolean property demonstrating magnitude-based calculations.
        /// </summary>
        public bool IsUnit => Math.Abs(vector.Magnitude - 1.0) < 0.0001;

        /// <summary>
        /// Extension property that checks if the vector is a zero vector.
        /// Shows floating-point comparison in extension properties.
        /// </summary>
        public bool IsZero => Math.Abs(vector.X) < double.Epsilon &&
                             Math.Abs(vector.Y) < double.Epsilon &&
                             Math.Abs(vector.Z) < double.Epsilon;

        /// <summary>
        /// Extension method that returns a normalized (unit) vector.
        /// Demonstrates vector normalization with proper error handling.
        /// </summary>
        public Vector3D Normalize()
        {
            var magnitude = vector.Magnitude;
            if (Math.Abs(magnitude) < double.Epsilon)
                throw new InvalidOperationException("Cannot normalize a zero vector");

            return vector / magnitude;
        }

        /// <summary>
        /// Extension method that calculates the distance to another vector.
        /// Shows how extension methods can work with multiple instances.
        /// </summary>
        public double DistanceTo(Vector3D other)
        {
            var diff = vector - other;
            return diff.Magnitude;
        }

        /// <summary>
        /// Extension method that calculates the angle between two vectors.
        /// Advanced geometric calculation using dot product and arc cosine.
        /// </summary>
        public double AngleTo(Vector3D other)
        {
            var dot = Vector3D.DotProduct(vector, other);
            var magnitudes = vector.Magnitude * other.Magnitude;

            if (Math.Abs(magnitudes) < double.Epsilon)
                throw new InvalidOperationException("Cannot calculate angle with zero vector");

            var cosAngle = Math.Clamp(dot / magnitudes, -1.0, 1.0);
            return Math.Acos(cosAngle);
        }

        /// <summary>
        /// Extension method that projects this vector onto another vector.
        /// Demonstrates vector projection calculations in extension methods.
        /// </summary>
        public Vector3D ProjectOnto(Vector3D other)
        {
            var otherMagnitudeSquared = other.MagnitudeSquared;
            if (Math.Abs(otherMagnitudeSquared) < double.Epsilon)
                throw new InvalidOperationException("Cannot project onto zero vector");

            var scale = Vector3D.DotProduct(vector, other) / otherMagnitudeSquared;
            return other * scale;
        }

        /// <summary>
        /// Extension method that reflects this vector across a normal vector.
        /// Advanced geometric transformation demonstrating reflection formula.
        /// </summary>
        public Vector3D Reflect(Vector3D normal)
        {
            var normalizedNormal = normal.Normalize();
            var dot = Vector3D.DotProduct(vector, normalizedNormal);
            return vector - 2 * dot * normalizedNormal;
        }
    }
}