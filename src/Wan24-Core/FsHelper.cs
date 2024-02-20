using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Filesystem helper
    /// </summary>
    public static class FsHelper
    {
        /// <summary>
        /// Ensure a folder exists
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="permissions">New folder permissons</param>
        /// <returns>New folder created?</returns>
        public static bool EnsureFolder(in string path, in UnixFileMode? permissions = null)
        {
            if (Directory.Exists(path)) return false;
            CreateFolder(path, permissions);
            return true;
        }

        /// <summary>
        /// Create a folder
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="permissions">Permissions</param>
        /// <returns><see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo CreateFolder(in string path, UnixFileMode? permissions = null)
        {
            if (ENV.IsLinux)
            {
#pragma warning disable CA1416 // Not available on all platforms
                return Directory.CreateDirectory(path, permissions ?? Settings.CreateFolderMode);
#pragma warning restore CA1416 // Not available on all platforms
            }
            else
            {
                return Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Create 
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="mode">Mode</param>
        /// <param name="access">Access</param>
        /// <param name="share">Share</param>
        /// <param name="options">Options</param>
        /// <param name="permissions">Permissions</param>
        /// <param name="overwrite">Overwrite an existing file?</param>
        /// <param name="bufferSize">Buffer size in bytes (zero or one to disable)</param>
        /// <returns><see cref="FileStream"/> (don't forget to dispose!)</returns>
        public static FileStream CreateFileStream(
            in string fileName, 
            in FileMode mode = FileMode.CreateNew, 
            in FileAccess access = FileAccess.ReadWrite, 
            in FileShare share = FileShare.None, 
            in FileOptions options = FileOptions.None, 
            UnixFileMode? permissions = null, 
            in bool overwrite = false,
            in int bufferSize = 4096
            )
        {
            FileStreamOptions fso = new()
            {
                Mode = mode,
                Access = access,
                Share = share,
                Options = options,
                BufferSize = bufferSize
            };
#pragma warning disable CA1416 // Not available on all platforms
            if (ENV.IsLinux && (mode == FileMode.OpenOrCreate || mode == FileMode.Create || mode == FileMode.CreateNew)) fso.UnixCreateMode = permissions ?? Settings.CreateFileMode;
#pragma warning restore CA1416 // Not available on all platforms
            FileStream res = new(fileName, fso);
            try
            {
                if (overwrite && res.CanWrite && res.CanSeek && res.Length > 0) res.SetLength(0);
                return res;
            }
            catch
            {
                res.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Find files in a folder
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <param name="rx">Regular expression for matching a filename</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="recursive">Recursive?</param>
        /// <param name="extensionComparsion">File extension string comparsion</param>
        /// <param name="extensions">File extensions</param>
        /// <returns>Found files</returns>
        public static IEnumerable<string> FindFiles(
            in string path,
            Regex? rx = null,
            in string? searchPattern = null,
            in bool recursive = true,
            StringComparison extensionComparsion = StringComparison.OrdinalIgnoreCase,
            params string[] extensions
            )
            => rx is null && extensions.Length < 1
                ? Directory.EnumerateFiles(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : from file in Directory.EnumerateFiles(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                  where (rx?.IsMatch(file) ?? true) &&
                   (extensions.Length < 1 || extensions.Any(ext => file.EndsWith(ext, extensionComparsion)))
                  select file;

        /// <summary>
        /// Find folders in a folder
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <param name="rx">Regular expression</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="recursive">Recursive?</param>
        /// <returns>Found folders</returns>
        public static IEnumerable<string> FindFolders(in string path, Regex? rx = null, in string? searchPattern = null, in bool recursive = true)
            => rx is null
                ? Directory.EnumerateDirectories(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : from folder in Directory.EnumerateDirectories(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                  where (rx?.IsMatch(folder) ?? true)
                  select folder;
    }
}
