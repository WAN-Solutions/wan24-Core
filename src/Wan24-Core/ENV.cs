using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Environment information
    /// </summary>
    public static class ENV
    {
        /// <summary>
        /// Constructor
        /// </summary>
        static ENV()
        {
            IsBrowserApp = RuntimeInformation.OSDescription == "web";
            IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
#if !RELEASE
            IsDebug = true;
#else
            IsDebug = false;
#endif
        }

        /// <summary>
        /// Is a browser app?
        /// </summary>
        public static bool IsBrowserApp { get; }

        /// <summary>
        /// Is a Windows OS?
        /// </summary>
        public static bool IsWindows { get; }

        /// <summary>
        /// Is a Linux OS?
        /// </summary>
        public static bool IsLinux { get; }

        /// <summary>
        /// Is a <c>DEBUG</c> build?
        /// </summary>
        public static bool IsDebug { get; }
    }
}
