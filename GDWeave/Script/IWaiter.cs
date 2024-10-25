using GDWeave.Godot;

namespace GDWeave.Modding;

public interface IWaiter {
    public bool Matched { get; }
    public bool Ready { get; }
    public void Reset();
    public void SetReady();
    public bool Check(Token token);
}
