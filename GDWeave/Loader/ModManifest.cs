namespace GDWeave;

internal class ModManifest {
    public required string Id { get; set; }
    public string? AssemblyPath { get; set; }
    public string? PackPath { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public ModMetadata? Metadata { get; set; }

    internal class ModMetadata {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public string? Homepage { get; set; }
    }
}
