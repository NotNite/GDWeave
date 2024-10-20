using GDWeave.Godot;

namespace GDWeave.Modding;

// Ripoff of Harmony Transpiler
public interface IScriptMod {
    public bool ShouldRun(string path);
    public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens);
}
