// ReSharper disable InconsistentNaming

using System.Reflection;
using System.Text.Json;

namespace GDWeave;

public class GDWeave {
    private static readonly Version VersionObject = Assembly.GetExecutingAssembly().GetName().Version!;
    public static readonly string Version = $"v{VersionObject.Major}.{VersionObject.Minor}.{VersionObject.Build}";

    public delegate void MainDelegate();

    public static Config Config = null!;
    public static Interop Interop = null!;
    public static Hooks Hooks = null!;

    public static void Main() {
        try {
            ConsoleFixer.Init();

            Console.WriteLine($"GDWeave {Version}");

            Config = GetConfig();
            Interop = new Interop();
            Hooks = new Hooks(Interop);
        } catch (Exception e) {
            Console.WriteLine($"GDWeave failed to start: {e.Message}");
        }
    }

    public static Config GetConfig() {
        var configPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath!)!, "gdweave.json");
        return !File.Exists(configPath) ? CreateConfigFile(configPath) : ReadConfigFile(configPath);
    }

    private static Config CreateConfigFile(string configPath) {
        var defaultConfig = new Config();

        try {
            var configFile = File.CreateText(configPath);
            configFile.Write(JsonSerializer.Serialize(defaultConfig));
            configFile.Close();
        } catch (Exception e) {
            Console.WriteLine($"GDWeave: Failed to create config file: {e.Message}");
        }

        return defaultConfig;
    }

    private static Config ReadConfigFile(string configPath) {
        try {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(json)!;
            return config;
        } catch (Exception e) {
            Console.WriteLine($"GDWeave: Failed to deserialize config file: {e.Message}");
            return new Config();
        }
    }
}
