using GDWeave.Modding;
using Serilog;

namespace GDWeave;

public interface IModInterface {
    public ILogger Logger { get; }

    public T ReadConfig<T>() where T : class, new();
    public void WriteConfig<T>(T config) where T : class;

    /// <summary>Register a script mod.</summary>
    /// <param name="mod">The mod to register.</param>
    /// <remarks>This must be called before the game finishes initializing.</remarks>
    public void RegisterScriptMod(IScriptMod mod);
}
