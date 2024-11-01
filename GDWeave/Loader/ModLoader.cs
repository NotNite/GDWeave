using System.Text.Json;
using GDWeave.Modding;
using Serilog;

namespace GDWeave;

internal class ModLoader {
    public List<LoadedMod> LoadedMods = new();
    public List<IScriptMod> ScriptMods => this.scriptMods.Values.SelectMany(x => x).ToList();

    private readonly ILogger logger = GDWeave.Logger.ForContext<ModLoader>();
    private readonly Dictionary<string, List<IScriptMod>> scriptMods = new();

    public ModLoader() {
        this.Register();
        this.Sort();
        this.LoadAssemblies();
        this.logger.Information("Loaded {Count} mods: {ModIds}", this.LoadedMods.Count,
            this.LoadedMods.Select(x => x.Manifest.Id));
    }

    private void Register() {
        var modsDir = Path.Combine(GDWeave.GDWeaveDir, "mods");
        if (!Directory.Exists(modsDir)) return;

        foreach (var modDir in Directory.GetDirectories(modsDir)) {
            try {
                var manifestPath = Path.Combine(modDir, "manifest.json");
                if (!File.Exists(manifestPath)) {
                    this.logger.Warning("Mod at {ModDir} does not have a manifest.json", modDir);
                    continue;
                }

                var manifest = JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath))!;
                if (this.LoadedMods.Any(x => x.Manifest.Id == manifest.Id)) {
                    this.logger.Warning("Duplicate mod ID: {ModId}", manifest.Id);
                    continue;
                }

                this.logger.Debug("Loading mod {ModId} from {ModDir}", manifest.Id, modDir);

                var loadedMod = new LoadedMod {
                    Manifest = manifest,
                    Directory = modDir,
                    AssemblyMod = null,
                    AssemblyPath = manifest.AssemblyPath is { } assemblyPath
                                       ? Path.Combine(modDir, assemblyPath)
                                       : null,
                    PackPath = manifest.PackPath is { } packPath
                                   ? Path.Combine(modDir, packPath)
                                   : null
                };

                if (loadedMod.AssemblyPath != null && !File.Exists(loadedMod.AssemblyPath)) {
                    this.logger.Warning("Assembly at {AssemblyPath} does not exist", loadedMod.AssemblyPath);
                    continue;
                }

                if (loadedMod.PackPath != null && !File.Exists(loadedMod.PackPath)) {
                    this.logger.Warning("Pack file at {PackPath} does not exist", loadedMod.PackPath);
                    continue;
                }

                this.LoadedMods.Add(loadedMod);
            } catch (Exception e) {
                this.logger.Warning(e, "Failed to load mod at {ModDir}", modDir);
            }
        }
    }

    private void Sort() {
        while (true) {
            var invalidMods = this.LoadedMods
                .Where(x => x.Manifest.Dependencies.Any(d => !this.LoadedMods.Any(m => m.Manifest.Id == d))).ToList();
            if (invalidMods.Count == 0) break;

            foreach (var invalidMod in invalidMods) {
                this.logger.Warning("Mod {ModId} has missing/invalid dependencies: {InvalidDependencies}",
                    invalidMod.Manifest.Id, invalidMod.Manifest.Dependencies);
                this.LoadedMods.Remove(invalidMod);
            }
        }

        var dependencyGraph = this.LoadedMods.ToDictionary(x => x.Manifest.Id, x => x.Manifest.Dependencies);
        var resolvedOrder = new List<string>();
        while (dependencyGraph.Count > 0) {
            var noDependencies = dependencyGraph.Where(x => x.Value.Count == 0).ToList();
            foreach (var (modId, _) in noDependencies) {
                resolvedOrder.Add(modId);
                dependencyGraph.Remove(modId);
            }

            foreach (var (_, dependencies) in dependencyGraph) {
                foreach (var noDependency in noDependencies) {
                    dependencies.Remove(noDependency.Key);
                }
            }

            if (noDependencies.Count == 0) {
                this.logger.Warning("Circular dependency detected: {CircularDependency}", dependencyGraph.Keys);
                break;
            }
        }

        this.logger.Debug("Resolved mod load order: {ResolvedOrder}", resolvedOrder);
        this.LoadedMods = this.LoadedMods.OrderBy(x => resolvedOrder.IndexOf(x.Manifest.Id)).ToList();
    }

    private void LoadAssemblies() {
        var invalidMods = new List<LoadedMod>();

        foreach (var loadedMod in this.LoadedMods) {
            if (loadedMod.AssemblyPath is not { } assemblyPath) continue;

            try {
                this.logger.Debug("Loading assembly for mod {ModId} from {AssemblyPath}", loadedMod.Manifest.Id,
                    assemblyPath);
                var assemblyMod = this.LoadAssembly(loadedMod.Manifest.Id, assemblyPath);
                loadedMod.AssemblyMod = assemblyMod;
            } catch (Exception e) {
                this.logger.Warning(e, "Failed to load assembly for mod {ModId}", loadedMod.Manifest.Id);
                invalidMods.Add(loadedMod);
            }
        }

        foreach (var invalidMod in invalidMods) {
            this.LoadedMods.Remove(invalidMod);
        }
    }

    private IMod? LoadAssembly(string id, string assemblyPath) {
        var fullAssemblyPath = Path.GetFullPath(assemblyPath);
        var context = new ModLoadContext(fullAssemblyPath);
        var assembly = context.LoadFromAssemblyPath(fullAssemblyPath);
        var modType = assembly.GetTypes().FirstOrDefault(t =>
            t.GetInterfaces().FirstOrDefault(t => t.FullName == typeof(IMod).FullName) != null);

        if (modType == null) {
            this.logger.Warning("Assembly at {AssemblyPath} does not contain a mod", assemblyPath);
            return null;
        }

        var ctor = modType.GetConstructor([typeof(IModInterface)]);

        return ctor is not null
                   ? ctor.Invoke([new ModInterface(id, this)]) as IMod
                   : Activator.CreateInstance(modType) as IMod;
    }

    public void RegisterScriptMod(string modId, IScriptMod mod) {
        if (!this.scriptMods.ContainsKey(modId)) this.scriptMods[modId] = new();
        this.scriptMods[modId].Add(mod);
    }
}
