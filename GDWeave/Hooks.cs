using System.Runtime.InteropServices;
using System.Text;
using GDWeave.Mods;
using GDWeave.Parser;

namespace GDWeave;

public unsafe class Hooks {
    // GDScript::load_byte_code
    public delegate nint LoadByteCodeDelegate(nint gdscript, GodotString* path);

    // GDScriptTokenizerBuffer::set_code_buffer
    public delegate nint SetCodeBufferDelegate(nint tokenizerBuffer, GodotVector* codeBuffer);

    public ITrackedHook<LoadByteCodeDelegate> LoadByteCodeHook;
    public ITrackedHook<SetCodeBufferDelegate> SetCodeBufferHook;

    public static ThreadLocal<string> CurrentPath = new(() => string.Empty);
    public static object StdoutLock = new();

    public static ScriptModder Modder = null!;

    public Hooks(Interop interop) {
        List<ScriptMod> mods = [];

        if (GDWeave.Config.MenuTweaks) {
            mods.Add(new MenuTweaks());
        }

        if (GDWeave.Config.ControllerSupport) {
            mods.Add(new ControllerInput.InputRegister());
            mods.Add(new ControllerInput.PlayerModifier());
            mods.Add(new ControllerInput.Fishing3Modifier());
        }

        if (GDWeave.Config.SortInventory) {
            mods.Add(new InventorySorter());
        }

        Modder = new ScriptModder(mods);

        var loadByteCodeAddr = interop.ScanText([
            "E8 ?? ?? ?? ?? 85 C0 0F 84 ?? ?? ?? ?? 48 89 7D ?? 48 8D 35",
            "48 89 54 24 ?? 48 89 4C 24 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC 78 02 00 00",
        ]);
        this.LoadByteCodeHook = interop.CreateHook<LoadByteCodeDelegate>(loadByteCodeAddr, this.LoadByteCodeDetour);
        this.LoadByteCodeHook.Enable();

        var setCodeBufferAddr = interop.ScanText([
            "E8 ?? ?? ?? ?? 48 89 5D ?? 48 8D 54 24 ?? 48 8D 4D ?? E8 ?? ?? ?? ?? 44 8B E0",
            "48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 ?? 48 81 EC D0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 4C 8B F9"
        ]);
        this.SetCodeBufferHook = interop.CreateHook<SetCodeBufferDelegate>(setCodeBufferAddr, this.SetCodeBufferDetour);
        this.SetCodeBufferHook.Enable();
    }

    private nint LoadByteCodeDetour(nint gdscript, GodotString* path) {
        CurrentPath.Value = path->Value;
        //lock (StdoutLock) Console.WriteLine($"Hooked load_byte_code: {CurrentPath.Value}");
        return this.LoadByteCodeHook.Original(gdscript, path);
    }

    private nint SetCodeBufferDetour(nint tokenizerBuffer, GodotVector* codeBuffer) {
        var data = codeBuffer->Data;
        var path = CurrentPath.Value ?? string.Empty;
        var ran = false;

        try {
            using var ms = new MemoryStream(data.ToArray());
            using var br = new BinaryReader(ms);
            var gdsc = new GodotScriptFile(br);

            try {
                var gameDir = Path.GetDirectoryName(Environment.ProcessPath)!;
                var outFile = Path.Combine(gameDir, "gdc", path.Replace("res://", ""));
                var outDir = Path.GetDirectoryName(outFile)!;

                if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                if (File.Exists(outFile)) File.Delete(outFile);

                using var outFileHandle = File.OpenWrite(outFile);
                using var cleanBw = new BinaryWriter(outFileHandle);
                gdsc.Write(cleanBw);
            } catch (Exception e) {
                lock (StdoutLock) Console.WriteLine(e);
            }

            try {
                ran = Modder.Run(gdsc, path);
            } catch (Exception e) {
                lock (StdoutLock) Console.WriteLine(e);
            }

            using var output = new MemoryStream();
            using var bw = new BinaryWriter(output);
            gdsc.Write(bw);

            var replacement = output.ToArray();
            /*lock (StdoutLock) {
                Console.WriteLine(
                    $"Hooked set_code_buffer ({path}): "
                    + $"{gdsc.Identifiers.Count} identifiers, "
                    + $"{gdsc.Constants.Count} constants, "
                    + $"{gdsc.Lines.Count} lines, "
                    + $"{gdsc.Tokens.Count} tokens, "
                    + $"{data.Length} orig bytes, "
                    + $"{replacement.Length} new bytes"
                );
            }*/
            data = replacement;
        } catch (Exception e) {
            if (e is not InvalidDataException) {
                lock (StdoutLock) Console.WriteLine(e);
            }
        }

        if (!ran) {
            return this.SetCodeBufferHook.Original(tokenizerBuffer, codeBuffer);
        }

        using var vec = new GodotVectorWrapper(data);
        return this.SetCodeBufferHook.Original(tokenizerBuffer, vec.Vector);
    }

    public static nint CowDataCtor(ReadOnlySpan<byte> buffer) {
        var cowData = Marshal.AllocHGlobal(8 + buffer.Length);
        *(int*) cowData = 1;
        *(int*) (cowData + 4) = buffer.Length;
        buffer.CopyTo(new Span<byte>((void*) (cowData + 8), buffer.Length));
        return cowData + 8;
    }

    public static void CowDataDtor(nint cowData) {
        Marshal.FreeHGlobal(cowData - 8);
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
    public struct CowData : IDisposable {
        [FieldOffset(0x0)] public nint Value;

        public int RefCount => *(int*) (this.Value - 8);
        public int Size => *(int*) (this.Value - 4);

        public static CowData Ctor(ReadOnlySpan<byte> buffer) {
            var cowData = Marshal.AllocHGlobal(8 + buffer.Length);
            *(int*) cowData = 1;                   // Ref count
            *(int*) (cowData + 4) = buffer.Length; // Size
            buffer.CopyTo(new Span<byte>((void*) (cowData + 8), buffer.Length));
            return new CowData {Value = cowData + 8};
        }

        public void Dispose() {
            Marshal.FreeHGlobal(this.Value - 8);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GodotString {
        [FieldOffset(0x0)] public CowData CowData;

        public string Value {
            get {
                var str = Marshal.PtrToStringUni(this.CowData.Value, this.CowData.Size);
                Utils.TrimNullTerminator(ref str);
                return str;
            }
        }

        public static GodotString* Ctor(string value) {
            var str = (GodotString*) Marshal.AllocHGlobal(sizeof(GodotString));
            str->CowData = CowData.Ctor(Encoding.Unicode.GetBytes(value + '\0'));
            return str;
        }

        public static void Dtor(GodotString* str) {
            str->CowData.Dispose();
            Marshal.FreeHGlobal((nint) str);
        }
    }

    public class GodotStringWrapper(string value) : IDisposable {
        public readonly GodotString* String = GodotString.Ctor(value);

        public void Dispose() {
            GodotString.Dtor(this.String);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct GodotVector {
        [FieldOffset(0x8)] public CowData CowData;

        public ReadOnlySpan<byte> Data => new((void*) this.CowData.Value, this.CowData.Size);

        public static GodotVector* Ctor(ReadOnlySpan<byte> buffer) {
            var vector = (GodotVector*) Marshal.AllocHGlobal(sizeof(GodotVector));
            vector->CowData = CowData.Ctor(buffer);
            return vector;
        }

        public static void Dtor(GodotVector* vector) {
            vector->CowData.Dispose();
            Marshal.FreeHGlobal((nint) vector);
        }
    }

    public class GodotVectorWrapper(ReadOnlySpan<byte> buffer) : IDisposable {
        public readonly GodotVector* Vector = GodotVector.Ctor(buffer);

        public void Dispose() {
            GodotVector.Dtor(this.Vector);
        }
    }

    // messageboxa if it's needed
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}
