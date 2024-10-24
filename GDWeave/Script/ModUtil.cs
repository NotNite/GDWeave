using GDWeave.Godot;

namespace GDWeave.Modding;

public interface IWaiter {
    public bool Matched { get; }
    public bool Ready { get; }
    public void Reset();
    public void SetReady();
    public bool Check(Token token);
}

public class FunctionWaiter : IWaiter {
    private MultiTokenWaiter functionWaiter;

    public bool Matched {get; private set;} = false;
    public bool Ready {get; private set;} = true;

    public FunctionWaiter(string name, bool waitForReady = false) {
        functionWaiter = new([
            t => t.Type == TokenType.PrFunction,
            t => t is IdentifierToken identifier && identifier.Name == name,
        ]);
    }

    public void Reset() {
        foundFunction = false;
        foundColon = false;
    }

    public void SetReady() {
        Ready = true;
    }

    private bool foundFunction = false;
    private bool foundColon = false;
    public bool Check(Token token) {
        if (!Ready) {
            return false;
        }

        if (foundColon) {
            Matched = true;
            return true;
        }

        if (foundFunction && token.Type == TokenType.Colon) {
            foundColon = true;
            return false;
        }

        if (!functionWaiter.Check(token)) {
            foundFunction = true;
            return false;
        }
        else {
            functionWaiter.Reset();
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
