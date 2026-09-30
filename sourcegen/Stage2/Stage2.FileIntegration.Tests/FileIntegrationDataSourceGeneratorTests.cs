using Xunit;

namespace Stage2.FileIntegration.Tests;

public class FileIntegrationDataSourceGeneratorTests
{
    [Fact]
    public void GeneratesDataFactoryForSimpleClassWithoutExternalFiles()
    {
        var source = """
            using Stage2.FileIntegration.Attributes;
            using System;

            namespace TestNamespace;

            [DataSource(EntityName = "User", Count = 5)]
            public class User
            {
                public string Name { get; set; }
                public int Age { get; set; }
                public bool IsActive { get; set; }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Verify the generated source contains expected elements
        Assert.Contains("public static class UserDataFactory", generatedSource);
        Assert.Contains("CreateSample()", generatedSource);
        Assert.Contains("CreateSamples(int count = 5)", generatedSource);
        Assert.Contains("GetGeneratorInfo()", generatedSource);
        Assert.Contains("GetConfigurationInfo()", generatedSource);
        
        // Verify Stage 2 characteristics
        Assert.Contains("Stage 2: File Integration Data Source Generator", generatedSource);
        Assert.Contains("Multiple data sources", generatedSource);
        Assert.Contains("No external configuration found", generatedSource);
    }

    [Fact]
    public void GeneratesDataFactoryWithExternalConfiguration()
    {
        var source = """
            using Stage2.FileIntegration.Attributes;
            using System;

            namespace TestNamespace;

            [DataSource(EntityName = "User", Count = 5)]
            public class User
            {
                public string Name { get; set; }
                public string Email { get; set; }
                public int Age { get; set; }
            }
            """;

        var configFile = """
            {
              "entityType": "User",
              "defaultCount": 7,
              "templates": {
                "Name": ["Alice", "Bob", "Carol"],
                "Email": ["alice@test.com", "bob@test.com", "carol@test.com"]
              }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source, 
            ("User.datasource.json", configFile));
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Verify external configuration is used
        Assert.Contains("External configuration: User.datasource.json", generatedSource);
        Assert.Contains("Templates: 2", generatedSource);
        
        // Verify template-based generation
        Assert.Contains("new[] { \"Alice\", \"Bob\", \"Carol\" }", generatedSource);
        Assert.Contains("new[] { \"alice@test.com\", \"bob@test.com\", \"carol@test.com\" }", generatedSource);
    }

    [Fact]
    public void HandlesInvalidJsonConfiguration()
    {
        var source = """
            using Stage2.FileIntegration.Attributes;

            namespace TestNamespace;

            [DataSource]
            public class TestClass
            {
                public string Name { get; set; }
            }
            """;

        var invalidConfig = "{ invalid json content }";

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source, 
            ("TestClass.datasource.json", invalidConfig));
        
        // Should still generate code but with warning
        Assert.Contains("public static class TestClassDataFactory", generatedSource);
        Assert.Contains("No external configuration found", generatedSource);
        
        // Should have a warning diagnostic about invalid JSON
        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => d.Id == "DSG001");
    }

    [Fact]
    public void GeneratesAttributeSourceWithConfigurationFile()
    {
        var source = """
            using System;

            namespace TestNamespace;

            public class EmptyClass
            {
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Verify the DataSource attribute is generated with ConfigurationFile property
        Assert.Contains("DataSourceAttribute", generatedSource);
        Assert.Contains("EntityName { get; set; }", generatedSource);
        Assert.Contains("Count { get; set; }", generatedSource);
        Assert.Contains("ConfigurationFile { get; set; }", generatedSource);
        Assert.Contains("Stage2.FileIntegration.Attributes", generatedSource);
    }

    [Fact]
    public void HandlesMultipleClassesWithDifferentConfigurations()
    {
        var source = """
            using Stage2.FileIntegration.Attributes;

            namespace TestNamespace;

            [DataSource(EntityName = "User")]
            public class User
            {
                public string Name { get; set; }
            }

            [DataSource(EntityName = "Product")]
            public class Product
            {
                public string Name { get; set; }
                public decimal Price { get; set; }
            }
            """;

        var userConfig = """
            {
              "entityType": "User",
              "templates": {
                "Name": ["Alice", "Bob"]
              }
            }
            """;

        var productConfig = """
            {
              "entityType": "Product",
              "templates": {
                "Name": ["Laptop", "Phone"]
              }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source, 
            ("User.datasource.json", userConfig),
            ("Product.datasource.json", productConfig));
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Verify both factories are generated
        Assert.Contains("public static class UserDataFactory", generatedSource);
        Assert.Contains("public static class ProductDataFactory", generatedSource);
        
        // Verify different configurations are applied
        Assert.Contains("new[] { \"Alice\", \"Bob\" }", generatedSource);
        Assert.Contains("new[] { \"Laptop\", \"Phone\" }", generatedSource);
    }

    [Fact]
    public void IgnoresNonDataSourceJsonFiles()
    {
        var source = """
            using Stage2.FileIntegration.Attributes;

            namespace TestNamespace;

            [DataSource]
            public class TestClass
            {
                public string Name { get; set; }
            }
            """;

        var regularJsonFile = """
            {
              "someOtherConfig": "value"
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source, 
            ("config.json", regularJsonFile), // Not a .datasource.json file
            ("other.config.json", regularJsonFile)); // Not a .datasource.json file
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Should generate code but without external configuration
        Assert.Contains("public static class TestClassDataFactory", generatedSource);
        Assert.Contains("No external configuration found", generatedSource);
    }
}