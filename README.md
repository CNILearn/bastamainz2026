# BASTA! Mainz 2026

[CN innovation](https://www.cninnovation.com)

[C# Blog](https://csharp.christiannagel.com)

## C# 15: From Preview to Production — Praktische Wege in die nächste Sprachgeneration

Dienstag, 29. September 2026: 10:45 - 11:45, Gutenbergsaal 2+3

C# 14 ist eingeführt, C# 15 steht kurz vor dem Release. Diese Session liefert einen kompakten, praxisorientierten Überblick über die wichtigsten Sprachänderungen der aktuellen und kommenden Version. Wir starten mit einem Review auf C# 14 inkl. der C# Erweiterungen auf denen C# 15 aufbaut, bevor wir in die neuen Features von C# 15 eintauchen. Zu C# 15 Features zählen z.B. erweiterte Collection Expressions, Union Types, Readonly Parameters und die tiefere Integration von Async in die Runtime. Gerade Unions können Design von APIs grundlegend verändern. In dieser Session sehen Sie Beispiele in denen Unions gemeinsam mit Pattern Matching die Lesbarkeit erleichtern, Szenarien wo diese Funktionalität und wo sie nicht eingesetzt werden sollte. Sie lernen wie und wann Sie von Runtime Async profitieren können, und Richtungen in denen sich C# weiterentwickelt.

[C# 15: From Preview to Production - Slides](slides/CSharp14-15.pdf)

### Code Samples

## Using Aspire to publish to Azure and AWS

Dienstag, 29. September 2026: 9:00 - 10:00, Gutenbergsaal 2+3

Aspire hat sich in kürzester Zeit zu einem zentralen Baustein für Cloud‑native .NET‑Anwendungen entwickelt. Doch Aspire ist weit mehr als ein lokales Orchestrierungs‑Tool: Mit den integrierten Deployment‑Funktionen lassen sich Anwendungen heute sowohl auf Azure als auch auf AWS konsistent, reproduzierbar und mit minimalem Konfigurationsaufwand bereitstellen. In dieser Session zeigen wir, wie Aspire Projekte strukturiert, Services verbindet, Observability standardisiert und schließlich den Schritt in die Cloud automatisiert. Wir deployen eine Beispielanwendung live nach Azure Container Apps und AWS ECS/Fargate, vergleichen die jeweiligen Deployment‑Flows und beleuchten, wie Aspire Ressourcen, Secrets, Container‑Images und Cloud‑Bindings verwaltet. Teilnehmende lernen, wie man Aspire als „Single Source of Truth“ für lokale Entwicklung, Integrationstests und Cloud‑Deployments nutzt — unabhängig vom Cloud‑Provider. Außerdem diskutieren wir Best Practices für CI/CD‑Pipelines, Identity‑Management, Konfigurationsstrategien und Multi‑Cloud‑Szenarien.

[Using Aspire to publish to Azure and AWS - Slides](slides/SourceGenerators.pdf)

### Code Samples

## Special C# Features Used by Source Generators — Partial Events, Interceptors & UnsafeAccessor

Mittwoch, 30. September 2026: 9:00 - 10:00, Zagrebsaal A

Source Generators gehören inzwischen zu den wichtigsten Erweiterungsmechanismen im modernen .NET‑Ökosystem. Doch ihr volles Potenzial entfalten sie erst durch eine Reihe spezieller C#‑Features, die gezielt für Generator‑Szenarien entwickelt wurden. In dieser Session zeigen wir, wie partial events, Interceptors und das Attribut UnsafeAccessor Generatoren ermöglichen, tief in bestehende APIs einzuhaken, Boilerplate zu eliminieren und leistungsfähige Compile‑Time‑Erweiterungen zu erzeugen. Wir beleuchten, wie partial events Event‑Pipelines erweiterbar machen, wie Interceptors Methodenaufrufe zur Compile‑Zeit ersetzen oder anreichern, und wie UnsafeAccessor kontrollierten Zugriff auf private Member erlaubt — ohne Reflection und ohne Performance‑Kosten. Die Session vermittelt die Konzepte, typische Einsatzmuster, Sicherheitsaspekte und Best Practices für robuste, AOT‑freundliche Generatoren. Teilnehmende erhalten ein klares Verständnis dafür, wann diese Features sinnvoll sind, wie sie zusammenspielen und wie man sie in eigenen Projekten effektiv nutzt.

### The *dotnet new* template

You can use the **dotnet new template** to create a new source generator project:

```bash
dotnet new install CNinnovation.Templates.SourceGenerator

dotnet new sourcegen -n MySourceGenerator
```

### Code Samples