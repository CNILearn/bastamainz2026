using Stage1.Basic.Attributes;

namespace Stage1.Basic.Sample;

/// <summary>
/// Sample entity for demonstrating Stage 1: Basic Data Source Generator
/// </summary>
[DataSource(EntityName = "User", Count = 5)]
public class User
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }
}

public enum UserRole
{
    Guest,
    User,
    Admin,
    SuperAdmin
}

/// <summary>
/// Sample product entity with more complex properties
/// </summary>
[DataSource(EntityName = "Product", Count = 8)]
public class Product
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid Id { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? LastUpdated { get; set; }
    public ProductCategory Category { get; set; }
    public double Weight { get; set; }
    public float Rating { get; set; }
}

public enum ProductCategory
{
    Electronics,
    Clothing,
    Books,
    Home,
    Sports,
    Toys
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Stage 1: Basic Data Source Generator Demo ===");
        Console.WriteLine();
        
        // Display generator information
        Console.WriteLine("Generator Info:");
        Console.WriteLine(UserDataFactory.GetGeneratorInfo());
        Console.WriteLine();
        
        Console.WriteLine("Generated Users:");
        Console.WriteLine("================");
        
        var users = UserDataFactory.CreateSamples();
        foreach (var user in users)
        {
            Console.WriteLine($"Name: {user.Name}, Email: {user.Email}, Age: {user.Age}, Role: {user.Role}, Active: {user.IsActive}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Generated Products:");
        Console.WriteLine("==================");
        
        var products = ProductDataFactory.CreateSamples();
        foreach (var product in products)
        {
            Console.WriteLine($"Product: {product.Name}, Price: ${product.Price:F2}, Stock: {product.StockQuantity}, Category: {product.Category}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Stage 1 Characteristics:");
        Console.WriteLine("- No caching: Data generation logic runs every build");
        Console.WriteLine("- Single data source: Only uses attribute configuration");
        Console.WriteLine("- Simple implementation: Baseline for performance comparison");
        Console.WriteLine("- All data values are randomly generated at runtime");
        
        Console.WriteLine();
        Console.WriteLine("Performance Note: In Stage 1, all generation logic is recalculated");
        Console.WriteLine("during every compilation. Future stages will show caching benefits.");
    }
}