namespace GDWeave;

internal class LoadedMod {
    public required ModManifest Manifest;
    public IMod? AssemblyMod;
    public string? PackPath;
}
