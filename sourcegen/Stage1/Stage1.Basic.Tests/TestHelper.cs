using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Immutable;
using System.Linq;

namespace Stage1.Basic.Tests;

public static class TestHelper
{
    public static (string generatedSource, ImmutableArray<Diagnostic> diagnostics) RunGenerator(string source)
    {
        // Parse the input source
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references (similar to a real compilation)
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
        };

        // Create the compilation
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create the source generator
        var generator = new BasicDataSourceGenerator();

        // Create the generator driver
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the generator
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Get the result
        var result = driver.GetRunResult();
            
        // Get generated sources
        var generatedSources = result.Results[0].GeneratedSources;
        
        // Combine all generated source files
        var combinedSource = string.Join("\n\n", generatedSources.Select(s => s.SourceText.ToString()));

        return (combinedSource, result.Diagnostics);
    }
}