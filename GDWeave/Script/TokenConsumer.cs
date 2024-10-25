using GDWeave.Godot;

namespace GDWeave.Modding;

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
