using System.Diagnostics;
using System.Text;
using static wan24.Core.Logger;

namespace wan24.Core
{
    /// <summary>
    /// Process helper
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Run a command and don't wait for exit (just fire and forget)
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="args">Arguments</param>
        /// <returns>The process ID, if a new process was started, or <c>-1</c>, if not</returns>
        public static int Run(in string cmd, params string[] args)
        {
            using Process proc = new();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.ArgumentList.AddRange(args);
            return proc.Start() ? proc.Id : -1;
        }

        /// <summary>
        /// Run a command and wait for exit (and optional get the STDOUT)
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="returnStdOut">Return the STDOUT contents?</param>
        /// <param name="killOnError">Kill the process on error, if it didn't exit yet?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Arguments</param>
        /// <returns>STDOUT contents</returns>
        public static async Task<byte[]> RunAsync(
            string cmd, 
            bool returnStdOut = false, 
            bool killOnError = true, 
            CancellationToken cancellationToken = default, 
            params string[] args
            )
        {
            using Process proc = new();
            proc.StartInfo.UseShellExecute = !returnStdOut;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.ArgumentList.AddRange(args);
            proc.StartInfo.RedirectStandardOutput = returnStdOut;
            proc.StartInfo.RedirectStandardError = returnStdOut;
            if (returnStdOut) proc.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            proc.Start();
            try
            {
                await proc.WaitForExitAsync(cancellationToken).DynamicContext();
                cancellationToken.ThrowIfCancellationRequested();
                if (proc.ExitCode != 0)
                {
                    if (returnStdOut && Logging.Trace)
                        WriteTrace($"STDERR of \"{cmd}{(args.Length == 0 ? string.Empty : $" {(string.IsNullOrWhiteSpace(proc.StartInfo.Arguments) ? string.Join(' ', proc.StartInfo.ArgumentList) : proc.StartInfo.Arguments)}")}\" exit code #{proc.ExitCode}: {proc.StandardError.ReadToEnd()}");
                    throw new IOException($"Process did exit with an exit code #{proc.ExitCode}");
                }
                if (!returnStdOut) return [];
                using MemoryPoolStream ms = new();
                using StreamReader stdOut = proc.StandardOutput;
                await stdOut.BaseStream.CopyToAsync(ms, cancellationToken).DynamicContext();
                return ms.ToArray();
            }
            catch
            {
                if (killOnError && !proc.HasExited) proc.Kill(entireProcessTree: true);
                throw;
            }
        }

        /// <summary>
        /// Run a command and wait for exit to get the exit code
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="killOnError">Kill the process on error, if it didn't exit yet?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="args">Arguments</param>
        /// <returns>Exit code</returns>
        public static async Task<int> GetExitCodeAsync(
            string cmd,
            bool killOnError = true,
            CancellationToken cancellationToken = default,
            params string[] args
            )
        {
            using Process proc = new();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.ArgumentList.AddRange(args);
            proc.Start();
            try
            {
                await proc.WaitForExitAsync(cancellationToken).DynamicContext();
                cancellationToken.ThrowIfCancellationRequested();
                return proc.ExitCode;
            }
            catch
            {
                if (killOnError && !proc.HasExited) proc.Kill(entireProcessTree: true);
                throw;
            }
        }
    }
}
