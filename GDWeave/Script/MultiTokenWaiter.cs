using GDWeave.Godot;

namespace GDWeave.Modding;

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

