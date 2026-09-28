using ExtensionBlocks.Models;


/// <summary>
/// Demonstrates C# 15 Extension Members features including extension properties,
/// extension methods, extension indexers, and user-defined operators in extension members.
/// 
/// Extension blocks provide a clean, organized way to extend existing types
/// with properties, methods, indexers, and operators without modifying the original type.
/// </summary>
/// 
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("🚀 C# 15 Extension Members Demo - .NET 11");
Console.WriteLine("=========================================");
Console.WriteLine();
Console.WriteLine("Extension members provide a clean way to extend existing types with:");
Console.WriteLine("• Extension Properties - Add computed properties to any type");
Console.WriteLine("• Extension Methods - Add functionality without inheritance");
Console.WriteLine("• Extension Indexers - Add key-based member access");
Console.WriteLine("• User-Defined Operators - Add mathematical operations in organized blocks");
Console.WriteLine();

// Demonstrate Extension Properties
await DemonstrateExtensionProperties();
Console.WriteLine();

// Demonstrate Extension Methods
await DemonstrateExtensionMethods();
Console.WriteLine();

// Demonstrate User-Defined Operators
await DemonstrateUserDefinedOperators();
Console.WriteLine();

Console.WriteLine("✅ C# 15 Extension Members demonstration completed!");
Console.WriteLine();
Console.WriteLine("🔍 Key Benefits:");
Console.WriteLine("• Clean organization of related extensions");
Console.WriteLine("• Type safety and IntelliSense support");
Console.WriteLine("• Better performance than traditional extension methods");
Console.WriteLine("• Improved code readability and maintainability");
Console.WriteLine("• Seamless integration with existing code");


static async Task DemonstrateExtensionProperties()
{
    Console.WriteLine("📊 Extension Properties Demo");
    Console.WriteLine("===========================");
    Console.WriteLine();

    // Person extension properties
    var person = new Person
    {
        FirstName = "John",
        LastName = "Doe",
        DateOfBirth = new DateTime(1990, 5, 15),
        HeightInMeters = 1.75,
        WeightInKg = 70.0
    };

    Console.WriteLine("👤 Person Extension Properties:");
    Console.WriteLine($"   Name: {person.FullName}");
    Console.WriteLine($"   Age: {person.Age} years");
    Console.WriteLine($"   BMI: {person.BMI:F1} ({person.BMICategory})");
    Console.WriteLine($"   Is Adult: {person.IsAdult}");
    Console.WriteLine($"   Vital Stats: {person.VitalStats}");
    Console.WriteLine($"   Indexer[\"fullName\"]: {person["fullName"]}");
    Console.WriteLine($"   Indexer[\"bmiCategory\"]: {person["bmiCategory"]}");
    Console.WriteLine();

    // Rectangle extension properties
    var rectangle = new Rectangle(10.0, 6.0);
    Console.WriteLine("📐 Rectangle Extension Properties:");
    Console.WriteLine($"   Dimensions: {rectangle.Width} × {rectangle.Height}");
    Console.WriteLine($"   Area: {rectangle.Area:F2}");
    Console.WriteLine($"   Perimeter: {rectangle.Perimeter:F2}");
    Console.WriteLine($"   Diagonal: {rectangle.Diagonal:F2}");
    Console.WriteLine($"   Is Square: {rectangle.IsSquare}");
    Console.WriteLine($"   Orientation: {rectangle.Orientation}");
    Console.WriteLine($"   Aspect Ratio: {rectangle.AspectRatio:F2}");
    Console.WriteLine();
    Console.WriteLine(rectangle.Description);

    await Task.Delay(1); // Simulate async operation
}

static async Task DemonstrateExtensionMethods()
{
    Console.WriteLine("🔧 Extension Methods Demo");
    Console.WriteLine("========================");
    Console.WriteLine();

    // String extension methods
    var text = "hello world programming";
    Console.WriteLine("📝 String Extension Methods:");
    Console.WriteLine($"   Original: '{text}'");
    Console.WriteLine($"   Title Case: '{text.ToTitleCase()}'");
    Console.WriteLine($"   Word Count: {text.WordCount()}");
    Console.WriteLine($"   Alphanumeric Only: '{text.ToAlphanumericOnly()}'");
    Console.WriteLine($"   Truncated (10 chars): '{text.Truncate(10)}'");
    Console.WriteLine($"   Reversed: '{text.Reverse()}'");
    Console.WriteLine($"   Initials: '{text.GetInitials()}'");
    Console.WriteLine($"   'o' occurrences: {text.CountOccurrences("o")}");
    Console.WriteLine();

    var email = "user@example.com";
    Console.WriteLine($"   Email '{email}' is valid: {email.IsValidEmail()}");
    Console.WriteLine();

    // Collection extension methods
    var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    Console.WriteLine("📊 Collection Extension Methods:");
    Console.WriteLine($"   Numbers: [{string.Join(", ", numbers)}]");
    Console.WriteLine($"   Is Null or Empty: {numbers.IsNullOrEmpty()}");
    Console.WriteLine($"   Random Element: {numbers.GetRandomElement()}");
    Console.WriteLine($"   Random Elements (3): [{string.Join(", ", numbers.GetRandomElements(3))}]");
    Console.WriteLine();

    Console.WriteLine("   Chunked by 3:");
    foreach (var chunk in numbers.ChunkBy(3))
    {
        Console.WriteLine($"     [{string.Join(", ", chunk)}]");
    }
    Console.WriteLine();

    // Numeric collection statistics
    var stats = numbers.GetStatistics();
    Console.WriteLine($"   Statistics:");
    Console.WriteLine($"     Count: {stats.Count}");
    Console.WriteLine($"     Mean: {stats.Mean:F2}");
    Console.WriteLine($"     Median: {stats.Median:F2}");
    Console.WriteLine($"     Mode: {stats.Mode}");
    Console.WriteLine($"     Standard Deviation: {stats.StdDev:F2}");
    Console.WriteLine($"     Range: {stats.Min} - {stats.Max}");

    await Task.Delay(1); // Simulate async operation
}

static async Task DemonstrateUserDefinedOperators()
{
    Console.WriteLine("⚡ User-Defined Operators Demo");
    Console.WriteLine("=============================");
    Console.WriteLine();

    // Complex number operators
    Console.WriteLine("🔢 Complex Number Operators:");
    var c1 = new Complex(3, 4);
    var c2 = new Complex(1, 2);

    Console.WriteLine($"   c1 = {c1}");
    Console.WriteLine($"   c2 = {c2}");
    Console.WriteLine($"   c1 + c2 = {c1 + c2}");
    Console.WriteLine($"   c1 - c2 = {c1 - c2}");
    Console.WriteLine($"   c1 * c2 = {c1 * c2}");
    Console.WriteLine($"   c1 / c2 = {c1 / c2}");
    Console.WriteLine($"   c1 * 2.5 = {c1 * 2.5}");
    Console.WriteLine($"   -c1 = {-c1}");
    Console.WriteLine($"   c1 == c2 = {c1 == c2}");
    Console.WriteLine();

    // Compound assignment demonstration
    var c3 = new Complex(2, 3);
    Console.WriteLine("   Compound Assignment Operators:");
    Console.WriteLine($"   c3 = {c3}");
    c3 += c1;
    Console.WriteLine($"   After c3 += c1: {c3}");
    c3 *= 2.0;
    Console.WriteLine($"   After c3 *= 2.0: {c3}");
    Console.WriteLine();

    // Complex number extension methods
    Console.WriteLine("   Complex Number Extension Methods:");
    Console.WriteLine($"   c1 magnitude: {c1.Magnitude:F2}");
    Console.WriteLine($"   c1 phase: {c1.Phase:F2} radians");
    Console.WriteLine($"   c1 conjugate: {c1.Conjugate()}");
    Console.WriteLine($"   c1 to polar: {c1.ToPolar()}");
    Console.WriteLine($"   c1 squared: {c1.Power(2)}");
    Console.WriteLine($"   c1 square root: {c1.SquareRoot()}");
    Console.WriteLine();

    // Vector3D operators
    Console.WriteLine("📐 Vector3D Operators:");
    var v1 = new Vector3D(1, 2, 3);
    var v2 = new Vector3D(4, 5, 6);

    Console.WriteLine($"   v1 = {v1}");
    Console.WriteLine($"   v2 = {v2}");
    Console.WriteLine($"   v1 + v2 = {v1 + v2}");
    Console.WriteLine($"   v1 - v2 = {v1 - v2}");
    Console.WriteLine($"   v1 * 2.5 = {v1 * 2.5}");
    Console.WriteLine($"   v1 / 2.0 = {v1 / 2.0}");
    Console.WriteLine($"   -v1 = {-v1}");
    Console.WriteLine();

    // Vector geometric properties and methods
    Console.WriteLine("   Vector Geometric Properties:");
    Console.WriteLine($"   v1 Magnitude = {v1.Magnitude:F2}");
    Console.WriteLine($"   v1 Is Unit = {v1.IsUnit}");
    Console.WriteLine($"   v1 Is Zero = {v1.IsZero}");
    Console.WriteLine($"   v1 Normalized = {v1.Normalize()}");
    Console.WriteLine($"   Distance v1 to v2 = {v1.DistanceTo(v2):F2}");
    Console.WriteLine($"   Angle v1 to v2 = {v1.AngleTo(v2) * 180 / Math.PI:F1}°");
    Console.WriteLine();

    Console.WriteLine("   Vector Operations:");
    Console.WriteLine($"   Dot Product v1·v2 = {Vector3D.DotProduct(v1, v2):F2}");
    Console.WriteLine($"   Cross Product v1×v2 = {Vector3D.CrossProduct(v1, v2)}");
    Console.WriteLine($"   v1 projected onto v2 = {v1.ProjectOnto(v2)}");

    var normal = new Vector3D(0, 1, 0); // Y-axis
    Console.WriteLine($"   v1 reflected across {normal} = {v1.Reflect(normal)}");

    await Task.Delay(1); // Simulate async operation
}
