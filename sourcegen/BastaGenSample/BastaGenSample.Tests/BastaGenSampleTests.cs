namespace BastaGenSample.Tests;

public class BastaGenSampleTests
{
    [Fact]
    public void GeneratesInfoClassForSimpleClass()
    {
        var source = """
            using BastaGenSample.Attributes;

            namespace TestNamespace;

            [GenerateInfo]
            public class Person
            {
                public string Name { get; set; } = string.Empty;
                public int Age { get; set; }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static class PersonInfo", generatedSource);
        Assert.Contains("TypeName = \"TestNamespace.Person\"", generatedSource);
        Assert.Contains("ClassName = \"Person\"", generatedSource);
        Assert.Contains("Namespace = \"TestNamespace\"", generatedSource);
        Assert.Contains("PropertyCount = 2", generatedSource);
    }

    [Fact]
    public void GeneratesInfoClassWithCorrectMethodCount()
    {
        var source = """
            using BastaGenSample.Attributes;

            namespace TestNamespace;

            [GenerateInfo]
            public class Calculator
            {
                public int Add(int a, int b) => a + b;
                public int Subtract(int a, int b) => a - b;
                private int Multiply(int a, int b) => a * b;
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Contains("MethodCount = 2", generatedSource);
    }

    [Fact]
    public void GeneratesGetSummaryMethod()
    {
        var source = """
            using BastaGenSample.Attributes;

            namespace TestNamespace;

            [GenerateInfo]
            public class SampleClass
            {
                public string Value { get; set; } = string.Empty;
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static string GetSummary()", generatedSource);
    }

    [Fact]
    public void DoesNotGenerateForUnmarkedClass()
    {
        var source = """
            namespace TestNamespace;

            public class UnmarkedClass
            {
                public string Value { get; set; } = string.Empty;
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        // Only the injected attribute source file should be generated
        Assert.DoesNotContain("UnmarkedClassInfo", generatedSource);
    }

    [Fact]
    public void GeneratesInfoClassForGlobalNamespaceClass()
    {
        var source = """
            using BastaGenSample.Attributes;

            [GenerateInfo]
            public class GlobalPerson
            {
                public string Name { get; set; } = string.Empty;
                public int Age { get; set; }
            }
            """;

        var (generatedSource, diagnostics) = TestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Contains("public static class GlobalPersonInfo", generatedSource);
        // No namespace declaration should be emitted for global namespace types
        Assert.DoesNotContain("namespace ;", generatedSource);
        Assert.DoesNotContain("namespace Global", generatedSource);
        // TypeName constant should be just the class name
        Assert.Contains("TypeName = \"GlobalPerson\"", generatedSource);
    }
}
