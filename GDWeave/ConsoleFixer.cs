using System.Runtime.InteropServices;

namespace GDWeave;

public class ConsoleFixer {
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AttachConsole(int pid);

    public static void Init() {
        AllocConsole();
        AttachConsole(-1);

        var stdout = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = true};
        Console.SetOut(stdout);
        var stderr = new StreamWriter(Console.OpenStandardError()) {AutoFlush = true};
        Console.SetError(stderr);
    }
}
