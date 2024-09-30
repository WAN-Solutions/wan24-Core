using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // Normalize
    public static partial class FsHelper
    {
        /// <summary>
        /// Normalize a path (replace backslash with slash)
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns>Normalized path</returns>
        public static string NormalizePath(in string path) => path.Replace('\\', '/');

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
                using RentedMemoryRef<char> buffer = new(len: 1, clean: false);
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
                using RentedMemoryRef<char> buffer = new(len: 1, clean: false);
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
