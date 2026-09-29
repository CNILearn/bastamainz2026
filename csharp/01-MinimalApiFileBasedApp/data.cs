namespace BooksData;

using System.Text.Json.Serialization;

// Primary constructor - no boilerplate constructor body needed
record Book(int Id, string Title, string Author, decimal Price);

// Source-generated JSON context eliminates reflection-based serialization warnings
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(List<Book>))]
partial class BookJsonContext : JsonSerializerContext { }