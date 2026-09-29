# C# 15 Extension Members Sample

This sample demonstrates **C# extension members** using the modern `extension` keyword syntax that compiles and runs in .NET 11. It shows how to extend existing types with properties, methods, indexers, and operators without modifying the original type definition.

> **Note**: This sample uses extension member syntax as specified in the [Microsoft proposal](https://raw.githubusercontent.com/dotnet/csharplang/main/proposals/csharp-14.0/extensions.md). It demonstrates true extension properties (for example `person.FullName`) and C# 15 extension indexers (for example `person["fullName"]`).

## 🚀 Features

- **Extension Properties**: Add computed properties to existing types
- **Extension Methods**: Enhance types with additional functionality
- **Extension Indexers**: Add key-based indexed access to existing types
- **Extension Operators**: Organize mathematical operations in clean blocks
- **Type Safety**: Full IntelliSense support and compile-time checking
- **Performance**: Better performance than traditional extension methods
- **Organization**: Logical grouping of related extensions

## 📋 Prerequisites

- **.NET 11 SDK** (preview)
- **Visual Studio 2026** or **VS Code** with C# extension
- **C# 15 Language Support**

## 🏗️ Sample Structure

```
ExtensionMethods/
├── Models/
│   ├── Person.cs                    # Extension properties for person calculations
│   ├── Rectangle.cs                 # Extension properties for geometric calculations  
│   ├── StringExtensions.cs          # Extension methods for string manipulation
│   ├── CollectionExtensions.cs      # Extension methods for collection operations
│   ├── Complex.cs                   # Complex numbers with extension operators
│   └── Vector3D.cs                  # 3D vectors with mathematical operators
├── Program.cs                       # Comprehensive demonstration
├── ExtensionBlocks.csproj          # Project configuration
└── README.md                       # This documentation
```

## 🔧 Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/cnilearn/BastaMainz2026.git
   cd Dotnet10Samples/src/ExtensionBlocks
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the sample**:
   ```bash
   dotnet run
   ```

## 💡 Key Concepts

### Extension Properties

Extension properties allow adding computed properties to existing types:

```csharp
// C# 14 Extension Block syntax
public static class PersonExtensions
{
    extension (Person person)
    {
        public string FullName => $"{person.FirstName} {person.LastName}";
        public int Age => CalculateAge(person.DateOfBirth);
        public double BMI => person.WeightInKg / (person.HeightInMeters * person.HeightInMeters);
    }
}

// Usage - accessed as true properties in C# 14
var person = new Person { FirstName = "John", LastName = "Doe" };
Console.WriteLine(person.FullName); // "John Doe" - property access, not method call!
Console.WriteLine(person.Age);      // Calculated age - property access!
```

### Extension Methods

Extension methods provide additional functionality to existing types:

```csharp
// C# 14 Extension Block syntax
public static class StringExtensions
{
    extension (string input)
    {
        public string ToTitleCase() { /* implementation */ }
        public int WordCount() { /* implementation */ }
        public bool IsValidEmail() { /* implementation */ }
    }
}

// Usage
var text = "hello world";
Console.WriteLine(text.ToTitleCase()); // "Hello World"
Console.WriteLine(text.WordCount());   // 2
```

### Extension Indexers

Extension indexers add indexed access patterns to existing types:

```csharp
public static class PersonExtensions
{
    extension (Person person)
    {
        public string this[string fieldName] => fieldName.ToLowerInvariant() switch
        {
            "fullname" => person.FullName,
            "age" => person.Age.ToString(),
            "bmi" => person.BMI.ToString("F1"),
            _ => throw new ArgumentException($"Unsupported person field '{fieldName}'.", nameof(fieldName))
        };
    }
}

// Usage
Console.WriteLine(person["fullName"]); // "John Doe"
Console.WriteLine(person["bmi"]);      // "22.9"
```

### User-Defined Operators

Extension blocks can organize mathematical operations for custom types:

```csharp
// Operators are still defined on the type itself
public record Complex(double Real, double Imaginary)
{
    public static Complex operator +(Complex left, Complex right) { /* implementation */ }
    public static Complex operator *(Complex left, Complex right) { /* implementation */ }
}

// C# 14 Extension Block for additional functionality
public static class ComplexExtensions
{
    extension (Complex complex)
    {
        public double Magnitude => Math.Sqrt(complex.Real * complex.Real + complex.Imaginary * complex.Imaginary);
        public double Phase => Math.Atan2(complex.Imaginary, complex.Real);
        public Complex Conjugate() => new(complex.Real, -complex.Imaginary);
    }
}

// Usage
var c1 = new Complex(3, 4);
var c2 = new Complex(1, 2);
var result = c1 + c2;              // Uses operator
var magnitude = c1.Magnitude;      // Uses extension property (not c1.Magnitude()!)
```

## 🎯 Use Cases Demonstrated

### 1. Person Calculations
- **FullName**: Concatenate first and last name
- **Age**: Calculate age from birth date
- **BMI**: Calculate Body Mass Index
- **BMICategory**: Categorize BMI into health ranges
- **IsAdult**: Check if person is 18 or older
- **Indexer Access**: Read values like `person["fullName"]` and `person["bmiCategory"]`

### 2. Geometric Calculations
- **Area**: Calculate rectangle area
- **Perimeter**: Calculate rectangle perimeter
- **Diagonal**: Calculate diagonal length using Pythagorean theorem
- **AspectRatio**: Calculate width-to-height ratio
- **Orientation**: Determine if rectangle is landscape, portrait, or square

### 3. String Manipulation
- **ToTitleCase**: Convert to title case formatting
- **WordCount**: Count words in string
- **ToAlphanumericOnly**: Remove non-alphanumeric characters
- **Truncate**: Truncate with ellipsis
- **IsValidEmail**: Validate email format
- **Reverse**: Reverse string characters
- **GetInitials**: Extract initials from full name

### 4. Collection Operations
- **IsNullOrEmpty**: Safe null/empty checking
- **GetRandomElement**: Get random element from collection
- **ChunkBy**: Split collection into chunks
- **Statistics**: Calculate mean, median, mode, standard deviation

### 5. Complex Number Mathematics
- **Arithmetic**: Addition, subtraction, multiplication, division
- **Scalar Operations**: Multiplication with real numbers
- **Magnitude**: Calculate absolute value
- **Phase**: Calculate argument/angle
- **Polar Conversion**: Convert to/from polar coordinates
- **Advanced Operations**: Power, square root, conjugate

### 6. 3D Vector Operations
- **Vector Arithmetic**: Addition, subtraction, scalar multiplication
- **Geometric Properties**: Magnitude, normalization, unit checking
- **Vector Products**: Dot product, cross product
- **Geometric Operations**: Distance, angle, projection, reflection

## 🔍 Sample Operations

### Extension Properties Demo
```
👤 Person Extension Properties:
   Name: John Doe
   Age: 35 years
   BMI: 22.9 (Normal weight)
   Is Adult: True
   Vital Stats: John Doe: 35 years old, BMI: 22.9 (Normal weight)

📐 Rectangle Extension Properties:
   Dimensions: 10 × 6
   Area: 60.00, Perimeter: 32.00, Diagonal: 11.66
   Aspect Ratio: 1.67, Orientation: Landscape
```

### Extension Methods Demo
```
📝 String Extension Methods:
   Original: 'hello world programming'
   Title Case: 'Hello World Programming'
   Word Count: 3
   Alphanumeric Only: 'helloworldprogramming'
   Truncated (10 chars): 'hello w...'
   Reversed: 'gnimmargorp dlrow olleh'

📊 Collection Statistics:
   Count: 10, Mean: 5.50, Median: 5.50
   Standard Deviation: 2.87, Range: 1 - 10
```

### User-Defined Operators Demo
```
🔢 Complex Number Operators:
   c1 = 3.00 + 4.00i
   c2 = 1.00 + 2.00i
   c1 + c2 = 4.00 + 6.00i
   c1 * c2 = -5.00 + 10.00i
   c1 magnitude: 5.00, phase: 0.93 radians

📐 Vector3D Operators:
   v1 = (1.00, 2.00, 3.00)
   v2 = (4.00, 5.00, 6.00)
   v1 + v2 = (5.00, 7.00, 9.00)
   Dot Product: 32.00, Angle: 12.9°
```

## 🚀 Advanced Features

### Multiple Extension Blocks
C# 14 allows multiple extension members in the same file and class, enabling clean organization of related functionality:

```csharp
public static class Vector3DExtensions
{
    // Mathematical operations extension block
    extension (Vector3D vector)
    {
        public double Magnitude => Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        public bool IsUnit => Math.Abs(Magnitude - 1.0) < 0.0001;
        public Vector3D Normalize() { /* implementation */ }
    }
}

// Separate class for static math operations
public static class Vector3DMathOperators
{
    public static double DotProduct(Vector3D left, Vector3D right) { /* implementation */ }
    public static Vector3D CrossProduct(Vector3D left, Vector3D right) { /* implementation */ }
}
```

### True Extension Properties
Unlike traditional extension methods, C# 14 extension properties are accessed as true properties:

```csharp
var person = new Person { FirstName = "John", LastName = "Doe" };

// C# 14 Extension Properties (true property access)
var name = person.FullName;     // Not person.FullName()
var age = person.Age;           // Not person.Age()
var bmi = person.BMI;           // Not person.BMI()
```

### Type Safety
Extension blocks provide full compile-time checking and IntelliSense support, making them safer and more discoverable than traditional extension methods.

### Performance Benefits
- **No boxing**: Value types remain unboxed
- **Better inlining**: Compiler can optimize more effectively
- **Reduced allocations**: More efficient memory usage

## 📊 Sample Output

The application demonstrates all extension block features with comprehensive output showing:

- Extension properties calculations for Person and Rectangle types
- String manipulation using various extension methods
- Collection operations with statistical analysis
- Complex number arithmetic with advanced mathematical operations
- 3D vector mathematics with geometric calculations

## 🔗 Related Technologies

- **C# 14 Language Features**: Extension blocks, enhanced pattern matching
- **Record Types**: Modern C# data modeling with immutability
- **System.Numerics**: Mathematical operations and complex numbers
- **LINQ**: Collection operations and functional programming
- **Nullable Reference Types**: Enhanced type safety

## 📚 Additional Resources

- [C# 14 Extension Blocks Proposal](https://github.com/dotnet/csharplang/issues/5497)
- [C# 14 Documentation](https://docs.microsoft.com/dotnet/csharp/whats-new/csharp-14)
- [Extension Members](https://docs.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Operator Overloading](https://docs.microsoft.com/dotnet/csharp/language-reference/operators/operator-overloading)
- [.NET 10 Release Notes](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-10)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.
