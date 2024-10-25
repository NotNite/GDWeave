using System.Text.Json;
using GDWeave.Modding;
using Serilog;

namespace GDWeave;

internal class ModInterface(string modId, ModLoader modLoader) : IModInterface {
    public ILogger Logger { get; } = GDWeave.Logger.ForContext("SourceContext", modId);

    private string GetConfigPath() => Path.Combine(GDWeave.GDWeaveDir, "configs", $"{modId}.json");

    public T ReadConfig<T>() where T : class, new() {
        var path = this.GetConfigPath();

        if (!File.Exists(path)) {
            var @default = new T();
            this.WriteConfig(@default);
            return @default;
        }

        var json = File.ReadAllText(path);
        var obj = JsonSerializer.Deserialize<T>(json, GDWeave.JsonSerializerOptions)!;
        this.WriteConfig(obj); // apply new fields
        return obj;
    }

    public void WriteConfig<T>(T config) where T : class {
        var path = this.GetConfigPath();
        var dir = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(config, GDWeave.JsonSerializerOptions);
        File.WriteAllText(path, json);
    }

    public void RegisterScriptMod(IScriptMod mod) {
        modLoader.RegisterScriptMod(modId, mod);
    }
}
