# Minimal API File-Based App

Demonstrates a .NET 10 Minimal API as a **file-based app** — no `.csproj` required.
The entire application lives in a single `app.cs` file and can be executed directly on Linux via the hashbang (`#!`) at the top.

## What's Demonstrated

1. **File-Based App** – Single `.cs` file with inline SDK and package directives (`#:sdk`), no project file needed
2. **Hashbang / Shebang** – `#!/usr/bin/env dotnet run` makes the file directly executable on Linux
3. **Minimal API** – Lightweight HTTP endpoints with `WebApplication` without controllers
4. **Collection Expressions** – `[...]` syntax to initialize the in-memory list of books
5. **Primary Constructors** – `record Book(...)` provides a concise, immutable data model
6. **Pattern Matching** – `is { } book` guard in the GET-by-id route

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Health / welcome message |
| GET | `/books` | Return all books |
| GET | `/books/{id}` | Return a single book by id (404 if not found) |
| POST | `/books` | Add a new book |
| DELETE | `/books/{id}` | Remove a book by id |

## Running the App

### Windows

```powershell
cd samples\MinimalApiFileBasedApp
dotnet run app.cs
```

### Linux / macOS

#### Option A – via `dotnet run`

```bash
cd samples/MinimalApiFileBasedApp
dotnet run app.cs
```

#### Option B – direct execution (requires execute permission)

```bash
cd samples/MinimalApiFileBasedApp

# Grant execute permission (only needed once)
chmod +x app.cs

# Run directly
./app.cs
```

> **Note:** Direct execution (`./app.cs`) relies on the `#!/usr/bin/env dotnet run` hashbang at the top of the file. The .NET SDK must be on `PATH`.

## Testing the Endpoints

Once the app is running (default: `http://localhost:5000`), test the endpoints:

```bash
# Welcome message
curl http://localhost:5000/

# List all books
curl http://localhost:5000/books

# Get book by id
curl http://localhost:5000/books/1

# Add a new book
curl -X POST http://localhost:5000/books \
  -H "Content-Type: application/json" \
  -d '{"id":4,"title":"Refactoring","author":"Martin Fowler","price":47.99}'

# Delete a book
curl -X DELETE http://localhost:5000/books/4
```

## Key Takeaways

- **No project file needed** – `#:sdk` directives replace `.csproj` for simple scripts
- **Instant startup** – Ideal for demos, tooling, and developer scripts
- **Full ASP.NET Core** – Despite being a single file, you get the complete middleware pipeline
- **Linux-friendly** – The hashbang turns any `.cs` file into a runnable script

## Related Documentation

- [Minimal APIs overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [File-based apps (.NET 10)](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps)
- [Shebang / Hashbang](https://en.wikipedia.org/wiki/Shebang_(Unix))
