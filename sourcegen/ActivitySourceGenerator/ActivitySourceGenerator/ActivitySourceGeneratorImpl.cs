using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Text;

namespace ActivitySourceGenerator;

[Generator]
public class ActivitySourceGeneratorImpl : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the InterceptsLocationAttribute for the new .NET 9 format
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "InterceptsLocationAttribute.g.cs", SourceText.From(InterceptsLocationAttributeSource, Encoding.UTF8)));

        // Register the ActivityAttribute
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "ActivityAttribute.g.cs", SourceText.From(ActivityAttributeSource, Encoding.UTF8)));

        // Find methods and classes with ActivityAttribute
        var methodsProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m != null)
            .Collect();

        // Generate interceptors
        context.RegisterSourceOutput(methodsProvider, static (spc, methods) => Execute(spc, methods));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // Look for method invocations that could be intercepted
        return node is InvocationExpressionSyntax;
    }

    private static MethodInfo GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var semanticModel = context.SemanticModel;
        
        if (context.Node is InvocationExpressionSyntax invocationSyntax)
        {
            // Get the method being invoked
            var symbolInfo = semanticModel.GetSymbolInfo(invocationSyntax);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                // Check if the target method has the ActivityAttribute
                if (HasActivityAttribute(methodSymbol))
                {
#pragma warning disable RSEXPERIMENTAL002 // Experimental interceptable location API
                    // Get the InterceptableLocation using the semantic model
                    var interceptableLocation = semanticModel.GetInterceptableLocation(invocationSyntax);
                    if (interceptableLocation != null)
                    {
                        return new MethodInfo(methodSymbol, invocationSyntax, GetAttributeData(methodSymbol), interceptableLocation);
                    }
#pragma warning restore RSEXPERIMENTAL002
                }
            }
        }

        return null;
    }

    private static bool HasActivityAttribute(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attr => 
            attr.AttributeClass?.Name == "ActivityAttribute" &&
            attr.AttributeClass.ContainingNamespace?.ToDisplayString() == "ActivitySourceGenerator.Attributes");
    }

    private static AttributeData GetAttributeData(ISymbol symbol)
    {
        return symbol.GetAttributes().FirstOrDefault(attr => 
            attr.AttributeClass?.Name == "ActivityAttribute" &&
            attr.AttributeClass.ContainingNamespace?.ToDisplayString() == "ActivitySourceGenerator.Attributes");
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<MethodInfo> methods)
    {
        if (methods.IsDefaultOrEmpty)
            return;

        List<InterceptorInfo> interceptors = [];

        foreach (var method in methods.Where(m => m != null))
        {
            var interceptor = GenerateInterceptorForMethod(method);
            if (interceptor is not null)
                interceptors.Add(interceptor);
        }

        if (interceptors.Any())
        {
            var source = GenerateInterceptorSource(interceptors);
            context.AddSource("ActivityInterceptors.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static InterceptorInfo GenerateInterceptorForMethod(MethodInfo methodInfo)
    {
        var method = methodInfo.Symbol;
        var syntax = methodInfo.Syntax;
        var interceptableLocation = methodInfo.InterceptableLocation;
        
        // Stable deterministic identifier based on fully qualified containing type, method name, and source line/column.
        // This avoids directory-dependent hashing while still providing uniqueness per invocation site.
        var location = syntax.GetLocation().GetLineSpan();
        // Lines/columns are zero-based from Roslyn; make them 1-based for readability.
        var line = location.StartLinePosition.Line + 1;
        var column = location.StartLinePosition.Character + 1;
        // Sanitize the containing type display string to be a valid identifier fragment.
        var containingTypeSafe = method.ContainingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            .Replace('<', '_').Replace('>', '_').Replace('.', '_').Replace('+', '_');
        var interceptorName = $"Intercept_{containingTypeSafe}_{method.Name}_{line}_{column}";

        return new InterceptorInfo(
            interceptorName,
            method,
            syntax,
            interceptableLocation,
            methodInfo.AttributeData);
    }

    private static string GenerateInterceptorSource(List<InterceptorInfo> interceptors)
    {
        var namespaces = interceptors.Select(i => i.Method.ContainingNamespace.ToDisplayString()).Distinct();
        var namespacesContent = string.Join("\n", namespaces.Select(ns => GenerateNamespaceInterceptors(ns, interceptors)));

        return $$"""
            // <auto-generated/>
            #nullable enable
            using System;
            using System.Diagnostics;
            using System.Runtime.CompilerServices;

            {{namespacesContent}}
            """;
    }

    private static string GenerateNamespaceInterceptors(string namespaceName, List<InterceptorInfo> interceptors)
    {
        var interceptorsInNamespace = interceptors.Where(i => i.Method.ContainingNamespace.ToDisplayString() == namespaceName);
        var interceptorClasses = string.Join("\n", interceptorsInNamespace.Select(GenerateInterceptorMethod));

        return $$"""
            namespace {{namespaceName}};

            {{interceptorClasses}}
            """;
    }

    private static string GenerateInterceptorMethod(InterceptorInfo interceptor)
    {
        var method = interceptor.Method;
        var isAsync = method.IsAsync;
        var returnType = method.ReturnType.ToDisplayString();
        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
        var arguments = string.Join(", ", method.Parameters.Select(p => p.Name));

        var activitySourceName = GetActivitySourceName(interceptor.AttributeData, method);
        var activityName = GetActivityName(interceptor.AttributeData, method);
        
        var methodBody = isAsync 
            ? GenerateAsyncInterceptorBody(method, activityName, arguments)
            : GenerateSyncInterceptorBody(method, activityName, arguments);

        var asyncModifier = isAsync ? "async " : "";

#pragma warning disable RSEXPERIMENTAL002 // Experimental interceptable location API
        var interceptsLocationAttribute = interceptor.InterceptableLocation.GetInterceptsLocationAttributeSyntax();
#pragma warning restore RSEXPERIMENTAL002

        // The GetInterceptsLocationAttributeSyntax() already includes the brackets, so don't add extra ones
        return $$"""
            file static class {{interceptor.InterceptorName}}
            {
                private static readonly ActivitySource _activitySource = new("{{activitySourceName}}");

                {{interceptsLocationAttribute}}
                public static {{asyncModifier}}{{returnType}} {{method.Name}}({{parameters}})
                {
            {{methodBody}}
                }
            }

            """;
    }

    private static string GenerateSyncInterceptorBody(IMethodSymbol method, string activityName, string arguments)
    {
        var hasReturnValue = !method.ReturnsVoid;
        var containingTypeDisplay = method.ContainingType.ToDisplayString();
        
        if (hasReturnValue)
        {
            return $$"""
                    using var activity = _activitySource.StartActivity("{{activityName}}");
                    try
                    {
                        var result = {{containingTypeDisplay}}.{{method.Name}}({{arguments}});
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        activity?.SetTag("exception.type", ex.GetType().FullName);
                        activity?.SetTag("exception.message", ex.Message);
                        throw;
                    }
            """;
        }
        else
        {
            return $$"""
                    using var activity = _activitySource.StartActivity("{{activityName}}");
                    try
                    {
                        {{containingTypeDisplay}}.{{method.Name}}({{arguments}});
                        activity?.SetStatus(ActivityStatusCode.Ok);
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        activity?.SetTag("exception.type", ex.GetType().FullName);
                        activity?.SetTag("exception.message", ex.Message);
                        throw;
                    }
            """;
        }
    }

    private static string GenerateAsyncInterceptorBody(IMethodSymbol method, string activityName, string arguments)
    {
        var containingTypeDisplay = method.ContainingType.ToDisplayString();

        return $$"""
                using var activity = _activitySource.StartActivity("{{activityName}}");
                try
                {
                    var result = await {{containingTypeDisplay}}.{{method.Name}}({{arguments}});
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.SetTag("exception.type", ex.GetType().FullName);
                    activity?.SetTag("exception.message", ex.Message);
                    throw;
                }
        """;
    }

    private static string GetActivitySourceName(AttributeData attributeData, IMethodSymbol method)
    {
        if (attributeData?.NamedArguments.FirstOrDefault(kvp => kvp.Key == "ActivitySourceName").Value.Value is string sourceName)
        {
            return sourceName;
        }
        return method.ContainingType.ToDisplayString();
    }

    private static string GetActivityName(AttributeData attributeData, IMethodSymbol method)
    {
        if (attributeData?.NamedArguments.FirstOrDefault(kvp => kvp.Key == "ActivityName").Value.Value is string activityName)
        {
            return activityName;
        }
        return method.Name;
    }

    private class MethodInfo
    {
        public IMethodSymbol Symbol { get; }
        public SyntaxNode Syntax { get; }
        public AttributeData AttributeData { get; }
#pragma warning disable RSEXPERIMENTAL002 // Experimental interceptable location API
        public InterceptableLocation InterceptableLocation { get; }
#pragma warning restore RSEXPERIMENTAL002

        public MethodInfo(IMethodSymbol symbol, SyntaxNode syntax, AttributeData attributeData, 
#pragma warning disable RSEXPERIMENTAL002 // Experimental interceptable location API
            InterceptableLocation interceptableLocation)
#pragma warning restore RSEXPERIMENTAL002
        {
            Symbol = symbol;
            Syntax = syntax;
            AttributeData = attributeData;
            InterceptableLocation = interceptableLocation;
        }
    }
    
    private class InterceptorInfo
    {
        public string InterceptorName { get; }
        public IMethodSymbol Method { get; }
        public SyntaxNode Syntax { get; }
#pragma warning disable RSEXPERIMENTAL002 // Experimental interceptable location API
        public InterceptableLocation InterceptableLocation { get; }
#pragma warning restore RSEXPERIMENTAL002
        public AttributeData AttributeData { get; }

        public InterceptorInfo(string interceptorName, IMethodSymbol method, SyntaxNode syntax,
#pragma warning disable RSEXPERIMENTAL002 // Experimental interceptable location API
            InterceptableLocation interceptableLocation, AttributeData attributeData)
#pragma warning restore RSEXPERIMENTAL002
        {
            InterceptorName = interceptorName;
            Method = method;
            Syntax = syntax;
            InterceptableLocation = interceptableLocation;
            AttributeData = attributeData;
        }
    }

    private const string InterceptsLocationAttributeSource = """
// <auto-generated/>
        
#nullable enable
using System;

namespace System.Runtime.CompilerServices;

// .NET 9 InterceptsLocationAttribute with support for both old and new constructor formats.
// The new format uses (int version, string data) instead of (string filePath, int line, int character).
// This approach uses the new InterceptableLocation-based generation which eliminates the CS9270 warning.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class InterceptsLocationAttribute : Attribute
{
    // Legacy constructor for backward compatibility - this will show CS9270 warning with .NET 9+, preview feature with .NET 8
    public InterceptsLocationAttribute(string filePath, int line, int character)
    {
        FilePath = filePath;
        Line = line;
        Character = character;
    }

    // New constructor format used by InterceptableLocation.GetInterceptsLocationAttributeSyntax() with .NET 9+
    public InterceptsLocationAttribute(int version, string data)
    {
        Version = version;
        Data = data;
    }

    public string? FilePath { get; }  // for .NET 8
    public int Line { get; } // for .NET 8
    public int Character { get; } // for .NET 8
    public int Version { get; }
    public string? Data { get; }
}
""";

    private const string ActivityAttributeSource = """
// <auto-generated/>
        
#nullable enable
using System;

namespace ActivitySourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class ActivityAttribute : Attribute
{
    public string? ActivitySourceName { get; set; }
    public string? ActivityName { get; set; }
    public bool RecordExceptions { get; set; } = true;
    public string? Tags { get; set; }
}
""";
}
