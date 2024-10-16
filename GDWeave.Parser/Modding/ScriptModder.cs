namespace GDWeave.Parser;

public class ScriptModder(List<ScriptMod>? mods = null) {
    public void Run(GodotScriptFile file, string path) {
        var hls = Utils.CreateHighLevel(file);

        if (mods != null) {
            foreach (var mod in mods) {
                if (mod.ShouldRun(path)) {
                    hls = mod.Modify(path, hls).ToList();
                }
            }
        }

        // Translate high level tokens back into tokens
        file.Tokens.Clear();
        foreach (var hl in hls) {
            switch (hl) {
                case ConstantToken constant: {
                    var index = file.Constants.FindIndex(x => x.Equals(constant.Value));
                    if (index == -1) {
                        index = file.Constants.Count;
                        file.Constants.Add(constant.Value);
                    }

                    constant.AssociatedData = (uint) index;
                    break;
                }

                case IdentifierToken identifier: {
                    var index = file.Identifiers.FindIndex(x => x == identifier.Name);
                    if (index == -1) {
                        index = file.Identifiers.Count;
                        file.Identifiers.Add(identifier.Name);
                    }

                    identifier.AssociatedData = (uint) index;
                    break;
                }
            }

            file.Tokens.Add(hl);
        }
    }
}
