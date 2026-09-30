

namespace ActivitySourceGenerator.SnapshotTests;

public class ActivitySourceGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesInterceptorForSimpleMethod()
    {
        var source = """
            using ActivitySourceGenerator.Attributes;

            namespace TestNamespace;

            public class TestClass
            {
                [Activity]
                public static string TestMethod(string input)
                {
                    return $"Hello {input}";
                }
            }

            public class Usage
            {
                public static void Main()
                {
                    var result = TestClass.TestMethod("World");
                }
            }
            """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesInterceptorForAsyncMethod()
    {
        var source = """
            using ActivitySourceGenerator.Attributes;
            using System.Threading.Tasks;

            namespace TestNamespace;

            public class TestClass
            {
                [Activity(ActivityName = "AsyncOperation")]
                public static async Task<int> TestMethodAsync(string input)
                {
                    await Task.Delay(100);
                    return input.Length;
                }
            }

            public class Usage
            {
                public static async Task Main()
                {
                    var result = await TestClass.TestMethodAsync("World");
                }
            }
            """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesInterceptorWithCustomActivitySource()
    {
        var source = """
            using ActivitySourceGenerator.Attributes;

            namespace TestNamespace;

            public class TestClass
            {
                [Activity(ActivitySourceName = "CustomSource", ActivityName = "CustomActivity")]
                public static void TestMethod()
                {
                    // Method body
                }
            }

            public class Usage
            {
                public static void Main()
                {
                    TestClass.TestMethod();
                }
            }
            """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesInterceptorForMethodWithMultipleParameters()
    {
        var source = """
            using ActivitySourceGenerator.Attributes;

            namespace TestNamespace;

            public class TestClass
            {
                [Activity]
                public static double Calculate(int x, double y, string operation)
                {
                    return operation switch
                    {
                        "add" => x + y,
                        "multiply" => x * y,
                        _ => 0
                    };
                }
            }

            public class Usage
            {
                public static void Main()
                {
                    var result = TestClass.Calculate(5, 3.14, "multiply");
                }
            }
            """;

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GeneratesInterceptorForVoidMethod()
    {
        var source = """
            using ActivitySourceGenerator.Attributes;

            namespace TestNamespace;

            public class TestClass
            {
                [Activity]
                public static void DoSomething(string message)
                {
                    Console.WriteLine(message);
                }
            }

            public class Usage
            {
                public static void Main()
                {
                    TestClass.DoSomething("Hello World");
                }
            }
            """;

        return TestHelper.Verify(source);
    }
}
