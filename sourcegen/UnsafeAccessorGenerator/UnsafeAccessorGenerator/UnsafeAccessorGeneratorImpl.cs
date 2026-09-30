using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Text;

namespace UnsafeAccessorGenerator;

[Generator]
public class UnsafeAccessorGeneratorImpl : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the JsonUnsafeAccessorAttribute only once
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "JsonUnsafeAccessorAttribute.g.cs", SourceText.From(JsonUnsafeAccessorAttributeSource, Encoding.UTF8)));

        // Find partial classes that need implementation
        var partialClassesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m != null)
            .Collect();

        // Combine with additional files (JSON files)
        var combinedProvider = partialClassesProvider.Combine(context.AdditionalTextsProvider.Collect());

        // Generate implementations
        context.RegisterSourceOutput(combinedProvider, ExecuteGeneration);
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // Look for partial classes with partial methods
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            // Check if it's partial
            bool isPartial = classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
            
            if (!isPartial) return false;
            
            // Check if it has any partial methods that return IEnumerable<T>
            return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
                .Any(method => method.Modifiers.Any(SyntaxKind.PartialKeyword) && 
                              IsIEnumerableReturnType(method.ReturnType));
        }
        
        return false;
    }

    private static bool IsIEnumerableReturnType(TypeSyntax returnType)
    {
        return returnType switch
        {
            GenericNameSyntax genericName => 
                genericName.Identifier.ValueText == "IEnumerable" && 
                genericName.TypeArgumentList.Arguments.Count == 1,
            QualifiedNameSyntax qualifiedName => 
                qualifiedName.Right is GenericNameSyntax rightGeneric &&
                rightGeneric.Identifier.ValueText == "IEnumerable" &&
                rightGeneric.TypeArgumentList.Arguments.Count == 1,
            _ => false
        };
    }

    private static ClassInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol is null)
            return null;

        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace 
            ? string.Empty 
            : classSymbol.ContainingNamespace.ToDisplayString();

        // Find all partial methods that return IEnumerable<T>
        var partialMethods = new List<MethodInfo>();

        foreach (var method in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            if (!method.Modifiers.Any(SyntaxKind.PartialKeyword) || !IsIEnumerableReturnType(method.ReturnType))
                continue;

            var returnType = method.ReturnType.ToString();
            var parameters = method.ParameterList.Parameters
                .Select(p => p.Identifier.ValueText)
                .ToArray();

            // Extract the type from IEnumerable<T>
            var targetTypeName = ExtractTypeFromReturnType(returnType);
            if (string.IsNullOrEmpty(targetTypeName))
                continue;

            // Try to find the target type in the semantic model
            var targetType = FindTypeSymbol(context.SemanticModel, targetTypeName);
            var typeMembers = targetType != null ? AnalyzeTypeMembers(targetType) : Array.Empty<TypeMemberInfo>();

            partialMethods.Add(new MethodInfo(method.Identifier.ValueText, returnType, parameters, typeMembers));
        }

        if (partialMethods.Count == 0)
            return null;

        return new ClassInfo(
            classSymbol.Name,
            namespaceName,
            partialMethods.ToArray()
        );
    }

    private static INamedTypeSymbol? FindTypeSymbol(SemanticModel semanticModel, string typeName)
    {
        var compilation = semanticModel.Compilation;
        
        // First try to find by metadata name
        var typeByMetadata = compilation.GetTypeByMetadataName(typeName);
        if (typeByMetadata != null)
            return typeByMetadata;
            
        // Search through all source symbols in the compilation
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var root = syntaxTree.GetRoot();
            var model = compilation.GetSemanticModel(syntaxTree);
            
            foreach (var classDeclaration in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            {
                if (classDeclaration.Identifier.ValueText == typeName)
                {
                    var symbol = model.GetDeclaredSymbol(classDeclaration);
                    if (symbol is INamedTypeSymbol namedTypeSymbol)
                        return namedTypeSymbol;
                }
            }
            
            // Also check for record declarations
            foreach (var recordDeclaration in root.DescendantNodesAndSelf().OfType<RecordDeclarationSyntax>())
            {
                if (recordDeclaration.Identifier.ValueText == typeName)
                {
                    var symbol = model.GetDeclaredSymbol(recordDeclaration);
                    if (symbol is INamedTypeSymbol namedTypeSymbol)
                        return namedTypeSymbol;
                }
            }
        }
        
        return null;
    }

    private static TypeMemberInfo[] AnalyzeTypeMembers(INamedTypeSymbol typeSymbol)
    {
        var members = new List<TypeMemberInfo>();

        // Analyze fields (including private ones, but exclude backing fields of auto-properties unless it's a record)
        foreach (var field in typeSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.IsStatic || field.IsConst)
                continue;

            // For records, we want to include backing fields of properties for init accessors
            bool isRecordBackingField = field.IsImplicitlyDeclared && 
                                       field.Name.Contains("k__BackingField") && 
                                       typeSymbol.IsRecord;

            // For regular classes, skip backing fields. For records, include them if they're for init properties
            if (field.IsImplicitlyDeclared && field.Name.Contains("k__BackingField") && !isRecordBackingField)
                continue;

            members.Add(new TypeMemberInfo(
                field.Name,
                field.Type.ToDisplayString(),
                isRecordBackingField ? TypeMemberKind.RecordBackingField : TypeMemberKind.Field,
                field.DeclaredAccessibility == Accessibility.Private || isRecordBackingField
            ));
        }

        // Analyze properties (especially those with private setters or init accessors)
        foreach (var property in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.IsStatic || property.IsIndexer)
                continue;

            // Check for private setters
            var hasPrivateSetter = property.SetMethod?.DeclaredAccessibility == Accessibility.Private;
            if (hasPrivateSetter)
            {
                members.Add(new TypeMemberInfo(
                    property.Name,
                    property.Type.ToDisplayString(),
                    TypeMemberKind.PropertySetter,
                    true
                ));
            }
            
            // Check for init accessors (for records and init-only properties)
            var hasInitSetter = property.SetMethod?.IsInitOnly == true;
            if (hasInitSetter && !typeSymbol.IsRecord) // For non-records with explicit init
            {
                members.Add(new TypeMemberInfo(
                    property.Name,
                    property.Type.ToDisplayString(),
                    TypeMemberKind.InitSetter,
                    false
                ));
            }
        }

        return members.ToArray();
    }

    private static void ExecuteGeneration(SourceProductionContext context, 
        (ImmutableArray<ClassInfo?> Classes, ImmutableArray<AdditionalText> Files) input)
    {
        if (input.Classes.IsDefaultOrEmpty)
            return;

        foreach (var classInfo in input.Classes)
        {
            if (classInfo != null)
                GenerateJsonContextImplementation(context, classInfo, input.Files);
        }
    }

    private static void GenerateJsonContextImplementation(SourceProductionContext context, ClassInfo classInfo, 
        ImmutableArray<AdditionalText> additionalFiles)
    {
        if (classInfo.PartialMethods.Length == 0)
            return;

        var namespaceDeclaration = !string.IsNullOrEmpty(classInfo.Namespace) 
            ? $"namespace {classInfo.Namespace};" 
            : "";

        var methods = string.Join("\n\n    ", classInfo.PartialMethods.Select(GenerateMethodImplementation));

        // Get all unique types and their UnsafeAccessor methods
        var typeAccessorMethods = new List<string>();
        foreach (var method in classInfo.PartialMethods)
        {
            var typeName = ExtractTypeFromReturnType(method.ReturnType);
            if (!string.IsNullOrEmpty(typeName))
            {
                typeAccessorMethods.Add(GenerateUnsafeAccessorMethods(typeName, method.TypeMembers));
            }
        }

        var unsafeAccessorMethods = string.Join("\n\n    ", typeAccessorMethods.Distinct());

        var source = $$"""
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.IO;
            using System.Linq;
            using System.Runtime.CompilerServices;
            using System.Text.Json;

            {{namespaceDeclaration}}

            public partial class {{classInfo.ClassName}}
            {
                {{unsafeAccessorMethods}}

                {{methods}}
            }
            """;

        context.AddSource($"{classInfo.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string ExtractTypeFromReturnType(string returnType)
    {
        // Extract T from IEnumerable<T>
        if (returnType.StartsWith("IEnumerable<") && returnType.EndsWith(">"))
        {
            return returnType.Substring(12, returnType.Length - 13);
        }
        return "";
    }

    private static string GenerateUnsafeAccessorMethods(string typeName, TypeMemberInfo[] typeMembers)
    {
        var methods = new List<string>();

        // Check if this is a record type by looking for backing fields
        bool isRecord = typeMembers.Any(m => m.Kind == TypeMemberKind.RecordBackingField);

        if (isRecord)
        {
            // For records, we need to use the primary constructor with default values, then modify backing fields
            // We'll create it with empty strings and then set the backing fields directly
            methods.Add($"""
            // UnsafeAccessor methods for {typeName} (Record)
            private static {typeName} Create{typeName}() => new {typeName}(string.Empty, string.Empty);
            """);
        }
        else
        {
            // For regular classes, use parameterless constructor
            methods.Add($"""
            // UnsafeAccessor methods for {typeName}
            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            private static extern {typeName} Create{typeName}();
            """);
        }

        // Generate field accessors
        foreach (var member in typeMembers.Where(m => m.Kind == TypeMemberKind.Field && m.IsPrivate))
        {
            var accessorName = GetAccessorName(member.Name);
            methods.Add($"""
            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "{member.Name}")]
            private static extern ref {member.Type} Get{typeName}{accessorName}Field({typeName} instance);
            """);
        }

        // Generate record backing field accessors 
        foreach (var member in typeMembers.Where(m => m.Kind == TypeMemberKind.RecordBackingField))
        {
            var accessorName = GetRecordBackingFieldAccessorName(member.Name);
            methods.Add($"""
            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "{member.Name}")]
            private static extern ref {member.Type} Get{typeName}{accessorName}BackingField({typeName} instance);
            """);
        }

        // Generate property setter accessors (private setters)
        foreach (var member in typeMembers.Where(m => m.Kind == TypeMemberKind.PropertySetter && m.IsPrivate))
        {
            methods.Add($"""
            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_{member.Name}")]
            private static extern void Set{typeName}{member.Name}({typeName} instance, {member.Type} value);
            """);
        }

        // Generate init setter accessors (for init-only properties)
        foreach (var member in typeMembers.Where(m => m.Kind == TypeMemberKind.InitSetter))
        {
            methods.Add($"""
            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_{member.Name}")]
            private static extern void Init{typeName}{member.Name}({typeName} instance, {member.Type} value);
            """);
        }

        return string.Join("\n\n    ", methods);
    }

    private static string GetAccessorName(string fieldName)
    {
        // Convert _fieldName to FieldName for accessor method names
        if (fieldName.StartsWith("_") && fieldName.Length > 1)
        {
            return char.ToUpper(fieldName[1]) + fieldName.Substring(2);
        }
        return char.ToUpper(fieldName[0]) + fieldName.Substring(1);
    }

    private static string GetRecordBackingFieldAccessorName(string backingFieldName)
    {
        // Convert <Title>k__BackingField to Title for accessor method names
        if (backingFieldName.StartsWith("<") && backingFieldName.Contains(">k__BackingField"))
        {
            var propertyName = backingFieldName.Substring(1, backingFieldName.IndexOf('>') - 1);
            return propertyName;
        }
        return backingFieldName;
    }

    private static string GenerateMethodImplementation(MethodInfo methodInfo)
    {
        var typeName = ExtractTypeFromReturnType(methodInfo.ReturnType);
        var methodName = methodInfo.MethodName;
        var parameterName = methodInfo.Parameters.FirstOrDefault() ?? "jsonFile";

        // Generate field population code
        var populationCode = GeneratePopulationCode(typeName, methodInfo.TypeMembers);
        
        return $$"""
            public partial IEnumerable<{{typeName}}> {{methodName}}(string {{parameterName}})
            {
                var items = new List<{{typeName}}>();
        
                try
                {
                    var jsonContent = File.ReadAllText({{parameterName}});
                    var itemData = JsonSerializer.Deserialize<JsonElement[]>(jsonContent);
        
                    if (itemData != null)
                    {
                        foreach (var item in itemData)
                        {
                            var instance = Create{{typeName}}();
            {{populationCode}}
                            items.Add(instance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading JSON file: {ex.Message}");
                }
        
                return items;
            }
            """;
    }

    private static string GeneratePopulationCode(string typeName, TypeMemberInfo[] typeMembers)
    {
        var lines = new List<string>();

        foreach (var member in typeMembers)
        {
            var jsonPropertyName = GetJsonPropertyName(member.Name);
            var variableName = $"{member.Name.ToLower().Replace("<", "").Replace(">", "").Replace("k__backingfield", "")}Prop";
            
            if (member.Kind == TypeMemberKind.Field && member.IsPrivate)
            {
                var accessorName = GetAccessorName(member.Name);
                lines.Add($"                            if (item.TryGetProperty(\"{jsonPropertyName}\", out var {variableName}))");
                lines.Add($"                                Get{typeName}{accessorName}Field(instance) = {variableName}.GetString() ?? string.Empty;");
            }
            else if (member.Kind == TypeMemberKind.RecordBackingField)
            {
                var accessorName = GetRecordBackingFieldAccessorName(member.Name);
                var propertyName = GetJsonPropertyName(accessorName);
                variableName = $"{accessorName.ToLower()}Prop";
                lines.Add($"                            if (item.TryGetProperty(\"{propertyName}\", out var {variableName}))");
                lines.Add($"                                Get{typeName}{accessorName}BackingField(instance) = {variableName}.GetString() ?? string.Empty;");
            }
            else if (member.Kind == TypeMemberKind.PropertySetter && member.IsPrivate)
            {
                lines.Add($"                            if (item.TryGetProperty(\"{jsonPropertyName}\", out var {variableName}))");
                lines.Add($"                                Set{typeName}{member.Name}(instance, {variableName}.GetString() ?? string.Empty);");
            }
            else if (member.Kind == TypeMemberKind.InitSetter)
            {
                lines.Add($"                            if (item.TryGetProperty(\"{jsonPropertyName}\", out var {variableName}))");
                lines.Add($"                                Init{typeName}{member.Name}(instance, {variableName}.GetString() ?? string.Empty);");
            }
        }

        return string.Join("\n", lines);
    }

    private static string GetJsonPropertyName(string memberName)
    {
        // Convert _title to title, Title to title, etc.
        if (memberName.StartsWith("_") && memberName.Length > 1)
        {
            return char.ToLower(memberName[1]) + memberName.Substring(2);
        }
        return char.ToLower(memberName[0]) + memberName.Substring(1);
    }



    private const string JsonUnsafeAccessorAttributeSource = """
        #nullable enable
        using System;

        namespace UnsafeAccessorGenerator.Attributes
        {
            /// <summary>
            /// Marks a partial class for UnsafeAccessor-based JSON deserialization generation.
            /// </summary>
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class JsonUnsafeAccessorAttribute : Attribute
            {
                /// <summary>
                /// The target type to generate UnsafeAccessor methods for.
                /// </summary>
                public Type? TargetType { get; set; }
                
                /// <summary>
                /// The JSON file name to process.
                /// </summary>
                public string? JsonFileName { get; set; }
            }
        }
        """;

    private class ClassInfo(string className, string @namespace, UnsafeAccessorGeneratorImpl.MethodInfo[] partialMethods)
    {
        public string ClassName { get; } = className;
        public string Namespace { get; } = @namespace;
        public MethodInfo[] PartialMethods { get; } = partialMethods;
    }

    private class MethodInfo(string methodName, string returnType, string[] parameters, UnsafeAccessorGeneratorImpl.TypeMemberInfo[]? typeMembers = null)
    {
        public string MethodName { get; } = methodName;
        public string ReturnType { get; } = returnType;
        public string[] Parameters { get; } = parameters;
        public TypeMemberInfo[] TypeMembers { get; } = typeMembers ?? Array.Empty<TypeMemberInfo>();
    }

    private class TypeMemberInfo(string name, string type, UnsafeAccessorGeneratorImpl.TypeMemberKind kind, bool isPrivate)
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
        public TypeMemberKind Kind { get; } = kind;
        public bool IsPrivate { get; } = isPrivate;
    }

    private enum TypeMemberKind
    {
        Field,
        PropertySetter,
        InitSetter,
        RecordBackingField
    }
}