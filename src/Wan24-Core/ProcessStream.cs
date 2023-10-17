using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Process stream (uses STDIN/OUT)
    /// </summary>
    public class ProcessStream : WrapperStream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stdin">STDIN (used for writing)</param>
        /// <param name="stdout">STDOUT (used for reading)</param>
        /// <param name="leaveOpen">Leave the streams open when disposing?</param>
        public ProcessStream(in StreamWriter? stdin, in StreamReader? stdout = null, in bool leaveOpen = false)
            : base(new BiDirectionalStream(stdout?.BaseStream ?? Null, stdin?.BaseStream ?? Null, leaveOpen))
        {
            if (stdin is null && stdout is null) throw new ArgumentNullException(nameof(stdin));
            StdIn = stdin;
            StdOut = stdout;
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
        }

        /// <summary>
        /// Process (will be disposed)
        /// </summary>
        public Process? Process { get; protected set; }

        /// <summary>
        /// Kill the process when disposing?
        /// </summary>
        public bool KillOnDispose { get; set; } = true;

        /// <summary>
        /// STDIN
        /// </summary>
        public StreamWriter? StdIn { get; }

        /// <summary>
        /// STDOUT
        /// </summary>
        public StreamReader? StdOut { get; }

        /// <inheritdoc/>
        public override bool LeaveOpen
        {
            get => (BaseStream as BiDirectionalStream)!.LeaveOpen;
            set => _LeaveOpen = (BaseStream as BiDirectionalStream)!.LeaveOpen = value;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Process is not null)
            {
                if (KillOnDispose && !Process.HasExited) Process.Kill(entireProcessTree: true);
                Process.Dispose();
            }
            StdIn?.Dispose();
            StdOut?.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (Process is not null)
            {
                if (KillOnDispose && !Process.HasExited) Process.Kill(entireProcessTree: true);
                Process.Dispose();
            }
            if (StdIn is not null) await StdIn.DisposeAsync().DynamicContext();
            StdOut?.Dispose();
        }

        /// <summary>
        /// Create a process stream
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="useStdin">Use STDIN for writing?</param>
        /// <param name="useStdout">Use STDOUT for reading?</param>
        /// <param name="killOnDispose">Kill the process when disposing?</param>
        /// <param name="args">Arguments</param>
        /// <returns>Process stream</returns>
        public static ProcessStream Create(in string cmd, in bool useStdin, in bool useStdout, in bool killOnDispose = true, params string[] args)
        {
            if (!useStdin && !useStdout) throw new ArgumentException("STDIN/OUT must be used for streaming anything", nameof(useStdin));
            Process proc = new();
            try
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = cmd;
                proc.StartInfo.ArgumentList.AddRange(args);
                proc.StartInfo.RedirectStandardInput = useStdin;
                proc.StartInfo.RedirectStandardOutput = useStdout;
                proc.Start();
                return new(useStdin ? proc.StandardInput : null, useStdout ? proc.StandardOutput : null)
                {
                    KillOnDispose = killOnDispose
                };
            }
            catch
            {
                proc.Dispose();
                throw;
            }
        }
    }
}
