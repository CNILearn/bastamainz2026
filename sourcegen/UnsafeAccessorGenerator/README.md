# UnsafeAccessor Generator

A source generator that demonstrates how to use the `UnsafeAccessor` attribute to access private members of classes and instantiate objects from JSON data at compile time.

## Overview

This generator creates implementations for partial `JsonContext` classes that can deserialize JSON data into objects with private constructors, fields, or property setters using .NET's `UnsafeAccessor` feature.

## Features

- **UnsafeAccessor Integration**: Demonstrates how to generate `UnsafeAccessor` methods for accessing private members
- **JSON Deserialization**: Reads JSON files and creates strongly-typed objects
- **Private Member Access**: Works with classes that have private fields, private setters, or private constructors
- **Compile-time Generation**: All accessor methods are generated at build time with zero runtime overhead
- **Type Safety**: Generated code maintains full type safety while bypassing normal access restrictions

## How It Works

### 1. Declare a Partial Class

Create a partial `JsonContext` class with a partial method declaration:

```csharp
public partial class JsonContext
{
    public partial IEnumerable<Book> GetBooks(string jsonFile);
}
```

### 2. Define Target Classes

The generator works with various class designs:

**Private Fields:**
```csharp
public class Book
{
    private string _title = string.Empty;
    private string _publisher = string.Empty;
    
    public override string ToString() => $"{_title} {_publisher}";
}
```

**Private Setters:**
```csharp
public class BookWithPrivateSetters
{
    public string Title { get; private set; } = string.Empty;
    public string Publisher { get; private set; } = string.Empty;
    
    public override string ToString() => $"{Title} {Publisher}";
}
```

**Records:**
```csharp
public record class BookRecord(string Title, string Publisher);
```

### 3. Provide JSON Data

Add a JSON file as an additional file in your project:

```json
[
  {
    "title": "Pragmatic Microservices",
    "publisher": "Packt"
  },
  {
    "title": "Expert C#",
    "publisher": "Packt"
  }
]
```

### 4. Generated Implementation

The source generator automatically creates:

```csharp
public partial class JsonContext
{
    // UnsafeAccessor methods for accessing private members
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    private static extern Book CreateBookPrivateFields();

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_title")]
    private static extern ref string GetTitleField(Book book);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_publisher")]
    private static extern ref string GetPublisherField(Book book);

    // Implementation of the GetBooks method
    public partial IEnumerable<Book> GetBooks(string jsonFile)
    {
        var books = new List<Book>();
        
        try
        {
            var jsonContent = File.ReadAllText(jsonFile);
            var bookData = JsonSerializer.Deserialize<JsonElement[]>(jsonContent);

            if (bookData != null)
            {
                foreach (var item in bookData)
                {
                    var title = item.GetProperty("title").GetString() ?? string.Empty;
                    var publisher = item.GetProperty("publisher").GetString() ?? string.Empty;

                    // Create instance and set private fields using UnsafeAccessor
                    var book = CreateBookPrivateFields();
                    GetTitleField(book) = title;
                    GetPublisherField(book) = publisher;

                    books.Add(book);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
        }

        return books;
    }
}
```

## Usage Example

```csharp
var context = new JsonContext();
var books = context.GetBooks("books.json");

foreach (var book in books)
{
    Console.WriteLine($"- {book}");
}
```

Output:
```
- Pragmatic Microservices Packt
- Expert C# Packt
```

## Requirements

- **.NET 9.0** or later (required for `UnsafeAccessor` support)
- **C# 12.0** or later
- **MSBuild-based projects** (.NET SDK style)

## Key Benefits

1. **Zero Runtime Overhead**: All accessor methods are generated at compile time
2. **Type Safety**: Maintains compile-time type checking while bypassing access restrictions
3. **Performance**: Direct field access without reflection overhead
4. **Flexibility**: Works with various class designs and access patterns
5. **Modern C#**: Demonstrates latest .NET features for high-performance scenarios

## Advanced Scenarios

This generator demonstrates several advanced source generator concepts:

- **UnsafeAccessor Integration**: Shows how to generate `UnsafeAccessor` methods programmatically
- **JSON Processing**: Combines compile-time generation with runtime JSON deserialization
- **Private Member Access**: Bypasses normal access restrictions for testing and serialization scenarios
- **Partial Method Implementation**: Generates implementations for partial method declarations

## Use Cases

- **Testing**: Access private members for unit testing without reflection
- **Serialization**: Create high-performance serializers that can access private state
- **Data Transfer**: Populate objects with private setters from external data sources
- **Legacy Code**: Work with existing classes that weren't designed for external instantiation

## Technical Details

The generator uses the `UnsafeAccessor` attribute introduced in .NET 8 which provides a compile-time safe way to access private members without runtime reflection overhead. The generated methods are essentially compiler-generated friends that can access otherwise inaccessible members.

This approach is particularly useful for scenarios where you need high-performance access to private members, such as serialization libraries, testing frameworks, or when working with legacy code that wasn't designed for external access.