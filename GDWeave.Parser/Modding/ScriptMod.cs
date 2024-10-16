namespace GDWeave.Parser;

// Ripoff of Harmony Transpiler
public abstract class ScriptMod {
    public abstract bool ShouldRun(string path);
    public abstract IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens);
}
