using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace Stage2.FileIntegration;

/// <summary>
/// Stage 2: File Integration Data Source Generator
/// 
/// This implementation demonstrates:
/// - Combining multiple data sources (attributes + external files)
/// - Using AdditionalFiles provider for external data
/// - JSON configuration file processing
/// - Enhanced data generation with external templates
/// 
/// Performance characteristics:
/// - Still no caching: Provides baseline for Stage 3 comparison
/// - Multiple data sources: Attributes + JSON configuration files
/// - File I/O operations: Reading and parsing external files every build
/// </summary>
[Generator]
public class FileIntegrationDataSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the DataSource attribute for post-initialization
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "DataSourceAttribute.g.cs", SourceText.From(DataSourceAttributeSource, Encoding.UTF8)));

        // Find classes with DataSource attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m != null)
            .Collect();

        // Get additional files (JSON configuration files)
        var additionalFiles = context.AdditionalTextsProvider.Collect();

        // Combine class declarations with additional files
        var combinedProvider = classDeclarations.Combine(additionalFiles);

        // Generate code for each class with access to additional files
        context.RegisterSourceOutput(combinedProvider, Execute);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static ClassInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol == null)
            return null;

        // Check if the class has the DataSource attribute
        var dataSourceAttribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "DataSourceAttribute");

        if (dataSourceAttribute == null)
            return null;

        return new ClassInfo(
            classSymbol,
            classDeclaration,
            dataSourceAttribute);
    }

    private static void Execute(SourceProductionContext context, 
        (ImmutableArray<ClassInfo?> Classes, ImmutableArray<AdditionalText> AdditionalFiles) input)
    {
        var (classes, additionalFiles) = input;
        
        if (classes.IsDefaultOrEmpty)
            return;

        ClassInfo?[] validClasses = [.. classes.Where(c => c != null)];
        
        // Parse configuration files
        var configurations = ParseConfigurationFiles(additionalFiles, context);
        
        foreach (var classInfo in validClasses)
        {
            if (classInfo != null)
            {
                var source = GenerateDataClass(classInfo, configurations);
                context.AddSource($"{classInfo.Symbol.Name}_Generated.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    private static DataSourceConfiguration[] ParseConfigurationFiles(
        ImmutableArray<AdditionalText> additionalFiles,
        SourceProductionContext context)
    {
        List<DataSourceConfiguration> configurations = [];
        
        foreach (var file in additionalFiles)
        {
            if (file.Path.EndsWith(".datasource.json"))
            {
                try
                {
                    var content = file.GetText(context.CancellationToken)?.ToString();
                    if (!string.IsNullOrEmpty(content))
                    {
                        var config = ParseSimpleJson(content!);
                        
                        if (config != null)
                        {
                            config.SourceFile = Path.GetFileName(file.Path);
                            configurations.Add(config);
                        }
                        else
                        {
                            // JSON parsing failed - report diagnostic
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "DSG001",
                                    "Invalid JSON Configuration",
                                    $"Failed to parse JSON configuration file '{file.Path}': Invalid JSON format",
                                    "DataSourceGenerator",
                                    DiagnosticSeverity.Warning,
                                    true),
                                Location.None));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "DSG001",
                            "Invalid JSON Configuration",
                            $"Failed to parse JSON configuration file '{file.Path}': {ex.Message}",
                            "DataSourceGenerator",
                            DiagnosticSeverity.Warning,
                            true),
                        Location.None));
                }
            }
        }
        
        return [.. configurations];
    }

    private static DataSourceConfiguration? ParseSimpleJson(string content)
    {
        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            var config = new DataSourceConfiguration();

            // Parse entityType
            if (root.TryGetProperty("entityType", out var entityTypeElement))
            {
                config.EntityType = entityTypeElement.GetString() ?? string.Empty;
            }

            // Parse defaultCount
            if (root.TryGetProperty("defaultCount", out var defaultCountElement))
            {
                if (defaultCountElement.TryGetInt32(out var count))
                {
                    config.DefaultCount = count;
                }
            }

            // Parse templates object
            if (root.TryGetProperty("templates", out var templatesElement))
            {
                var templates = new Dictionary<string, string[]>();

                foreach (var property in templatesElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        var values = new List<string>();
                        foreach (var arrayElement in property.Value.EnumerateArray())
                        {
                            if (arrayElement.ValueKind == JsonValueKind.String)
                            {
                                var value = arrayElement.GetString();
                                if (value != null)
                                {
                                    values.Add(value);
                                }
                            }
                        }

                        if (values.Count > 0)
                        {
                            templates[property.Name] = values.ToArray();
                        }
                    }
                }

                if (templates.Count > 0)
                {
                    config.Templates = templates;
                }
            }

            return config;
        }
        catch (JsonException)
        {
            // Return null for invalid JSON - will be handled by caller
            return null;
        }
        catch (System.Exception)
        {
            // Return null for any other parsing errors
            return null;
        }
    }

    private static string GenerateDataClass(ClassInfo classInfo, DataSourceConfiguration[] configurations)
    {
        var className = classInfo.Symbol.Name;
        var namespaceName = classInfo.Symbol.ContainingNamespace.ToDisplayString();
        
        // Extract configuration from attribute
        var entityName = GetAttributeValue<string>(classInfo.AttributeData, "EntityName") ?? className;
        var count = GetAttributeValue<int>(classInfo.AttributeData, "Count");
        if (count == 0) count = 10; // Default value

        // Find matching configuration file
        var fileConfig = configurations.FirstOrDefault(c => 
            string.Equals(c.EntityType, className, System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(c.EntityType, entityName, System.StringComparison.OrdinalIgnoreCase));

        var properties = classInfo.Symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod != null && !p.IsStatic)
            .ToArray();

        var propertyGenerators = properties.Select(p => GeneratePropertyAssignment(p, fileConfig)).ToArray();

        var configurationInfo = fileConfig != null 
            ? $"External configuration: {fileConfig.SourceFile}, Templates: {fileConfig.Templates?.Count ?? 0}"
            : "No external configuration found";

        return $$"""
            // <auto-generated/>
            // Stage 2: File Integration Data Source Generator
            // Generated from: {{className}}
            // {{configurationInfo}}
            // No caching - file operations performed every build
            #nullable enable

            using System;
            using System.Collections.Generic;
            using System.Linq;

            namespace {{namespaceName}};

            /// <summary>
            /// Generated data factory for {{entityName}}
            /// Stage 2: File integration with external data sources
            /// </summary>
            public static class {{className}}DataFactory
            {
                private static readonly Random _random = new();

                /// <summary>
                /// Creates a single sample {{entityName}}
                /// </summary>
                public static {{className}} CreateSample()
                {
                    return new {{className}}
                    {
                        {{string.Join(",\n            ", propertyGenerators)}}
                    };
                }

                /// <summary>
                /// Creates multiple sample {{entityName}} instances
                /// </summary>
                public static List<{{className}}> CreateSamples(int count = {{count}})
                {
                    List<{{className}}> items = [];
                    for (int i = 0; i < count; i++)
                    {
                        items.Add(CreateSample());
                    }
                    return items;
                }

                /// <summary>
                /// Gets statistics about this generator stage
                /// </summary>
                public static string GetGeneratorInfo()
                {
                    return "Stage 2: File Integration Data Source Generator - Multiple data sources (attributes + JSON files), no caching";
                }

                /// <summary>
                /// Gets information about external configuration
                /// </summary>
                public static string GetConfigurationInfo()
                {
                    return "{{configurationInfo}}";
                }
            }
            """;
    }

    private static string GeneratePropertyAssignment(IPropertySymbol property, DataSourceConfiguration? fileConfig)
    {
        var propertyName = property.Name;
        var typeName = property.Type.ToDisplayString();
        
        // Check if we have external templates for this property
        var template = fileConfig?.Templates?.ContainsKey(propertyName) == true 
            ? fileConfig.Templates[propertyName] 
            : null;
        if (template != null && template.Length > 0)
        {
            return $"{propertyName} = {GenerateFromTemplate(property.Type, template)}";
        }
        
        return $"{propertyName} = {GenerateTestValue(property.Type, propertyName)}";
    }

    private static string GenerateFromTemplate(ITypeSymbol type, string[] templates)
    {
        var typeName = type.ToDisplayString();
        
        if (typeName == "string")
        {
            var templatesList = string.Join("\", \"", templates);
            return $"new[] {{ \"{templatesList}\" }}[_random.Next({templates.Length})]";
        }
        
        // For non-string types, fall back to regular generation
        return GenerateTestValue(type, "");
    }

    private static string GenerateTestValue(ITypeSymbol type, string propertyName)
    {
        var typeName = type.ToDisplayString();
        
        return typeName switch
        {
            "string" => $"$\"Sample{propertyName}_{{_random.Next(1, 1000)}}\"",
            "int" => "_random.Next(1, 100)",
            "long" => "_random.NextInt64(1, 1000)",
            "decimal" => "((decimal)_random.NextDouble() * 1000)",
            "double" => "_random.NextDouble() * 1000",
            "float" => "((float)_random.NextDouble() * 1000)",
            "bool" => "_random.Next(0, 2) == 1",
            "System.Guid" => "Guid.NewGuid()",
            "System.DateTime" => "DateTime.Now.AddDays(_random.Next(-365, 365))",
            "System.DateTime?" => "_random.Next(0, 2) == 1 ? DateTime.Now.AddDays(_random.Next(-365, 365)) : null",
            _ when type.TypeKind == TypeKind.Enum => GenerateEnumValue(type),
            _ when typeName.EndsWith("?") => "null",
            _ => "default"
        };
    }

    private static string GenerateEnumValue(ITypeSymbol enumType)
    {
        var enumMembers = enumType.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsStatic && f.HasConstantValue)
            .ToArray();

        if (enumMembers.Length == 0)
            return "default";

        return $"({enumType.ToDisplayString()})_random.Next(0, {enumMembers.Length})";
    }

    private static T? GetAttributeValue<T>(AttributeData attributeData, string propertyName)
    {
        var namedArgument = attributeData.NamedArguments
            .FirstOrDefault(na => na.Key == propertyName);

        if (namedArgument.Key == propertyName && namedArgument.Value.Value is T value)
            return value;

        return default;
    }

    private const string DataSourceAttributeSource = """
        // <auto-generated/>
        #nullable enable

        using System;

        namespace Stage2.FileIntegration.Attributes;

        /// <summary>
        /// Marks a class for data source generation in Stage 2
        /// Stage 2: Attribute + external file data sources
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        public sealed class DataSourceAttribute : Attribute
        {
            /// <summary>
            /// The name of the entity being generated
            /// </summary>
            public string? EntityName { get; set; }

            /// <summary>
            /// Default number of items to generate in collections
            /// </summary>
            public int Count { get; set; } = 10;

            /// <summary>
            /// Optional reference to external configuration file
            /// </summary>
            public string? ConfigurationFile { get; set; }
        }
        """;
}
