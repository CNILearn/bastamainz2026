# BastaGenSample — .NET Source Generator Template

This template scaffolds a complete .NET source generator solution that includes:

- **`BastaGenSample/`** – The source generator project (`netstandard2.0`)
- **`BastaGenSample.Tests/`** – Unit tests using **xUnit v3** (`4.0.1`)
- **`BastaGenSample.SnapshotTests/`** *(optional)* – Snapshot tests using **xUnit v3** (`4.0.1`) + **Verify** (`33.1.5`)

---

## Using the Template

Install the template from NuGet:

```bash
dotnet new install CNinnovation.Templates.SourceGenerator
```

Scaffold a new generator solution (snapshot tests are disabled by default):

```bash
dotnet new cni-sourcegen -n AwesomeGenerator
cd AwesomeGenerator
```

Scaffold with snapshot tests enabled:

```bash
dotnet new cni-sourcegen -n AwesomeGenerator --IncludeSnapshotTests true
cd AwesomeGenerator
```

Remove the template installation when no longer needed:

```bash
dotnet new uninstall CNinnovation.Templates.SourceGenerator
```

---

## Project Structure

```
AwesomeGenerator/
├── AwesomeGenerator/                   # Source generator (netstandard2.0 by default)
│   ├── AwesomeGenerator.csproj
│   └── AwesomeGeneratorImpl.cs         # IIncrementalGenerator implementation
│
├── AwesomeGenerator.Tests/             # xUnit v3 unit tests
│   ├── AwesomeGenerator.Tests.csproj
│   ├── GlobalUsings.cs
│   ├── TestHelper.cs                   # Compiles code and runs the generator
│   └── AwesomeGeneratorTests.cs
│
├── AwesomeGenerator.SnapshotTests/     # Optional: xUnit v3 snapshot tests via Verify
│   ├── AwesomeGenerator.SnapshotTests.csproj
│   ├── GlobalUsings.cs
│   ├── TestHelper.cs                   # Runs generator and calls Verifier.Verify
│   ├── AwesomeGeneratorSnapshotTests.cs
│   └── Snapshots/                      # Verified snapshot files live here
│
├── Directory.Build.props.example       # Optional Verify fee-status declaration template
│
└── AwesomeGenerator.slnx               # Solution file
```

---

## Getting Started

### Build

```bash
dotnet build AwesomeGenerator.slnx
```

### Run Unit Tests

```bash
dotnet test AwesomeGenerator.Tests/AwesomeGenerator.Tests.csproj
```

### Run Snapshot Tests

```bash
dotnet test AwesomeGenerator.SnapshotTests/AwesomeGenerator.SnapshotTests.csproj
```

### Run All Tests

```bash
dotnet test AwesomeGenerator.slnx
```

---

## How the Generator Works

The template implements a simple but realistic pattern:

1. **Inject an attribute** – `GenerateInfoAttribute` is added to every compilation via
   `RegisterPostInitializationOutput`.

2. **Detect marked classes** – one of two approaches is used (selected at scaffold time):
   - **`CreateSyntaxProvider`** *(default)* – visits all class declarations, uses a fast
     syntactic predicate to filter candidates, then a semantic transform to confirm the
     attribute is present. Flexible and general-purpose.
   - **`ForAttributeWithMetadataName`** – tells Roslyn to watch specifically for
     `[GenerateInfo]` by its fully-qualified metadata name. Roslyn only invokes the
     transform for nodes that already carry the attribute, making it more efficient for
     attribute-driven generators. Recommended when a single well-known attribute triggers
     generation.

3. **Generate a companion class** – For each marked class a `<ClassName>Info` static class
   is generated containing compile-time constants (type name, property count, method count)
   and a `GetSummary()` method.

Replace `GenerateInfoAttribute` and the corresponding generation logic in
`BastaGenSampleImpl.cs` with your own attribute and code generation.

### Choosing a Syntax Provider Mode

| Option | Flag | Best for |
|--------|------|----------|
| `CreateSyntaxProvider` | *(default)* | Any syntax-based detection; maximum flexibility |
| `ForAttributeWithMetadataName` | `-m ForAttributeWithMetadataName` | Attribute-driven generators; better incremental performance |

```bash
# scaffold with the optimized attribute-based approach
dotnet new cni-sourcegen -n AwesomeGenerator -m ForAttributeWithMetadataName
```

---

## Snapshot Testing with Verify

Snapshot tests use the [Verify](https://github.com/VerifyTests/Verify) library together
with `Verify.SourceGenerators` to capture and compare the exact text of every generated
source file.

### Verify Sponsorship / Exemption Setup (Required)

Verify v33+ requires one fee-status declaration during build. The template does **not**
set this for you.

If snapshot tests were included:

1. Copy `Directory.Build.props.example` to `Directory.Build.props` at the solution root.
2. Choose exactly one declaration option in that file (sponsor account, exemption, or
   licensed-until) and provide valid values.
3. Keep time-bounded fields (for example `Verify_SponsorshipExemptionUntil`) current.

For policy and declaration details, see the official Verify documentation:
https://github.com/VerifyTests/Verify/blob/main/docs/maintenance-fee.md

### Running the Bundled Snapshot Tests

After configuring Verify declaration properties, the included snapshot tests run normally.
The template ships with two pre-accepted `.verified.txt` files in
`BastaGenSample.SnapshotTests/Snapshots/`.

### First Run for New Tests

When you add a new snapshot test (or modify the generator), no `.verified.txt` file exists yet for that test. Verify will:

1. Write a `.received.txt` file in `Snapshots/` next to the test class.
2. **Fail the test** so you can inspect the output.
3. Accept it once the output looks correct (see below).

### Accepting / Updating Snapshots

**Via environment variable** (useful in CI pipelines):

```bash
VERIFY_AUTO_APPROVE=true dotnet test AwesomeGenerator.SnapshotTests
```

**Via test runner argument**:

```bash
dotnet test AwesomeGenerator.SnapshotTests -- Verify.AutoVerify=true
```

**Manually**: Rename the `.received.txt` file to `.verified.txt` (remove the `received`
segment) and commit it.

### Snapshot File Location

Verified snapshots are stored in `BastaGenSample.SnapshotTests/Snapshots/` and should be
committed to source control so that future test runs can detect regressions.

---

## xUnit v3

This template targets **xUnit v3** (`xunit.v3` NuGet package), which provides:

- Improved parallelism and performance
- Cleaner async test support
- Better extensibility model

All standard xUnit attributes (`[Fact]`, `[Theory]`, `[InlineData]`, etc.) work the
same as in xUnit v2.

---

## Customizing the Generator

1. Open `BastaGenSampleImpl.cs` and replace the `GenerateInfo` attribute name and logic
   with your own.
2. Update `BastaGenSampleTests.cs` and `BastaGenSampleSnapshotTests.cs` with tests for your
   new behavior.
3. Run `dotnet test` to verify everything works.
4. Accept new snapshots as described above.

---

## Requirements

- **.NET SDK matching the selected test TFM** (for example, `net9.0` or `net10.0`)
- **C# 13** (LangVersion `latest` is set automatically)

---

## Third-Party Dependencies

This template uses the following NuGet packages:

### Source Generator
- **Microsoft.CodeAnalysis.CSharp** (5.9.0) – Roslyn C# compiler API
- **Microsoft.CodeAnalysis.Analyzers** (5.9.0) – Analyzer development tools

### Test Projects
- **xUnit v3** (4.0.1) – Unit testing framework
- **Microsoft.NET.Test.Sdk** (18.10.1) – Test platform
- **xunit.runner.visualstudio** (4.0.0) – Visual Studio test runner
- **coverlet.collector** (10.1.0) – Code coverage collector

### Snapshot Testing
- **Verify.XunitV3** (33.1.5) – Snapshot testing library
- **Verify.SourceGenerators** (2.5.0) – Source generator snapshot extensions

For complete license information and attribution, see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

All dependencies use permissive open-source licenses (MIT or Apache-2.0).

