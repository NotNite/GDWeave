// ReSharper disable InconsistentNaming

using System.Text.Json;

namespace GDWeave;

public class GDWeave {
    public delegate void MainDelegate();

    public static Config Config = null!;
    public static Interop Interop = null!;
    public static Hooks Hooks = null!;

    public static void Main() {
        if (Environment.GetEnvironmentVariable("GDWEAVE_DEBUG") is not null) {
            ConsoleFixer.Init();
        }

        Config = GetConfig();
        Console.WriteLine($"GDWeave: Controller support is {(Config.ControllerSupport ? "enabled" : "disabled")}");
        Console.WriteLine($"GDWeave: Controller vibration is {(Config.ControllerVibration ? "enabled" : "disabled")}");
        Console.WriteLine($"GDWeave: Controller vibration strength is {Config.ControllerVibrationStrength}");

        Interop = new Interop();
        Hooks = new Hooks(Interop);
    }

    public static Config GetConfig() {
        var configPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath!)!, "gdweave.json");
        if (!File.Exists(configPath)) {
            var defaultConfig = new Config();
            string? defaultConfigJson = null;

            try {
                defaultConfigJson = JsonSerializer.Serialize(defaultConfig);
            } catch (Exception e) {
                Console.WriteLine($"GDWeave: Failed to serialize default config, not creating the file: {e.Message}");
            }

            if (defaultConfigJson is not null && defaultConfigJson != "null") {
                try {
                    var configFile = File.CreateText(configPath);
                    configFile.Write(defaultConfigJson);
                    configFile.Close();
                    Console.WriteLine("GDWeave: Created new config file");
                } catch (Exception e) {
                    Console.WriteLine($"GDWeave: Failed to create config file: {e.Message}");
                }
            }

            return defaultConfig;
        }

        try {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<Config>(json);
            if (config is null) {
                throw new Exception("Deserialized result is null");
            }
            Console.WriteLine("GDWeave: Read config file");
            return config;
        } catch (Exception e) {
            Console.WriteLine($"GDWeave: Failed to deserialize config file: {e.Message}");
            return new Config();
        }
    }
}
