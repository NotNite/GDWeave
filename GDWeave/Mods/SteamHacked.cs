using GDWeave.Parser;
using GDWeave.Parser.Variants;

namespace GDWeave.Mods;

public class SteamHacked : ScriptMod {
    public override bool ShouldRun(string path) => path == "res://Scenes/Singletons/SteamNetwork.gdc";

    public override void Modify(string path, List<Token> tokens) {
        foreach (var t in tokens) {
            if (t is ConstantToken {Value: StringVariant {Value: "Steam Active under username: "} variant}) {
                variant.Value = "Hacked by GDWeave lol: ";
            }
        }
    }
}
