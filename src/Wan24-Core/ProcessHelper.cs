using System.Diagnostics;

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
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = cmd;
            proc.StartInfo.ArgumentList.AddRange(args);
            proc.StartInfo.RedirectStandardOutput = returnStdOut;
            proc.Start();
            try
            {
                await proc.WaitForExitAsync(cancellationToken).DynamicContext();
                cancellationToken.ThrowIfCancellationRequested();
                if (proc.ExitCode != 0) throw new IOException($"Process did exit with an exit code != 0 (exit code is #{proc.ExitCode})");
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
    }
}
