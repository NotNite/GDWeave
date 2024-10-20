using System.Reflection;
using System.Text.Json;
using GDWeave.Modding;
using Serilog;

namespace GDWeave;

internal class ModLoader {
    public List<LoadedMod> LoadedMods = new();
    public List<IScriptMod> ScriptMods => this.scriptMods.Values.ToList();

    private readonly ILogger logger = GDWeave.Logger.ForContext<ModLoader>();
    private readonly Dictionary<string, IScriptMod> scriptMods = new();

    public ModLoader() {
        this.Load();
    }

    public void Load() {
        var modsDir = Path.Combine(GDWeave.GDWeaveDir, "mods");
        if (!Directory.Exists(modsDir)) return;

        foreach (var modDir in Directory.GetDirectories(modsDir)) {
            try {
                var manifestPath = Path.Combine(modDir, "manifest.json");
                if (!File.Exists(manifestPath)) {
                    logger.Warning("Mod at {ModDir} does not have a manifest.json", modDir);
                    continue;
                }

                var manifest = JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(manifestPath))!;
                if (manifest.Id != Path.GetFileName(modDir)) {
                    logger.Warning("Mod at {ModDir} has an incorrect ID in its manifest", modDir);
                    continue;
                }

                var mod = manifest.AssemblyPath is { } assemblyPath
                              ? this.LoadAssembly(manifest.Id, Path.Combine(modDir, assemblyPath))
                              : null;

                var loadedMod = new LoadedMod {
                    Manifest = manifest,
                    AssemblyMod = mod,
                    PackPath = manifest.PackPath is { } packPath
                                   ? Path.Combine(modDir, packPath)
                                   : null
                };

                this.LoadedMods.Add(loadedMod);
            } catch (Exception e) {
                this.logger.Warning(e, "Failed to load mod at {ModDir}", modDir);
            }
        }
    }

    private IMod? LoadAssembly(string id, string assemblyPath) {
        var fullAssemblyPath = Path.GetFullPath(assemblyPath);
        var context = new ModLoadContext(fullAssemblyPath);
        var assembly = context.LoadFromFile(fullAssemblyPath);
        var modType = assembly.GetTypes().FirstOrDefault(t =>
            t.GetInterfaces().FirstOrDefault(t => t.FullName == typeof(IMod).FullName) != null);

        if (modType == null) {
            logger.Warning("Assembly at {AssemblyPath} does not contain a mod", assemblyPath);
            return null;
        }

        var ctor = modType.GetConstructor([typeof(IModInterface)]);

        return ctor is not null
                   ? ctor.Invoke([new ModInterface(id, this)]) as IMod
                   : Activator.CreateInstance(modType) as IMod;
    }

    public void RegisterScriptMod(string modId, IScriptMod mod) {
        this.scriptMods[modId] = mod;
    }
}
