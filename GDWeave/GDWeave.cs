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

        Interop = new Interop();
        Hooks = new Hooks(Interop);
    }

    public static Config GetConfig() {
        var configPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath!)!, "gdweave.json");
        if (!File.Exists(configPath)) {
            return new Config();
        }

        var json = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<Config>(json)!;
    }
}
