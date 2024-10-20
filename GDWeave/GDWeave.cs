using System.Reflection;
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
        ConsoleFixer.Init();

        GameDir = Path.GetDirectoryName(Environment.ProcessPath!)!;

        var logPath = Path.Combine(GDWeaveDir, "GDWeave.log");
        if (File.Exists(logPath)) File.Delete(logPath);

        var config = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(logPath)
            .WriteTo.Console();
        Logger = config.CreateLogger();
        Log.Logger = Logger;

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
