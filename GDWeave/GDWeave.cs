using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using GDWeave.Modding;
using Serilog;

namespace GDWeave;

// ReSharper disable InconsistentNaming
internal class GDWeave {
    public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
    public static readonly string Version = Assembly.GetName().Version!.ToString();
    public static string GameDir = null!;
    public static string GDWeaveDir => Path.Combine(GameDir, "GDWeave");

    public static ILogger Logger = null!;
    public static ModLoader ModLoader = null!;
    public static Interop Interop = null!;
    public static Hooks Hooks = null!;

    public static JsonSerializerOptions JsonSerializerOptions = new() {
        WriteIndented = true,
        Converters = {new JsonStringEnumConverter()}
    };

    public delegate void MainDelegate();

    public static void Main() {
        try {
            Init();
        } catch (Exception e) {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (Logger is not null) {
                Logger.Error(e, "GDWeave failed to initialize");
            } else {
                Console.WriteLine($"GDWeave failed to initialize: {e.Message}");
            }
        }
    }

    private static void Init() {
        GameDir = Path.GetDirectoryName(Environment.ProcessPath!)!;

        var logPath = Path.Combine(GDWeaveDir, "GDWeave.log");
        if (File.Exists(logPath)) File.Delete(logPath);

        var config = new LoggerConfiguration()
            .WriteTo.File(logPath)
            .WriteTo.Console();

        if (Environment.GetEnvironmentVariable("GDWEAVE_DEBUG") is not null) {
            config.MinimumLevel.Verbose();
        } else {
            config.MinimumLevel.Information();
        }

        Logger = config.CreateLogger();
        Log.Logger = Logger;
        ConsoleFixer.Init();

        const string github = "https://github.com/NotNite/GDWeave";
        Logger.Information("This is GDWeave {Version} - {GitHub}", Version, github);

        ModLoader = new ModLoader();
        Interop = new Interop();

        List<IScriptMod> scriptMods = [
            ..ModLoader.ScriptMods,
        ];
        var hasPckFiles = ModLoader.LoadedMods.Where(m => m.PackPath is not null).ToList();
        if (hasPckFiles.Count > 0) scriptMods.Add(new PackFileLoader(hasPckFiles));

        var scriptModder = new ScriptModder(scriptMods);
        Hooks = new Hooks(scriptModder, Interop);
    }
}
