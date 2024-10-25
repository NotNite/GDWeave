using GDWeave.Godot;

namespace GDWeave.Modding;

public class FunctionWaiter(string name, bool waitForReady = false) : IWaiter {
    private readonly MultiTokenWaiter functionWaiter = new([
        t => t.Type == TokenType.PrFunction,
        t => t is IdentifierToken identifier && identifier.Name == name
    ]);

    public bool Matched { get; private set; }
    public bool Ready { get; private set; } = !waitForReady;

    public void Reset() {
        this.foundFunction = false;
        this.foundColon = false;
        this.Matched = false;
        this.Ready = !waitForReady;
        this.functionWaiter.Reset();
    }

    public void SetReady() {
        this.Ready = true;
    }

    private bool foundFunction = false;
    private bool foundColon = false;

    public bool Check(Token token) {
        if (!this.Ready || this.Matched) return false;

        if (this.foundColon && token.Type == TokenType.Newline) {
            this.Matched = true;
            return true;
        }

        if (this.foundFunction && token.Type == TokenType.Colon) {
            this.foundColon = true;
            return false;
        }

        if (this.functionWaiter.Check(token)) {
            this.foundFunction = true;
            return false;
        }

        return false;
    }

    public bool Check(Token token) {
        throw new NotImplementedException();
    }
}
