using System.Security;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    // Find
    public static partial class FsHelper
    {
        /// <summary>
        /// An object for static thread locking
        /// </summary>
        public static readonly object SyncObject = new();

        /// <summary>
        /// Search folders (lock <see cref="SyncObject"/> for modifying and <see cref="GetSearchFolders"/> for getting them during locked for modifications)
        /// </summary>
        public static HashSet<string> SearchFolders { get; }

        /// <summary>
        /// Find files in a folder
        /// </summary>
        /// <param name="path">Folder path</param>
        /// <param name="rx">Regular expression for matching a filename</param>
        /// <param name="searchPattern">Search pattern</param>
        /// <param name="recursive">Recursive?</param>
        /// <param name="extensionComparison">File extension string comparison</param>
        /// <param name="extensions">File extensions</param>
        /// <returns>Found files</returns>
        public static IEnumerable<string> FindFiles(
            in string path,
            Regex? rx = null,
            in string? searchPattern = null,
            in bool recursive = true,
            StringComparison extensionComparison = StringComparison.OrdinalIgnoreCase,
            params string[] extensions
            )
            => rx is null && extensions.Length < 1
                ? Directory.EnumerateFiles(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : from file in Directory.EnumerateFiles(path, searchPattern ?? "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                  where (rx?.IsMatch(file) ?? true) &&
                   (extensions.Length < 1 || extensions.Any(ext => file.EndsWith(ext, extensionComparison)))
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
        /// <param name="extensionComparison">File extension string comparison</param>
        /// <param name="extensions">File extensions</param>
        /// <returns>Found files</returns>
        public static IEnumerable<string> FindFilesBackward(
            string path,
            Regex? rx = null,
            string? searchPattern = null,
            string? stopPath = null,
            bool stopIfNotFound = false,
            StringComparison extensionComparison = StringComparison.OrdinalIgnoreCase,
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
                    files = FindFiles(currentPath, rx, searchPattern, recursive: false, extensionComparison, extensions);
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
        /// <param name="rx">Regular expression for matching a folder name</param>
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
            if (ENV.IsBrowserEnvironment) return [];
            lock (SyncObject)
                return includeCurrentDirectory && !SearchFolders.ContainsAny(Environment.CurrentDirectory, "./", ".")
                    ? [Environment.CurrentDirectory, .. SearchFolders]
                    : [.. SearchFolders];
        }

    }
}
