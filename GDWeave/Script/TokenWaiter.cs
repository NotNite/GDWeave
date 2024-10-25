using GDWeave.Godot;

namespace GDWeave.Modding;

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
