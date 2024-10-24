namespace GDWeave;

internal class LoadedMod {
    public required ModManifest Manifest;
    public required string Directory;
    public IMod? AssemblyMod;
    public string? AssemblyPath;
    public string? PackPath;
}
