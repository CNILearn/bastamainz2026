using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Stage2.FileIntegration.Tests;

public static class TestHelper
{
    public static (string generatedSource, ImmutableArray<Diagnostic> diagnostics) RunGenerator(
        string source, 
        params (string fileName, string content)[] additionalFiles)
    {
        // Parse the input source
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references (similar to a real compilation)
        PortableExecutableReference[] references = [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
        ];

        // Create the compilation
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Create additional text files
        var additionalTexts = additionalFiles
            .Select(f => new TestAdditionalText(f.fileName, f.content))
            .ToImmutableArray<AdditionalText>();

        // Create the source generator
        var generator = new FileIntegrationDataSourceGenerator();

        // Create the generator driver
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            additionalTexts: additionalTexts);

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

    private class TestAdditionalText(string path, string content) : AdditionalText
    {
        private readonly string _content = content;

        public override string Path { get; } = path;

        public override SourceText? GetText(System.Threading.CancellationToken cancellationToken = default)
        {
            return SourceText.From(_content, Encoding.UTF8);
        }
    }
}