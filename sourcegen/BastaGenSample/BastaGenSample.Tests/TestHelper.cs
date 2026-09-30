using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BastaGenSample.Tests;

/// <summary>
/// Helper that compiles source code with the generator and returns the results.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Runs <see cref="BastaGenSampleImpl"/> against <paramref name="source"/> and returns
    /// the first generated source file text together with any diagnostic messages.
    /// </summary>
    public static (string GeneratedSource, string[] Diagnostics) RunGenerator(string source)
    {
        var generator = new BastaGenSampleImpl();

        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        PortableExecutableReference[] references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
        ];

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        var runResult = driver.GetRunResult();

        // Skip the injected attribute file; return the first generated companion class
        var generated = runResult.Results[0].GeneratedSources
            .FirstOrDefault(s => !s.HintName.Contains("Attribute"));

        var generatedSource = generated.SourceText?.ToString() ?? string.Empty;
        var diagnosticMessages = diagnostics.Select(d => d.ToString()).ToArray();

        return (generatedSource, diagnosticMessages);
    }
}
