namespace GDWeave;

internal class ModManifest {
    public required string Id { get; set; }
    public string? AssemblyPath { get; set; }
    public string? PackPath { get; set; }
    public List<string> Dependencies { get; set; } = new();
}
