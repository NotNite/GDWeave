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
    private MultiTokenWaiter _FunctionWaiter = new([
        t => t.Type == TokenType.PrFunction,
        t => t.Type == TokenType.Identifier,
    ]);

    public bool Matched {get; private set;} = false;
    public bool Ready {get; private set;} = true;

    private readonly string _Name;
    public FunctionWaiter(string name)
    {
        _Name = name;
    }

    public void Reset() {
        _FoundFunction = false;
        _FoundColon = false;
    }

    public void SetReady() {
        Ready = true;
    }

    private bool _FoundFunction = false;
    private bool _FoundColon = false;
    public bool Check(Token token)
    {
        if (_FoundColon && token.Type == TokenType.Newline)
        {
            Reset();
            return true;
        }

        if (_FoundFunction && token.Type == TokenType.Colon)
        {
            _FoundColon = true;
            return false;
        }

        if (!_FunctionWaiter.Check(token))
        {
            return false;
        }
        else
        {
            _FunctionWaiter.Reset();
        }

        if (token is IdentifierToken idToken)
        if (idToken.Name == _Name)
        {
            Serilog.Log.Information($"Found token: {idToken.Name}");
            _FoundFunction = true;
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
