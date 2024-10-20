using System.Runtime.InteropServices;
using System.Text;

namespace GDWeave;

public static class MemoryUtils {
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualProtect(nint address, nint size, uint newProtect, out uint oldProtect);

    public static void Unprotect(nint memoryAddress, int size, Action action) {
        VirtualProtect(memoryAddress, size, 0x40, out var oldProtect);
        action();
        VirtualProtect(memoryAddress, size, oldProtect, out _);
    }

    public static byte[] ReadRaw(nint memoryAddress, int length) {
        var value = new byte[length];
        Marshal.Copy(memoryAddress, value, 0, value.Length);
        return value;
    }

    public static void WriteRaw(nint memoryAddress, byte[] value) {
        Marshal.Copy(value, 0, memoryAddress, value.Length);
    }

    public static void TrimNullTerminator(ref string str) {
        var @null = str.IndexOf('\u0000');
        if (@null != -1) str = str[..@null];
    }
}
