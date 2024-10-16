namespace GDWeave.Parser;

public class TokenWaiter(Func<Token, bool> check, bool waitForReady = false) {
    private bool matched;
    private bool ready = !waitForReady;

    public void SetReady() {
        this.ready = true;
    }

    public bool Check(Token token) {
        if (check(token) && !this.matched && this.ready) {
            this.matched = true;
            return true;
        }

        return false;
    }
}
