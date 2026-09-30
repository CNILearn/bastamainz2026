using Xunit;

namespace Stage1.Basic.Tests;

public class BasicDataSourceGeneratorTests
{
    [Fact]
    public void GeneratesDataFactoryForSimpleClass()
    {
        var source = """
            using Stage1.Basic.Attributes;
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
        
        // Verify property assignments are generated
        Assert.Contains("Name = ", generatedSource);
        Assert.Contains("Age = ", generatedSource);
        Assert.Contains("IsActive = ", generatedSource);
        
        // Verify Stage 1 characteristics
        Assert.Contains("Stage 1: Basic Data Source Generator", generatedSource);
        Assert.Contains("No caching", generatedSource);
    }

    [Fact]
    public void GeneratesDataFactoryForComplexClass()
    {
        var source = """
            using Stage1.Basic.Attributes;
            using System;

            namespace TestNamespace;

            public enum Status
            {
                Active,
                Inactive,
                Pending
            }

            [DataSource(EntityName = "Product", Count = 10)]
            public class Product
            {
                public string Name { get; set; }
                public decimal Price { get; set; }
                public Guid Id { get; set; }
                public DateTime? LastUpdated { get; set; }
                public Status Status { get; set; }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Verify the generated source contains expected elements
        Assert.Contains("public static class ProductDataFactory", generatedSource);
        Assert.Contains("CreateSamples(int count = 10)", generatedSource);
        
        // Verify complex type handling
        Assert.Contains("Price = ", generatedSource);
        Assert.Contains("Id = Guid.NewGuid()", generatedSource);
        Assert.Contains("LastUpdated = ", generatedSource);
        Assert.Contains("Status = ", generatedSource);
    }

    [Fact]
    public void GeneratesAttributeSource()
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
        
        // Verify the DataSource attribute is generated
        Assert.Contains("DataSourceAttribute", generatedSource);
        Assert.Contains("EntityName { get; set; }", generatedSource);
        Assert.Contains("Count { get; set; }", generatedSource);
        Assert.Contains("Stage1.Basic.Attributes", generatedSource);
    }

    [Fact]
    public void HandlesClassWithoutDataSourceAttribute()
    {
        var source = """
            using System;

            namespace TestNamespace;

            public class RegularClass
            {
                public string Name { get; set; }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Should only generate the attribute, not any data factory
        Assert.Contains("DataSourceAttribute", generatedSource);
        Assert.DoesNotContain("RegularClassDataFactory", generatedSource);
    }

    [Fact]
    public void GeneratesCorrectGeneratorInfo()
    {
        var source = """
            using Stage1.Basic.Attributes;

            namespace TestNamespace;

            [DataSource]
            public class TestClass
            {
                public string Name { get; set; }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);
        
        // Verify no diagnostics/errors
        Assert.Empty(diagnostics);
        
        // Verify generator info describes Stage 1 characteristics
        Assert.Contains("Stage 1: Basic Data Source Generator - No caching, single data source (attributes)", generatedSource);
    }
}