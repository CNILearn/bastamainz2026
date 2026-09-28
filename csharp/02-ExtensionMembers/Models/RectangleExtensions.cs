namespace ExtensionBlocks.Models;

/// <summary>
/// C# 14 Extension Block for Rectangle demonstrating geometric extension properties.
/// Shows how extension properties can add calculated geometric properties to simple data types.
/// 
/// Uses the actual C# 14 extension syntax that compiles and runs in .NET 10.
/// </summary>
public static class RectangleExtensions
{
    extension (Rectangle rectangle)
    {
        /// <summary>
        /// Extension property that calculates the area of the rectangle.
        /// Basic geometric calculation using extension properties.
        /// </summary>
        public double Area => rectangle.Width * rectangle.Height;

        /// <summary>
        /// Extension property that calculates the perimeter of the rectangle.
        /// Another basic geometric calculation demonstrating multiple extension properties.
        /// </summary>
        public double Perimeter => 2 * (rectangle.Width + rectangle.Height);

        /// <summary>
        /// Extension property that calculates the diagonal length of the rectangle.
        /// More complex calculation using Pythagorean theorem in extension properties.
        /// </summary>
        public double Diagonal => Math.Sqrt(rectangle.Width * rectangle.Width + rectangle.Height * rectangle.Height);

        /// <summary>
        /// Extension property that determines if the rectangle is a square.
        /// Boolean extension property with conditional logic.
        /// </summary>
        public bool IsSquare => Math.Abs(rectangle.Width - rectangle.Height) < 0.0001;

        /// <summary>
        /// Extension property that calculates the aspect ratio of the rectangle.
        /// Demonstrates division and ratio calculations in extension properties.
        /// </summary>
        public double AspectRatio => rectangle.Height == 0 ? 0 : rectangle.Width / rectangle.Height;

        /// <summary>
        /// Extension property that categorizes the rectangle orientation.
        /// String extension property with conditional logic based on dimensions.
        /// </summary>
        public string Orientation => rectangle.IsSquare ? "Square" : rectangle.Width > rectangle.Height ? "Landscape" : "Portrait";

        /// <summary>
        /// Extension property that provides a formatted description of the rectangle.
        /// Combines multiple extension properties into a comprehensive description.
        /// </summary>
        public string Description => 
            $"Rectangle: {rectangle.Width:F1} × {rectangle.Height:F1} ({rectangle.Orientation})\n" +
            $"Area: {rectangle.Area:F2}, Perimeter: {rectangle.Perimeter:F2}, Diagonal: {rectangle.Diagonal:F2}\n" +
            $"Aspect Ratio: {rectangle.AspectRatio:F2}";
    }
}