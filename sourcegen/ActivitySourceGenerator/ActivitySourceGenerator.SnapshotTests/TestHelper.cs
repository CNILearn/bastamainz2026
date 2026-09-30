namespace ActivitySourceGenerator.SnapshotTests;

public static class TestHelper
{
    /// <summary>
    /// Verifies the ActivitySourceGenerator output for a given source code.
    /// Updated to work with the latest .NET 9 interceptor changes:
    /// - Uses consistent language version (Preview) across all syntax trees
    /// - Configures generator driver with proper parse options
    /// - Suppresses interceptor-related warnings (CS9270, CS8795, CS0618)
    /// - Verifies the generated source files rather than the full driver result
    /// </summary>
    /// <param name="source">The source code to test with the generator</param>
    /// <returns>A task representing the verification process</returns>
    public static Task Verify(string source)
    {
        // Create the generator
        var generator = new ActivitySourceGeneratorImpl();

        // Create the compilation with interceptor support for .NET 9
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.ActivitySource).Assembly.Location),
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(
                OutputKind.ConsoleApplication, 
                allowUnsafe: false, 
                optimizationLevel: OptimizationLevel.Debug,
                specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic> 
                {
                    // Enable interceptors for the test and suppress warnings
                    ["CS0618"] = ReportDiagnostic.Suppress, // Suppress obsolete warnings for preview features
                    ["CS8795"] = ReportDiagnostic.Suppress, // Suppress interceptor preview warnings  
                    ["CS9270"] = ReportDiagnostic.Suppress, // Suppress legacy interceptor format warnings
                }));

        // Run the generator with parse options to ensure consistent language versions
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Get the generator result and extract the generated sources
        var runResult = driver.GetRunResult();
        var generatorResult = runResult.Results[0];
        
        // Create a cleaner object with just the source code for verification
        var sourcesToVerify = generatorResult.GeneratedSources
            .Select(source => new { 
                HintName = source.HintName, 
                SourceText = source.SourceText.ToString() 
            })
            .ToArray();
        
        // Use Verify to snapshot just the generated sources
        return Verifier.Verify(sourcesToVerify);
    }
}