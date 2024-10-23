using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using Serilog.Events;

namespace GDWeave;

internal class ConsoleFixer {
    private static readonly string StdoutPipeName = "GDWeave-Stdout-" + Process.GetCurrentProcess().Id;
    private static readonly string StderrPipeName = "GDWeave-Stderr-" + Process.GetCurrentProcess().Id;

    public static readonly nint StdoutPipeNameAlloc = Marshal.StringToHGlobalUni(@"\\.\pipe\" + StdoutPipeName);
    public static readonly nint StderrPipeNameAlloc = Marshal.StringToHGlobalUni(@"\\.\pipe\" + StderrPipeName);

    private static readonly NamedPipeServerStream StdoutPipe = new(StdoutPipeName, PipeDirection.In, 1);
    private static readonly NamedPipeServerStream StderrPipe = new(StderrPipeName, PipeDirection.In, 1);

    private static ILogger Logger = null!;

    public static void Init() {
        Logger = GDWeave.Logger.ForContext("SourceContext", "Godot");
        CreateThread(0, 0, RunStdoutPipe, 0, 0, 0);
        CreateThread(0, 0, RunStderrPipe, 0, 0, 0);
    }

    private static void RunStdoutPipe() {
        StdoutPipe.WaitForConnection();
        var loggerStream = new LoggerStream(Logger, LogEventLevel.Information);
        StdoutPipe.CopyTo(loggerStream);
    }

    private static void RunStderrPipe() {
        StderrPipe.WaitForConnection();
        var loggerStream = new LoggerStream(Logger, LogEventLevel.Error);
        StderrPipe.CopyTo(loggerStream);
    }

    private delegate void ThreadStartDelegate();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint CreateThread(
        nint threadAttributes, nint stackSize, ThreadStartDelegate startAddress, nint parameter, uint creationFlags,
        nint threadId
    );

    private class LoggerStream(ILogger logger, LogEventLevel level) : Stream {
        private StringBuilder builder = new();

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotImplementedException();
        public override long Position {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush() {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            var str = Encoding.UTF8.GetString(buffer, offset, count);
            builder.Append(str);

            if (str.Contains('\n')) {
                var built = builder.ToString();
                while (built.Contains('\n')) {
                    var index = built.IndexOf('\n');
                    // in the case we're receiving a line that's still being written
                    if (index == built.Length - 1) break;

                    var line = built[..index];
                    built = built[(index + 1)..];

                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger.Write(level, line);
                }

                if (built.Length > 0) {
                    builder = new StringBuilder(built);
                } else {
                    builder.Clear();
                }
            }
        }
    }
}
