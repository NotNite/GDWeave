using System.Text.Json;
using System.Text.Json.Serialization;
using GDWeave.Modding;
using Serilog;

namespace GDWeave;

internal class ModInterface(string modId, ModLoader modLoader) : IModInterface {
    private static JsonSerializerOptions JsonSerializerOptions = new() {
        WriteIndented = true,
        Converters = {new JsonStringEnumConverter()}
    };

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
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions)!;
    }

    public void WriteConfig<T>(T config) where T : class {
        var path = this.GetConfigPath();
        var dir = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(config, JsonSerializerOptions);
        File.WriteAllText(path, json);
    }

    public void RegisterScriptMod(IScriptMod mod) {
        modLoader.RegisterScriptMod(modId, mod);
    }
}
