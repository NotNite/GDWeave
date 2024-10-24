using GDWeave.Godot;

namespace GDWeave.Modding;

public interface IWaiter {
    public bool Matched { get; }
    public bool Ready { get; }
    public void Reset();
    public void SetReady();
    public bool Check(Token token);
}

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
}

public class TokenWaiter(Func<Token, bool> check, bool waitForReady = false) : IWaiter {
    public bool Matched { get; private set; }
    public bool Ready { get; private set; } = !waitForReady;

    public void Reset() {
        this.Matched = false;
        this.Ready = !waitForReady;
    }

    public void SetReady() {
        this.Ready = true;
    }

    public bool Check(Token token) {
        if (!this.Matched && this.Ready && check(token)) {
            this.Matched = true;
            return true;
        }

        return false;
    }
}

public class MultiTokenWaiter(
    Func<Token, bool>[] checks,
    bool waitForReady = false,
    bool allowPartialMatch = false
) : IWaiter {
    public bool Matched { get; private set; }
    public bool Ready { get; private set; } = !waitForReady;
    public uint Step { get; private set; }

    public void Reset() {
        this.Matched = false;
        this.Ready = !waitForReady;
        this.Step = 0;
    }

    public void SetReady() {
        this.Ready = true;
    }

    public bool Check(Token token) {
        if (!this.Matched && this.Ready) {
            if (checks[this.Step](token)) {
                this.Step++;
                if (this.Step >= checks.Length) {
                    this.Matched = true;
                    return true;
                }
            } else if (!allowPartialMatch) {
                this.Reset();
            }
        }

        return false;
    }
}

public class TokenConsumer(Func<Token, bool> check) : IWaiter {
    public bool Matched { get; private set; }
    public bool Ready { get; private set; } = false;

    public void Reset() {
        this.Matched = false;
        this.Ready = false;
    }

    public void SetReady() {
        this.Ready = true;
    }

    public bool Check(Token token) {
        if (!this.Matched && this.Ready) {
            if (check(token)) {
                this.Matched = true;
                return false;
            }

            return true;
        }

        return false;
    }
}
