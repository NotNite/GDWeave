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

    public static unsafe byte[] ReadRawNullTerminated(nint memoryAddress) {
        var byteCount = 0;
        while (*(byte*) (memoryAddress + byteCount) != 0x00) byteCount++;
        return ReadRaw(memoryAddress, byteCount);
    }

    public static byte[] ReadRaw(nint memoryAddress, int length) {
        var value = new byte[length];
        Marshal.Copy(memoryAddress, value, 0, value.Length);
        return value;
    }

    public static void WriteRaw(nint memoryAddress, byte[] value) {
        Marshal.Copy(value, 0, memoryAddress, value.Length);
    }

    public static void WriteStringNullTerminated(nint memoryAddress, string value) {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteRawNullTerminated(memoryAddress, bytes);
    }

    public static void WriteRawNullTerminated(nint memoryAddress, byte[] value) {
        var length = value.Length;
        Marshal.Copy(value, 0, memoryAddress, length);
        Marshal.WriteByte(memoryAddress + length, 0x00);
    }

    public static nint Alloc(int size) {
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(new byte[size], 0, ptr, size);
        return ptr;
    }
}
