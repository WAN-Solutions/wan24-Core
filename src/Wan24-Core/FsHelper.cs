using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// Filesystem helper
    /// </summary>
    public static class FsHelper
    {
        /// <summary>
        /// An object for static thread locking
        /// </summary>
        public static readonly object SyncObject = new();

        /// <summary>
        /// Constructor
        /// </summary>
        static FsHelper() => SearchFolders = ENV.IsBrowserApp ? [] : [Environment.CurrentDirectory, ENV.AppFolder, Settings.TempFolder];

        /// <summary>
        /// Search folders (lock <see cref="SyncObject"/> for modifying and <see cref="GetSearchFolders"/> for getting them during locked for modifications)
        /// </summary>
        public static HashSet<string> SearchFolders { get; }

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
        /// <param name="rx">Regular expression for matching a folder name</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="recursive">Recursive?</param>
        /// <returns>Found folders</returns>
        public static IEnumerable<string> FindFolders(in string path, Regex? rx = null, in string? searchPattern = null, in bool recursive = true)
            => rx is null
                ? Directory.EnumerateDirectories(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : from folder in Directory.EnumerateDirectories(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                  where (rx?.IsMatch(folder) ?? true)
                  select folder;

        /// <summary>
        /// Find files in a folder backward to the root (or the first unreadable parent folder or I/O error)
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <param name="rx">Regular expression for matching a filename</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="stopPath">Parent path to stop at (won't go up more from that path)</param>
        /// <param name="stopIfNotFound">Stop in the folder where no file was found (don't go up more)?</param>
        /// <param name="extensionComparsion">File extension string comparsion</param>
        /// <param name="extensions">File extensions</param>
        /// <returns>Found files</returns>
        public static IEnumerable<string> FindFilesBackward(
            string path,
            Regex? rx = null,
            string? searchPattern = null,
            string? stopPath = null,
            bool stopIfNotFound = false,
            StringComparison extensionComparsion = StringComparison.OrdinalIgnoreCase,
            params string[] extensions
            )
        {
            if (stopPath is not null) stopPath = Path.GetFullPath(stopPath);
            string lastPath = string.Empty,
                currentPath = Path.GetFullPath(path);
            IEnumerable<string> files;
            IEnumerator<string> filesEnumerator;
            bool first;
            while (!lastPath.Equals(currentPath))
            {
                // Find files in the current path
                try
                {
                    files = FindFiles(currentPath, rx, searchPattern, recursive: false, extensionComparsion, extensions);
                    filesEnumerator = files.GetEnumerator();
                }
                catch (SecurityException)
                {
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
                // Yield the found files
                first = true;
                while (true)
                {
                    try
                    {
                        if (!filesEnumerator.MoveNext()) break;
                        first = false;
                    }
                    catch (SecurityException)
                    {
                        filesEnumerator.Dispose();
                        if (first) throw;
                        yield break;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        filesEnumerator.Dispose();
                        if (first) throw;
                        yield break;
                    }
                    catch (Exception)
                    {
                        filesEnumerator.Dispose();
                        throw;
                    }
                    yield return filesEnumerator.Current;
                }
                filesEnumerator.Dispose();
                if (first && stopIfNotFound) break;
                // Move one folder up
                if (stopPath is not null && stopPath.Equals(currentPath)) break;
                lastPath = currentPath;
                try
                {
                    currentPath = Path.GetFullPath(Path.Combine(lastPath, ".."));
                }
                catch (SecurityException)
                {
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Find folders in a folder backward to the root (or the first unreadable parent folder or I/O error)
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <param name="rx">Regular expression for matching a foldername</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="stopPath">Parent path to stop at (won't go up more from that path)</param>
        /// <param name="stopIfNotFound">Stop in the folder where no folder was found (don't go up more)?</param>
        /// <returns>Found folders</returns>
        public static IEnumerable<string> FindFoldersBackward(
            string path,
            Regex? rx = null,
            string? searchPattern = null,
            string? stopPath = null,
            bool stopIfNotFound = false
            )
        {
            if (stopPath is not null) stopPath = Path.GetFullPath(stopPath);
            string lastPath = string.Empty,
                currentPath = Path.GetFullPath(path);
            IEnumerable<string> folders;
            IEnumerator<string> foldersEnumerator;
            bool first;
            while (!lastPath.Equals(currentPath))
            {
                // Find folders in the current path
                try
                {
                    folders = FindFolders(currentPath, rx, searchPattern, recursive: false);
                    foldersEnumerator = folders.GetEnumerator();
                }
                catch (SecurityException)
                {
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
                // Yield the found folders
                first = true;
                while (true)
                {
                    try
                    {
                        if (!foldersEnumerator.MoveNext()) break;
                        first = false;
                    }
                    catch (SecurityException)
                    {
                        foldersEnumerator.Dispose();
                        if (first) throw;
                        yield break;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        foldersEnumerator.Dispose();
                        if (first) throw;
                        yield break;
                    }
                    catch (Exception)
                    {
                        foldersEnumerator.Dispose();
                        throw;
                    }
                    yield return foldersEnumerator.Current;
                }
                foldersEnumerator.Dispose();
                if (first && stopIfNotFound) break;
                // Move one folder up
                if (stopPath is not null && stopPath.Equals(currentPath)) break;
                lastPath = currentPath;
                try
                {
                    currentPath = Path.GetFullPath(Path.Combine(lastPath, ".."));
                }
                catch (SecurityException)
                {
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Normalize a path (replace backslash with slash)
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Normalized path</returns>
        public static string NormalizePath(in string path) => path.Replace('\\', '/');

        /// <summary>
        /// File a file in several folders
        /// </summary>
        /// <param name="fileName">Filename (or an absolute path)</param>
        /// <param name="folders">Folders for file lookup (if not given, the <see cref="SearchFolders"/> are being used per default)</param>
        /// <returns>Existing filename (absolute path) or <see langword="null"/>, if not found</returns>
        public static string? FindFile(in string fileName, params string[] folders)
        {
            string fn = Path.GetFileName(fileName);
            if (fn != fileName) return File.Exists(fileName) ? Path.GetFullPath(fileName) : null;
            if (folders.Length < 1) folders = GetSearchFolders();
            string res;
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                res = Path.Combine(Path.GetFullPath(folder), fn);
                if (File.Exists(res)) return res;
            }
            return null;
        }

        /// <summary>
        /// File a file in several folders
        /// </summary>
        /// <param name="fileName">Filename (or an absolute path)</param>
        /// <param name="includeCurrentDirectory">Include the current directory?</param>
        /// <param name="folders">Folders for file lookup (if not given, the <see cref="SearchFolders"/> are being used per default)</param>
        /// <returns>Existing filename (absolute path) or <see langword="null"/>, if not found</returns>
        public static string? FindFile(in string fileName, in bool includeCurrentDirectory, params string[] folders)
        {
            if (folders.Length < 1)
            {
                folders = GetSearchFolders(includeCurrentDirectory);
            }
            else if (includeCurrentDirectory && !folders.ContainsAny(Environment.CurrentDirectory, "./", "."))
            {
                folders = [.. folders, Environment.CurrentDirectory];
            }
            return FindFile(fileName, folders);
        }

        /// <summary>
        /// Get the search folders
        /// </summary>
        /// <param name="includeCurrentDirectory">Include the current directory?</param>
        /// <returns>Search folders</returns>
        public static string[] GetSearchFolders(in bool includeCurrentDirectory = false)
        {
            if (ENV.IsBrowserApp) return [];
            lock (SyncObject)
                return includeCurrentDirectory && !SearchFolders.ContainsAny(Environment.CurrentDirectory, "./", ".")
                    ? [Environment.CurrentDirectory, .. SearchFolders]
                    : [.. SearchFolders];
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

        /// <summary>
        /// Normalize a path for display (current OS style; default is Linux; won't validate)
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Normalized display path</returns>
        public static string NormalizeDisplayPath(in string path) => ENV.IsWindows ? NormalizeWindowsDisplayPath(path) : NormalizeLinuxDisplayPath(path);

        /// <summary>
        /// Normalize a Windows path for display (format drive letter and path separator; won't validate!)
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Normalized display path</returns>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static string NormalizeWindowsDisplayPath(in string path)
        {
            int len = path.Length;
            if (len < 1 || !path.TryFindPathSeparator(out _)) return path;
            StringBuilder res = new(len);
            int i;
            if (len > 1 && (path[0] != '/' || path[0] != '\\') && (path[1] == '/' || path[1] == '\\' || (len > 2 && path[1] == ':' && (path[2] == '/' || path[2] == '\\'))))
            {
                // c/... or c\... or c:/... or c:\... -> C...
#if NO_UNSAFE
                using RentedArrayRefStruct<char> buffer = new(len: 1, clean: false);
                Span<char> driveLetter = buffer.Span;
#else
                Span<char> driveLetter = stackalloc char[1];
#endif
                if (path.AsSpan(0, 1).ToUpper(driveLetter, CultureInfo.InvariantCulture) != 1)
                    throw new InvalidProgramException();
                res.Append(driveLetter);
                i = 1;
            }
            else if (len > 2 && (path[0] == '/' || path[0] == '\\') && path[1] != '/' && path[1] != '\\' && (path[1] == '/' || path[1] == '\\' || (len > 3 && (path[2] == '/' || path[2] == '\\'))))
            {
                // /c/... or \c\... or /c:/... or \c:\... or /c\... or \c/... or \c:/... or /c:\... -> C...
#if NO_UNSAFE
                using RentedArrayRefStruct<char> buffer = new(len: 1, clean: false);
                Span<char> driveLetter = buffer.Span;
#else
                Span<char> driveLetter = stackalloc char[1];
#endif
                if (path.AsSpan(1, 1).ToUpper(driveLetter, CultureInfo.InvariantCulture) != 1)
                    throw new InvalidProgramException();
                res.Append(driveLetter);
                i = 2;
            }
            else
            {
                i = 0;
            }
            if (i > 0 && len < i && path[i] != ':') res.Append(':');
            for (; i < len; res.Append(path[i] == '/' ? '\\' : path[i]), i++) ;
            return res.ToString();
        }

        /// <summary>
        /// Normalize a Linux path for display (format path separator; won't validate!)
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Normalized display path</returns>
        public static string NormalizeLinuxDisplayPath(in string path)
        {
            int len = path.Length;
            if (len < 1 || !path.TryFindPathSeparator(out _)) return path;
            StringBuilder res = new(len);
            for (int i = 0; i < len; res.Append(path[i] == '\\' ? '/' : path[i]), i++) ;
            return res.ToString();
        }
    }
}
