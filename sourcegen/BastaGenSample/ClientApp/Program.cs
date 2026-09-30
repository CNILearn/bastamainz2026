using BastaGenSample.Attributes;

Console.WriteLine("Hello, World!");

Console.WriteLine(DemoInfo.GetSummary());

[GenerateInfo]
public class Demo
{
    public required string Name { get; set; }

    public void Foo() => Console.WriteLine($"Hello, {Name}!");
}