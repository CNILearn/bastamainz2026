using Stage2.FileIntegration.Attributes;

namespace Stage2.FileIntegration.Sample;

/// <summary>
/// Sample entity for demonstrating Stage 2: File Integration Data Source Generator
/// This class uses external JSON configuration for enhanced data generation
/// </summary>
[DataSource(EntityName = "User", Count = 5, ConfigurationFile = "User.datasource.json")]
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
/// Sample product entity with external configuration for realistic data
/// </summary>
[DataSource(EntityName = "Product", Count = 8, ConfigurationFile = "Product.datasource.json")]
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

/// <summary>
/// Entity without external configuration file to show fallback behavior
/// </summary>
[DataSource(EntityName = "Order", Count = 3)]
public class Order
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public bool IsProcessed { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Stage 2: File Integration Data Source Generator Demo ===");
        Console.WriteLine();
        
        // Display generator information
        Console.WriteLine("Generator Info:");
        Console.WriteLine(UserDataFactory.GetGeneratorInfo());
        Console.WriteLine();
        
        Console.WriteLine("Generated Users (with external configuration):");
        Console.WriteLine("==============================================");
        
        var users = UserDataFactory.CreateSamples();
        foreach (var user in users)
        {
            Console.WriteLine($"Name: {user.Name}, Email: {user.Email}, Age: {user.Age}, Role: {user.Role}, Active: {user.IsActive}");
        }
        
        Console.WriteLine();
        Console.WriteLine("User Configuration Info:");
        Console.WriteLine(UserDataFactory.GetConfigurationInfo());
        Console.WriteLine();
        
        Console.WriteLine("Generated Products (with external configuration):");
        Console.WriteLine("===============================================");
        
        var products = ProductDataFactory.CreateSamples();
        foreach (var product in products)
        {
            Console.WriteLine($"Product: {product.Name}");
            Console.WriteLine($"  Description: {product.Description}");
            Console.WriteLine($"  Price: ${product.Price:F2}, Stock: {product.StockQuantity}, Category: {product.Category}");
            Console.WriteLine();
        }
        
        Console.WriteLine("Product Configuration Info:");
        Console.WriteLine(ProductDataFactory.GetConfigurationInfo());
        Console.WriteLine();
        
        Console.WriteLine("Generated Orders (fallback to basic generation):");
        Console.WriteLine("===============================================");
        
        var orders = OrderDataFactory.CreateSamples();
        foreach (var order in orders)
        {
            Console.WriteLine($"Order: {order.OrderNumber}, Total: ${order.Total:F2}, Date: {order.OrderDate:yyyy-MM-dd}, Processed: {order.IsProcessed}");
        }
        
        Console.WriteLine();
        Console.WriteLine("Order Configuration Info:");
        Console.WriteLine(OrderDataFactory.GetConfigurationInfo());
        Console.WriteLine();
        
        Console.WriteLine("Stage 2 Characteristics:");
        Console.WriteLine("- Multiple data sources: Attributes + JSON configuration files");
        Console.WriteLine("- External file integration: JSON templates for realistic data");
        Console.WriteLine("- Graceful fallback: Basic generation when no external config found");
        Console.WriteLine("- Still no caching: File operations performed every build");
        
        Console.WriteLine();
        Console.WriteLine("Performance Note: Stage 2 reads and parses JSON files during every");
        Console.WriteLine("compilation. Stage 3 will introduce caching to improve build performance.");
    }
}