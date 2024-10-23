// TODO: engine-dependent

using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using GDWeave.Godot;
using GDWeave.Modding;
using Serilog;

namespace GDWeave;

internal unsafe class Hooks {
    // GDScript::load_byte_code
    private delegate nint LoadByteCodeDelegate(nint gdscript, GodotString* path);

    // GDScriptTokenizerBuffer::set_code_buffer
    private delegate nint SetCodeBufferDelegate(nint tokenizerBuffer, GodotVector* codeBuffer);

    private delegate nint SetUnhandledExceptionFilterDelegate(nint filter);
    private delegate nint UnhandledExceptionFilterDelegate(ExceptionPointersStruct* exceptionInfo);

    private ITrackedHook<SetUnhandledExceptionFilterDelegate> setUnhandledExceptionFilterHook;
    private UnhandledExceptionFilterDelegate unhandledExceptionFilter;

    private ITrackedHook<LoadByteCodeDelegate> loadByteCodeHook;
    private ITrackedHook<SetCodeBufferDelegate> setCodeBufferHook;

    private readonly ILogger logger = GDWeave.Logger.ForContext<Hooks>();
    private readonly ThreadLocal<string> currentPath = new(() => string.Empty);
    private readonly ScriptModder modder;

    private bool dumpGdsc = Environment.GetEnvironmentVariable("GDWEAVE_DUMP_GDSC") is not null;

    private enum PatternType {
        LoadByteCode,
        SetCode
    }

    public Hooks(ScriptModder modder, Interop interop) {
        this.modder = modder;

        // We can't set an exception filter directly as the game overrides it, so we need to hook SetUnhandledExceptionFilter
        var kernel32 = LoadLibrary("kernel32.dll");
        var setUnhandledExceptionFilter = GetProcAddress(kernel32, "SetUnhandledExceptionFilter");
        this.unhandledExceptionFilter = this.UnhandledExceptionFilter;
        this.setUnhandledExceptionFilterHook = interop.CreateHook<SetUnhandledExceptionFilterDelegate>(
            setUnhandledExceptionFilter,
            this.SetUnhandledExceptionFilterDetour);
        this.setUnhandledExceptionFilterHook.Enable();

        var patterns = new Dictionary<PatternType, string[]> {
            [PatternType.LoadByteCode] = [
                "E8 ?? ?? ?? ?? 85 C0 0F 84 ?? ?? ?? ?? 48 89 7D ?? 48 8D 35",
                "48 89 54 24 ?? 48 89 4C 24 ?? 55 53 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC 78 02 00 00"
            ],
            [PatternType.SetCode] = [
                "E8 ?? ?? ?? ?? 48 89 5D ?? 48 8D 54 24 ?? 48 8D 4D ?? E8 ?? ?? ?? ?? 44 8B E0",
                "48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D 6C 24 ?? 48 81 EC D0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 ?? 4C 8B F9"
            ]
        };

        const string patternsJson = "patterns.json";
        if (File.Exists(patternsJson)) {
            patterns = JsonSerializer.Deserialize<Dictionary<PatternType, string[]>>(
                File.ReadAllText(patternsJson), GDWeave.JsonSerializerOptions)!;
        }

        var loadByteCodeAddr = interop.ScanText(patterns[PatternType.LoadByteCode]);
        this.loadByteCodeHook = interop.CreateHook<LoadByteCodeDelegate>(loadByteCodeAddr, this.LoadByteCodeDetour);
        this.loadByteCodeHook.Enable();

        var setCodeBufferAddr = interop.ScanText(patterns[PatternType.SetCode]);
        this.setCodeBufferHook = interop.CreateHook<SetCodeBufferDelegate>(setCodeBufferAddr, this.SetCodeBufferDetour);
        this.setCodeBufferHook.Enable();
    }

    private nint SetUnhandledExceptionFilterDetour(nint filter) {
        return this.setUnhandledExceptionFilterHook.Original(
            Marshal.GetFunctionPointerForDelegate(this.unhandledExceptionFilter));
    }

    private nint UnhandledExceptionFilter(ExceptionPointersStruct* exceptionInfo) {
        this.logger.Error("========== UNHANDLED EXCEPTION!!!");
        this.logger.Error("Exception code: {Code:X8}", exceptionInfo->ExceptionRecord->ExceptionCode);
        this.logger.Error("Exception address: {Address:X8}", exceptionInfo->ExceptionRecord->ExceptionAddress);

        const string message = """
                               The game has crashed. Sorry! :(

                               Try disabling some mods and see if the problem persists. It is very likely this issue was not caused by GDWeave itself but rather a mod.

                               When asking for support, provide the log file in the GDWeave folder in your game install.
                               """;

        _ = MessageBox(IntPtr.Zero, message, "GDWeave", 0x30);
        return 0;
    }

    private nint LoadByteCodeDetour(nint gdscript, GodotString* path) {
        this.currentPath.Value = path->Value;
        return this.loadByteCodeHook.Original(gdscript, path);
    }

    private nint SetCodeBufferDetour(nint tokenizerBuffer, GodotVector* codeBuffer) {
        var data = codeBuffer->Data;
        var path = this.currentPath.Value ?? string.Empty;
        var ran = false;

        try {
            using var ms = new MemoryStream(data.ToArray());
            using var br = new BinaryReader(ms);
            var gdsc = new GodotScriptFile(br);

            if (this.dumpGdsc) {
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
                    this.logger.Warning(e, "Failed to write clean GDSC file");
                }
            }

            try {
                ran = this.modder.Run(gdsc, path);
            } catch (Exception e) {
                this.logger.Error(e, "Failed to run mod on {Path}", path);
            }

            using var output = new MemoryStream();
            using var bw = new BinaryWriter(output);
            gdsc.Write(bw);

            var replacement = output.ToArray();
            data = replacement;
        } catch (Exception e) {
            if (e is not InvalidDataException) {
                this.logger.Error(e, "Failed to parse GDSC file {Path}", path);
            }
        }

        if (!ran) {
            return this.setCodeBufferHook.Original(tokenizerBuffer, codeBuffer);
        }

        using var vec = new GodotVectorWrapper(data);
        return this.setCodeBufferHook.Original(tokenizerBuffer, vec.Vector);
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
                MemoryUtils.TrimNullTerminator(ref str);
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


    [StructLayout(LayoutKind.Sequential)]
    private struct ExceptionPointersStruct {
        public ExceptionRecordStruct* ExceptionRecord;
        // We don't care about context here
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExceptionRecordStruct {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public nint ExceptionRecord;
        public nint ExceptionAddress;
        public uint NumberParameters;
        public fixed uint ExceptionInformation[15];
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern nint LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern nint GetProcAddress(nint hModule, string lpProcName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int MessageBox(nint hwnd, string text, string caption, uint type);
}
