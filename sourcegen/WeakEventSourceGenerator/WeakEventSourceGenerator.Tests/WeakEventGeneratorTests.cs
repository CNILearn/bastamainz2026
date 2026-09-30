using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Reflection;

using Xunit;
using Xunit.Abstractions;

namespace WeakEventSourceGenerator.Tests;

/// <summary>
/// Tests for the WeakEventSourceGenerator to ensure it generates correct weak event implementations.
/// </summary>
public class WeakEventGeneratorTests(ITestOutputHelper output)
{
    [Fact]
    public void WeakEventGenerator_ShouldGenerateAttribute()
    {
        // Arrange
        var sourceCode = """
            namespace TestNamespace;
            
            public partial class TestClass
            {
                // No actual usage needed, just test attribute generation
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var attributeSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("WeakEventAttribute"));
        Assert.NotNull(attributeSource);
        Assert.Contains("public sealed class WeakEventAttribute", attributeSource.ToString());
    }

    [Fact]
    public void WeakEventGenerator_ShouldGenerateWeakEventManager()
    {
        // Arrange
        var sourceCode = """
            namespace TestNamespace;
            
            public partial class TestClass
            {
                // No actual usage needed, just test manager generation
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        var managerSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("WeakEventManager"));
        Assert.NotNull(managerSource);
        Assert.Contains("public sealed class WeakEventManager<TDelegate>", managerSource.ToString());
        Assert.Contains("public void Subscribe(TDelegate handler)", managerSource.ToString());
        Assert.Contains("public void Unsubscribe(TDelegate handler)", managerSource.ToString());
        Assert.Contains("public void Invoke(params object?[] args)", managerSource.ToString());
    }

    [Fact]
    public void WeakEventGenerator_WithSimpleActionEvent_ShouldGenerateImplementation()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public partial class EventPublisher
            {
                [WeakEvent]
                public partial event Action<string> MessageReceived;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("partial class EventPublisher"));
        Assert.NotNull(generatedSource);
        
        var sourceText = generatedSource.ToString();
        
        // Verify partial event implementation with add/remove accessors
        Assert.Contains("public partial event System.Action<string> MessageReceived", sourceText);
        Assert.Contains("add", sourceText);
        Assert.Contains("remove", sourceText);
        Assert.Contains("private WeakEventManager<System.Action<string>>? _messageReceivedManager;", sourceText);
        Assert.Contains("public int GetMessageReceivedSubscriberCount()", sourceText);
        Assert.Contains("public void CleanupMessageReceived()", sourceText);
    }

    [Fact]
    public void WeakEventGenerator_WithEventHandlerEvent_ShouldGenerateImplementation()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public partial class EventPublisher
            {
                [WeakEvent]
                public partial event EventHandler<string> DataChanged;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("partial class EventPublisher"));
        Assert.NotNull(generatedSource);
        
        var sourceText = generatedSource.ToString();
        
        // Verify EventHandler implementation with add/remove accessors
        Assert.Contains("partial event System.EventHandler<string> DataChanged", sourceText);
        Assert.Contains("add", sourceText);
        Assert.Contains("remove", sourceText);
    }

    [Fact]
    public void WeakEventGenerator_WithCustomAttributes_ShouldRespectConfiguration()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public partial class EventPublisher
            {
                [WeakEvent(AutoCleanup = false, CleanupThreshold = 20)]
                public partial event Action StatusChanged;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Debug output
        output.WriteLine($"Generated {result.GeneratedTrees.Length} trees for custom attributes test:");
        foreach (var tree in result.GeneratedTrees)
        {
            output.WriteLine($"Tree: {tree.FilePath}");
            output.WriteLine(tree.ToString());
            output.WriteLine("---");
        }

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("partial class EventPublisher"));
        Assert.NotNull(generatedSource);
        
        var sourceText = generatedSource.ToString();
        
        // Verify configuration is applied
        Assert.Contains("new WeakEventManager<Action>(false, 20)", sourceText);
    }

    [Fact]
    public void WeakEventGenerator_WithNonPartialEvent_ShouldNotGenerate()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public class EventPublisher
            {
                [WeakEvent]
                public event Action? RegularEvent; // Not partial - should be ignored
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        // Should only have attribute and manager generation, not event implementation
        var implementationSource = result.GeneratedTrees.FirstOrDefault(t => 
            t.ToString().Contains("partial class EventPublisher"));
        Assert.Null(implementationSource);
    }

    [Fact]
    public void WeakEventGenerator_WithMultipleEvents_ShouldGenerateAll()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public partial class EventPublisher
            {
                [WeakEvent]
                public partial event Action<string> MessageReceived;

                [WeakEvent]
                public partial event EventHandler DataChanged;

                [WeakEvent]
                public partial event Action StatusChanged;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("partial class EventPublisher"));
        Assert.NotNull(generatedSource);
        
        var sourceText = generatedSource.ToString();
        
        // Verify all events are generated
        Assert.Contains("MessageReceived", sourceText);
        Assert.Contains("DataChanged", sourceText);
        Assert.Contains("StatusChanged", sourceText);
        
        // Verify each has its manager
        Assert.Contains("_messageReceivedManager", sourceText);
        Assert.Contains("_dataChangedManager", sourceText);
        Assert.Contains("_statusChangedManager", sourceText);
    }

    [Fact]
    public void WeakEventGenerator_WithDifferentAccessModifiers_ShouldPreserveAccessModifiers()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public partial class EventPublisher
            {
                [WeakEvent]
                public partial event Action PublicEvent;

                [WeakEvent]
                internal partial event Action InternalEvent;

                [WeakEvent]
                protected partial event Action ProtectedEvent;

                [WeakEvent]
                protected internal partial event Action ProtectedInternalEvent;

                [WeakEvent]
                private partial event Action PrivateEvent;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("partial class EventPublisher"));
        Assert.NotNull(generatedSource);
        
        var sourceText = generatedSource.ToString();
        
        // Verify that each event's access modifier is preserved
        Assert.Contains("public partial event System.Action PublicEvent", sourceText);
        Assert.Contains("public int GetPublicEventSubscriberCount()", sourceText);
        Assert.Contains("public void CleanupPublicEvent()", sourceText);
        
        Assert.Contains("internal partial event System.Action InternalEvent", sourceText);
        Assert.Contains("internal int GetInternalEventSubscriberCount()", sourceText);
        Assert.Contains("internal void CleanupInternalEvent()", sourceText);
        
        Assert.Contains("protected partial event System.Action ProtectedEvent", sourceText);
        Assert.Contains("protected int GetProtectedEventSubscriberCount()", sourceText);
        Assert.Contains("protected void CleanupProtectedEvent()", sourceText);
        
        Assert.Contains("protected internal partial event System.Action ProtectedInternalEvent", sourceText);
        Assert.Contains("protected internal int GetProtectedInternalEventSubscriberCount()", sourceText);
        Assert.Contains("protected internal void CleanupProtectedInternalEvent()", sourceText);
        
        Assert.Contains("private partial event System.Action PrivateEvent", sourceText);
        Assert.Contains("private int GetPrivateEventSubscriberCount()", sourceText);
        Assert.Contains("private void CleanupPrivateEvent()", sourceText);
    }

    [Fact]
    public void WeakEventGenerator_WithNoExplicitAccessModifier_ShouldUseInternal()
    {
        // Arrange
        var sourceCode = """
            using WeakEventSourceGenerator.Attributes;
            using System;
            
            namespace TestNamespace;
            
            public partial class EventPublisher
            {
                [WeakEvent]
                partial event Action ImplicitAccessEvent;
            }
            """;

        // Act
        var result = RunGenerator(sourceCode);

        // Assert
        Assert.True(result.Diagnostics.IsEmpty, $"Generator produced diagnostics: {string.Join(", ", result.Diagnostics)}");
        
        var generatedSource = result.GeneratedTrees.FirstOrDefault(t => t.ToString().Contains("partial class EventPublisher"));
        Assert.NotNull(generatedSource);
        
        var sourceText = generatedSource.ToString();
        
        // Verify that implicit access modifier defaults to internal
        Assert.Contains("internal partial event System.Action ImplicitAccessEvent", sourceText);
        Assert.Contains("internal int GetImplicitAccessEventSubscriberCount()", sourceText);
        Assert.Contains("internal void CleanupImplicitAccessEvent()", sourceText);
    }

    private static GeneratorDriverRunResult RunGenerator(string sourceCode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location)
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new WeakEventGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        return driver.RunGenerators(compilation).GetRunResult();
    }
}