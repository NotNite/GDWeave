namespace GDWeave;

internal class ModManifest {
    public required string Id { get; set; }
    public string? AssemblyPath { get; set; }
    public string? PackPath { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public ModMetadata? Metadata { get; set; }
    public Dictionary<string, ModConfigProperty>? ConfigSchema { get; set; }

    internal class ModMetadata {
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public string? Homepage { get; set; }
    }

    internal class ModConfigProperty {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? Pattern { get; set; }
        public List<object>? Enum { get; set; }
        public List<string>? SuggestedEnum { get; set; }
        public float? Minimum { get; set; }
        public float? Maximum { get; set; }
        public float? MultipleOf { get; set; }
        public Dictionary<string, ModConfigProperty>? Properties { get; set; }
        public ModConfigProperty? Items { get; set; }
        public int? MinItems { get; set; }
        public int? MaxItems { get; set; }
    }
}
