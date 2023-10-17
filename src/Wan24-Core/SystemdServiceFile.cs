using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// systemd service file helper
    /// </summary>
    public class SystemdServiceFile
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SystemdServiceFile() { }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = Settings.AppId!;

        /// <summary>
        /// Working directory
        /// </summary>
        public string WorkingDirectory { get; set; } = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? Environment.CurrentDirectory;

        /// <summary>
        /// App start command
        /// </summary>
        public string ExecStart { get; set; } = $"/usr/bin/dotnet {Assembly.GetEntryAssembly()?.Location}";

        /// <summary>
        /// Syslog identifier
        /// </summary>
        public string SyslogIdentifier { get; set; } = Settings.AppId!;

        /// <summary>
        /// Username to run as
        /// </summary>
        public string User { get; set; } = "www-data";

        /// <inheritdoc/>
        public override string ToString() => Properties.Resources.SystemdServiceFile.Parse(new Dictionary<string, string>()
        {
            {nameof(Description), Description },
            {nameof(WorkingDirectory), WorkingDirectory },
            {nameof(ExecStart), ExecStart },
            {nameof(SyslogIdentifier), SyslogIdentifier },
            {nameof(User), User }
        });
    }
}
