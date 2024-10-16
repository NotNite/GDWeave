// ReSharper disable InconsistentNaming

namespace GDWeave;

public class GDWeave {
    public delegate void MainDelegate();

    public static Interop Interop = null!;
    public static Hooks Hooks = null!;

    public static void Main() {
        if (Environment.GetEnvironmentVariable("GDWEAVE_DEBUG") is not null) {
            ConsoleFixer.Init();
        }

        Interop = new Interop();
        Hooks = new Hooks(Interop);
    }
}
