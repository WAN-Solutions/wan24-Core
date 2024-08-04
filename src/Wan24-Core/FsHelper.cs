namespace wan24.Core
{
    /// <summary>
    /// Filesystem helper
    /// </summary>
    public static partial class FsHelper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        static FsHelper() => SearchFolders = ENV.IsBrowserEnvironment ? [] : [Environment.CurrentDirectory, ENV.AppFolder, Settings.TempFolder];

        /// <summary>
        /// Ensure a folder exists
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="permissions">New folder permissions</param>
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
        /// Hide a file
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>If hidden</returns>
        public static bool HideFile(in string fileName)
        {
            FileInfo fi = new(fileName);
            if (!fi.Exists) throw new FileNotFoundException("File not found", fileName);
            fi.Attributes |= FileAttributes.Hidden;
            fi.Refresh();
            return (fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }

        /// <summary>
        /// Unhide a file
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <returns>If unhidden</returns>
        public static bool UnhideFile(in string fileName)
        {
            FileInfo fi = new(fileName);
            if (!fi.Exists) throw new FileNotFoundException("File not found", fileName);
            fi.Attributes &= ~FileAttributes.Hidden;
            fi.Refresh();
            return (fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden;
        }
    }
}
