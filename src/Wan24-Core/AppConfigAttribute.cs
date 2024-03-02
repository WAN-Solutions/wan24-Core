﻿namespace wan24.Core
{
    /// <summary>
    /// Attribute for a <see cref="AppConfig"/> property which contains a configuration with an <see cref="IAppConfig"/> to apply
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AppConfigAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AppConfigAttribute() { }

        /// <summary>
        /// Apply after bootstrapping?
        /// </summary>
        public bool AfterBootstrap { get; set; }

        /// <summary>
        /// Priority (lower is being applied first)
        /// </summary>
        public int Priority { get; set; }
    }
}
