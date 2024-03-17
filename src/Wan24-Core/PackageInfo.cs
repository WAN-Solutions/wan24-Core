namespace wan24.Core
{
    /// <summary>
    /// Package informations (3rd party package licenses helper (consumes the JSON output of <c>dotnet-project-licenses -i . -j -t -o licenses.json</c> 
    /// (<see href="https://github.com/tomchavakis/nuget-license"/>))
    /// </summary>
    public sealed record class PackageInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PackageInfo() { }

        /// <summary>
        /// Package name
        /// </summary>
        public required string PackageName { get; init; }

        /// <summary>
        /// Package version
        /// </summary>
        public required Version PackageVersion { get; init; }

        /// <summary>
        /// Package URI
        /// </summary>
        public required Uri PackageUri { get; init; }

        /// <summary>
        /// Copyright
        /// </summary>
        public required string Copyright { get; init; }

        /// <summary>
        /// Author names
        /// </summary>
        public required string[] Authors { get; init; }

        /// <summary>
        /// Description
        /// </summary>
        public required string Description { get; init; }

        /// <summary>
        /// License URI
        /// </summary>
        public required Uri LicenseUrl { get; init; }

        /// <summary>
        /// License type
        /// </summary>
        public required string LicenseType { get; init; }

        /// <summary>
        /// Repository informations
        /// </summary>
        public required RepositoryInfo Repository { get; init; }

        /// <summary>
        /// Repository info
        /// </summary>
        public sealed record class RepositoryInfo
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public RepositoryInfo() { }

            /// <summary>
            /// Type
            /// </summary>
            public required string Type { get; init; }

            /// <summary>
            /// URI
            /// </summary>
            public required Uri Uri { get; init; }

            /// <summary>
            /// Commit
            /// </summary>
            public required string Commit { get; init; }
        }
    }
}
