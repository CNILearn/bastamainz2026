List<int> list = [with(capacity: 20), 3, 5, 6, 11];
Console.WriteLine($"capacity: {list.Capacity}");
foreach (int i in list)
{
    Console.WriteLine(i);
}

string[] first = ["Alice", "Bob", "Charlie"];
string[] second = ["Dave", "Eve", "Frank", "Grace"];

// C# 12: clean syntax but no capacity hint
List<string> noCapacity = [..first, ..second];

// C# 15 equivalent – set capacity before adding items
List<string> withCapacity = [with(capacity: first.Length + second.Length), ..first, ..second];
Console.WriteLine($"withCapacity: {withCapacity.Capacity}");

// Another constructor-argument example with a different collection type.
// HashSet<T> gets a case-insensitive comparer through `with(...)`.
HashSet<string> names = [with(comparer: StringComparer.OrdinalIgnoreCase), "Alice", "alice", "Bob"];
Console.WriteLine($"hashset count (case-insensitive): {names.Count}");