using GDWeave.Parser;
using GDWeave.Parser.Variants;

namespace GDWeave.Mods;

public class InventorySorter : ScriptMod {
    public override bool ShouldRun(string path) => path == "res://Scenes/Singletons/playerdata.gdc";

    public override IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        var extendsWaiter = new MultiTokenWaiter([
            t => t.Type is TokenType.PrExtends,
            t => t.Type is TokenType.Newline
        ], allowPartialMatch: true);
        var inventoryAppendWaiter = new MultiTokenWaiter([
            t => t is IdentifierToken {Name: "inventory"},
            t => t.Type is TokenType.Period,
            t => t is IdentifierToken {Name: "append"},
            t => t.Type is TokenType.ParenthesisOpen,
        ]);
        var cosmeticAppendWaiter = new MultiTokenWaiter([
            t => t is IdentifierToken {Name: "cosmetics_unlocked"},
            t => t.Type is TokenType.Period,
            t => t is IdentifierToken {Name: "append"},
            t => t.Type is TokenType.ParenthesisOpen,
        ]);
        var loadedSaveWaiter = new TokenWaiter(t => t is ConstantToken {Value: StringVariant {Value: "Loaded Save"}});
        var newlineWaiter = new TokenWaiter(t => t.Type is TokenType.Newline, waitForReady: true);

        var sortingInventory = false;
        var sortingCosmetics = false;

        foreach (var token in tokens) {
            if (extendsWaiter.Check(token)) {
                yield return token;
                foreach (var t in this.AddSorter()) yield return t;
            } else if (inventoryAppendWaiter.Check(token)) {
                yield return token;
                sortingInventory = true;
                newlineWaiter.SetReady();
            } else if (cosmeticAppendWaiter.Check(token)) {
                yield return token;
                sortingCosmetics = true;
                newlineWaiter.SetReady();
            } else if (loadedSaveWaiter.Check(token)) {
                yield return token;
                sortingInventory = true;
                sortingCosmetics = true;
                newlineWaiter.SetReady();
            } else if (newlineWaiter.Check(token)) {
                yield return token;
                if (sortingInventory) {
                    foreach (var t in this.CallInventorySorter()) yield return t;
                    yield return token;
                    sortingInventory = false;
                }
                if (sortingCosmetics) {
                    foreach (var t in this.CallCosmeticSorter()) yield return t;
                    yield return token;
                    sortingCosmetics = false;
                }

                inventoryAppendWaiter.Reset();
                newlineWaiter.Reset();
            } else {
                yield return token;
            }
        }
    }

    private IEnumerable<Token> AddSorter() {
        // class InventorySorter:
        yield return new Token(TokenType.PrClass);
        yield return new IdentifierToken("InventorySorter");
        yield return new Token(TokenType.Colon);
        yield return new Token(TokenType.Newline, 1);

        // static func sort_inventory(a, b):
        yield return new Token(TokenType.PrStatic);
        yield return new Token(TokenType.PrFunction);
        yield return new IdentifierToken("sort_inventory");
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new IdentifierToken("a");
        yield return new Token(TokenType.Comma);
        yield return new IdentifierToken("b");
        yield return new Token(TokenType.ParenthesisClose);
        yield return new Token(TokenType.Colon);
        yield return new Token(TokenType.Newline, 2);

        // print(a, b)
        yield return new Token(TokenType.BuiltInFunc, (uint?) BuiltinFunction.TextPrint);
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new IdentifierToken("a");
        yield return new Token(TokenType.Comma);
        yield return new IdentifierToken("b");
        yield return new Token(TokenType.ParenthesisClose);
        yield return new Token(TokenType.Newline, 2);

        //     return a["id"] < b["id"]
        yield return new Token(TokenType.CfReturn);
        yield return new IdentifierToken("a");
        yield return new Token(TokenType.BracketOpen);
        yield return new ConstantToken(new StringVariant("id"));
        yield return new Token(TokenType.BracketClose);
        yield return new Token(TokenType.OpLess);
        yield return new IdentifierToken("b");
        yield return new Token(TokenType.BracketOpen);
        yield return new ConstantToken(new StringVariant("id"));
        yield return new Token(TokenType.BracketClose);
        yield return new Token(TokenType.Newline);
    }

    private IEnumerable<Token> CallInventorySorter() {
        // inventory.sort_custom(InventorySorter, "sort_inventory")
        yield return new IdentifierToken("inventory");
        yield return new Token(TokenType.Period);
        yield return new IdentifierToken("sort_custom");
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new IdentifierToken("InventorySorter");
        yield return new Token(TokenType.Comma);
        yield return new ConstantToken(new StringVariant("sort_inventory"));
        yield return new Token(TokenType.ParenthesisClose);
    }

    private IEnumerable<Token> CallCosmeticSorter() {
        yield return new IdentifierToken("cosmetics_unlocked");
        yield return new Token(TokenType.Period);
        yield return new IdentifierToken("sort");
        yield return new Token(TokenType.ParenthesisOpen);
        yield return new Token(TokenType.ParenthesisClose);
    }
}
