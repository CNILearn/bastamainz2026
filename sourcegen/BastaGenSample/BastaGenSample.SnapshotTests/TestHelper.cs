using System.Runtime.CompilerServices;

namespace BastaGenSample.SnapshotTests;

/// <summary>
/// Helper that compiles source code with the generator and snapshot-verifies the output.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Runs <see cref="BastaGenSampleImpl"/> against <paramref name="source"/>,
    /// then uses Verify to snapshot the generated sources.
    /// <para>
    /// On the first run Verify creates <c>.verified.txt</c> files in the
    /// <c>Snapshots/</c> directory. Subsequent runs compare against those files.
    /// To update a snapshot after an intentional change run:
    /// <code>dotnet test -- Verify.UseClipboard=false Verify.AutoVerify=true</code>
    /// or set <c>VERIFY_AUTO_APPROVE=true</c> as an environment variable.
    /// </para>
    /// </summary>
    public static Task Verify(string source)
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
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var runResult = driver.GetRunResult();
        var generatorResult = runResult.Results[0];

        // Verify only the generated source text (stable across SDK/Roslyn versions)
        var sourcesToVerify = generatorResult.GeneratedSources
            .Select(s => new { HintName = s.HintName, Source = s.SourceText.ToString() })
            .ToArray();

        return Verifier.Verify(sourcesToVerify)
            .UseDirectory("Snapshots");
    }
}

/// <summary>
/// Initializes the module by configuring VerifierSettings to normalize newlines when reading content.
/// </summary>
/// <remarks>Invoked automatically at module load time via the ModuleInitializer attribute to ensure consistent
/// newline handling for verification tests.</remarks>
public static class Initializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifierSettings.FixNewlinesOnRead();
}