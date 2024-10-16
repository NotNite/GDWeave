using System.Diagnostics;
using System.Runtime.InteropServices;
using Iced.Intel;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.Sigscan;

namespace GDWeave;

public class Interop : IDisposable {
    public nint BaseAddress { get; private init; }
    private readonly Process process;
    private readonly Scanner scanner;

    private List<TrackedHook> tracked = new();

    public Interop() {
        this.process = Process.GetCurrentProcess();
        this.scanner = new Scanner(this.process, this.process.MainModule);
        this.BaseAddress = this.process.MainModule!.BaseAddress;
    }

    public void Dispose() {
        this.tracked.ForEach(x => x.Dispose());
        this.tracked.Clear();
    }

    public nint ScanText(string[] text) {
        foreach (var sig in text) {
            var pattern = this.scanner.FindPattern(sig);
            if (!pattern.Found) {
                Console.WriteLine("Failed to match signature {0}", sig);
                continue;
            }

            var offset = this.process.MainModule!.BaseAddress + pattern.Offset;
            var firstByte = Marshal.ReadByte(offset);
            return firstByte is 0xE8 or 0xE9 ? this.ResolveJmpCall(offset) : offset;
        }

        throw new Exception("Failed to match any signatures");
    }

    public nint GetStaticAddress(string[] sigs, int offset = 0) {
        var addr = this.ScanText(sigs) + offset;

        unsafe {
            var reader = new UnsafeCodeReader((byte*) addr, 64);
            var decoder = Decoder.Create(64, reader, (ulong) addr, DecoderOptions.AMD);

            while (reader.CanReadByte) {
                var instruction = decoder.Decode();
                if (!instruction.IsInvalid &&
                    (instruction.Op0Kind == OpKind.Memory || instruction.Op1Kind == OpKind.Memory))
                    return (nint) instruction.MemoryDisplacement64;
            }
        }

        throw new Exception("Failed to resolve static address from signature");
    }

    private nint ResolveJmpCall(nint address) {
        var offset = Marshal.ReadInt32(address + 1);
        return address + 5 + offset;
    }

    public ITrackedHook<T> CreateHook<T>(nint addr, T detour) where T : Delegate {
        var origBytes = MemoryUtils.ReadRaw(addr, 0x32);
        var hook = ReloadedHooks.Instance.CreateHook(detour, addr);
        hook.Activate();
        hook.Disable();

        var trackedHook = new TrackedHook<T>(addr, origBytes, hook);
        this.tracked.Add(trackedHook);

        return trackedHook;
    }

    private class UnsafeCodeReader : CodeReader {
        private readonly int length;
        private readonly unsafe byte* address;
        private int pos;

        public unsafe UnsafeCodeReader(byte* address, int length) {
            this.length = length;
            this.address = address;
        }

        public bool CanReadByte => this.pos < this.length;

        public override unsafe int ReadByte() => this.pos >= this.length ? -1 : this.address[this.pos++];
    }
}

public interface ITrackedHook : IDisposable {
    public nint Address { get; }
    public bool Enabled { get; }
    public void Enable();
    public void Disable();
    public bool Toggle();
}

public interface ITrackedHook<out T> : ITrackedHook where T : Delegate {
    public T Original { get; }
}

public abstract class TrackedHook(nint addr, byte[] originalBytes) : ITrackedHook {
    public byte[] OriginalBytes => originalBytes;
    public nint Address => addr;

    public abstract bool Enabled { get; }

    public virtual void Dispose() {
        MemoryUtils.VirtualProtect(this.Address, this.OriginalBytes.Length, 0x40, out var oldProtect);
        MemoryUtils.WriteRaw(this.Address, this.OriginalBytes);
        MemoryUtils.VirtualProtect(this.Address, this.OriginalBytes.Length, oldProtect, out _);
    }

    public abstract void Enable();
    public abstract void Disable();
    public abstract bool Toggle();
}

public class TrackedHook<T>(nint addr, byte[] originalBytes, IHook<T> hook)
    : TrackedHook(addr, originalBytes), ITrackedHook<T> where T : Delegate {
    public T Original => hook.OriginalFunction;
    public override bool Enabled => hook.IsHookEnabled;

    public override void Enable() {
        if (!hook.IsHookEnabled) hook.Enable();
    }

    public override void Disable() {
        if (hook.IsHookEnabled) hook.Disable();
    }

    public override bool Toggle() {
        if (hook.IsHookEnabled) {
            hook.Disable();
            return false;
        } else {
            hook.Enable();
            return true;
        }
    }

    public override void Dispose() {
        this.Disable();
        base.Dispose();
    }
}
