namespace GDWeave.Parser;

public abstract class ScriptMod {
    public abstract bool ShouldRun(string path);
    public abstract void Modify(string path, List<Token> tokens);
}
