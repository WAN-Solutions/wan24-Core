namespace wan24.Core
{
    /// <summary>
    /// systemd service file helper
    /// </summary>
    public class SystemdServiceFile
    {
        /// <summary>
        /// Default user
        /// </summary>
        public const string DEFAULT_USER = "www-data";
        /// <summary>
        /// Default restart timeout in seconds
        /// </summary>
        public const int DEFAULT_RESTART_TIMEOUT = 10;
        /// <summary>
        /// Default restart behavior
        /// </summary>
        public const string DEFAULT_RESTART = "always";
        /// <summary>
        /// Default kill signal
        /// </summary>
        public const string DEFAULT_KILL_SIGNAL = "SIGINT";
        /// <summary>
        /// Default environment
        /// </summary>
        public const string DEFAULT_ENVIRONMENT= "ASPNETCORE_ENVIRONMENT=Production";
        /// <summary>
        /// Default wanted by
        /// </summary>
        public const string DEFAULT_WANTED_BY = "multi-user.target";

        /// <summary>
        /// Constructor
        /// </summary>
        public SystemdServiceFile() { }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; } = Settings.AppId;

        /// <summary>
        /// Working directory
        /// </summary>
        public string WorkingDirectory { get; set; } = ENV.AppFolder;

        /// <summary>
        /// App start command
        /// </summary>
        public string ExecStart { get; set; } = ENV.AppCommand;

        /// <summary>
        /// Syslog identifier
        /// </summary>
        public string SyslogIdentifier { get; set; } = Settings.AppId;

        /// <summary>
        /// Username to run as
        /// </summary>
        public string User { get; set; } = DEFAULT_USER;

        /// <summary>
        /// Restart service after N seconds if the dotnet service crashes
        /// </summary>
        public int RestartTimeout { get; set; } = DEFAULT_RESTART_TIMEOUT;

        /// <summary>
        /// Restart behavior
        /// </summary>
        public string Restart { get; set; } = DEFAULT_RESTART;

        /// <summary>
        /// Kill signal
        /// </summary>
        public string KillSignal { get; set; } = DEFAULT_KILL_SIGNAL;

        /// <summary>
        /// Environment variables
        /// </summary>
        public string Environment { get; set; } = DEFAULT_ENVIRONMENT;

        /// <summary>
        /// Wanted by
        /// </summary>
        public string WantedBy { get; set; } = DEFAULT_WANTED_BY;

        /// <inheritdoc/>
        public override string ToString() => Properties.Resources.SystemdServiceFile.Parse(GetParserData());

        /// <summary>
        /// Get the parser data for parsing the systemd service file template
        /// </summary>
        /// <returns>Parser data</returns>
        protected virtual Dictionary<string, string> GetParserData() => new()
        {
            {nameof(Description), Description },
            {nameof(WorkingDirectory), WorkingDirectory },
            {nameof(ExecStart), ExecStart },
            {nameof(SyslogIdentifier), SyslogIdentifier },
            {nameof(User), User },
            {nameof(RestartTimeout), RestartTimeout.ToString() },
            {nameof(Restart), Restart },
            {nameof(KillSignal), KillSignal },
            {nameof(Environment), Environment },
            {nameof(WantedBy), WantedBy }
        };
    }
}
