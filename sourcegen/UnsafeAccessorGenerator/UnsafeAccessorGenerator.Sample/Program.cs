using UnsafeAccessorGenerator.Attributes;

namespace UnsafeAccessorGenerator.Sample;

// Book class with private fields
public class Book
{
    private string _title = string.Empty;
    private string _publisher = string.Empty;
    
    public override string ToString() => $"{_title} {_publisher}";
}

public class Person
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    public override string ToString() => $"{_firstName} {_lastName}";
}

// Book class with private setters
public class BookWithPrivateSetters
{
    public string Title { get; private set; } = string.Empty;
    public string Publisher { get; private set; } = string.Empty;
    
    public override string ToString() => $"{Title} {Publisher}";
}

// Record class variant
public record class BookRecord(string Title, string Publisher);

// Partial JsonContext class - the other part will be generated
public partial class JsonContext
{
    public partial IEnumerable<Book> GetBooks(string jsonFile);
    public partial IEnumerable<Person> GetPeople(string jsonFile);
    public partial IEnumerable<BookWithPrivateSetters> GetBooksWithPrivateSetters(string jsonFile);
    public partial IEnumerable<BookRecord> GetBookRecords(string jsonFile);
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("UnsafeAccessor Generator Sample");
        Console.WriteLine("===============================");
        
        var context = new JsonContext();
        
        try
        {
            var books = context.GetBooks("books.json");
            
            Console.WriteLine("\nBooks (private fields) loaded using UnsafeAccessor:");
            foreach (var book in books)
            {
                Console.WriteLine($"- {book}");
            }

            var people = context.GetPeople("people.json");
            
            Console.WriteLine("\nPeople (private fields) loaded using UnsafeAccessor:");
            foreach (var person in people)
            {
                Console.WriteLine($"- {person}");
            }

            var booksWithPrivateSetters = context.GetBooksWithPrivateSetters("books-private-setters.json");
            
            Console.WriteLine("\nBooks (private setters) loaded using UnsafeAccessor:");
            foreach (var book in booksWithPrivateSetters)
            {
                Console.WriteLine($"- {book}");
            }

            var bookRecords = context.GetBookRecords("book-records.json");
            
            Console.WriteLine("\nBook Records (init accessors) loaded using UnsafeAccessor:");
            foreach (var book in bookRecords)
            {
                Console.WriteLine($"- {book}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("\nThis demonstrates how UnsafeAccessor can be used to:");
        Console.WriteLine("- Access private fields (_title, _publisher)");
        Console.WriteLine("- Set private property setters");
        Console.WriteLine("- Create instances using private constructors");
        Console.WriteLine("- All generated at compile time with source generators!");
    }
}