using System.Collections.Generic;

namespace Stage2.FileIntegration;

internal class DataSourceConfiguration
{
    public string EntityType { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Templates { get; set; }
    public int? DefaultCount { get; set; }
    public string? SourceFile { get; set; }
}