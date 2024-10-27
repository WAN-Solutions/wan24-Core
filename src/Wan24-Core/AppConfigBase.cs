using System.Reflection;
using System.Text.Json.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an app configuration
    /// </summary>
    public abstract class AppConfigBase : IAppConfig
    {
        /// <summary>
        /// Set this instance to <see cref="Applied"/>
        /// </summary>
        protected bool SetApplied = false;

        /// <summary>
        /// Constructor
        /// </summary>
        protected AppConfigBase() { }

        /// <summary>
        /// Applied app configuration
        /// </summary>
        [JsonIgnore]
        public static IAppConfig? Applied { get; set; }

        /// <inheritdoc/>
        public abstract void Apply();

        /// <inheritdoc/>
        public abstract Task ApplyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Apply configurations from properties which have an <see cref="AppConfigAttribute"/>
        /// </summary>
        /// <param name="afterBootstrap">After bootstrapping?</param>
        protected virtual void ApplyProperties(bool afterBootstrap)
        {
            foreach (IAppConfig sub in from pi in GetType().GetPropertiesCached(BindingFlags.Public | BindingFlags.Instance)
                                       where pi.Getter is not null &&
                                        typeof(IAppConfig).IsAssignableFrom(pi.PropertyType) &&
                                        pi.GetCustomAttributeCached<AppConfigAttribute>() is AppConfigAttribute attr &&
                                        attr.AfterBootstrap == afterBootstrap &&
                                        pi.Getter(this) is IAppConfig
                                       orderby pi.GetCustomAttributeCached<AppConfigAttribute>()!.Priority
                                       select pi.Getter!(this) as IAppConfig)
                sub.Apply();
        }

        /// <summary>
        /// Apply configurations from properties which have an <see cref="AppConfigAttribute"/>
        /// </summary>
        /// <param name="afterBootstrap">After bootstrapping?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task ApplyPropertiesAsync(bool afterBootstrap, CancellationToken cancellationToken)
        {
            foreach (IAppConfig sub in from pi in GetType().GetPropertiesCached(BindingFlags.Public | BindingFlags.Instance)
                                       where pi.Getter is not null &&
                                        typeof(IAppConfig).IsAssignableFrom(pi.PropertyType) &&
                                        pi.GetCustomAttributeCached<AppConfigAttribute>() is AppConfigAttribute attr &&
                                        attr.AfterBootstrap == afterBootstrap &&
                                        pi.Getter(this) is IAppConfig
                                       orderby pi.GetCustomAttributeCached<AppConfigAttribute>()!.Priority
                                       select pi.Getter!(this) as IAppConfig)
                await sub.ApplyAsync(cancellationToken).DynamicContext();
        }
    }
}
