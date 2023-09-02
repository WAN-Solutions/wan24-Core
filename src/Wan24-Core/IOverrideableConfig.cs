using Microsoft.Extensions.Primitives;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an overrideable configuration
    /// </summary>
    public interface IOverrideableConfig : IChangeToken
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        object SyncObject { get; }
        /// <summary>
        /// GUID
        /// </summary>
        Guid GUID { get; }
        /// <summary>
        /// Master configuration
        /// </summary>
        IOverrideableConfig MasterConfig { get; }
        /// <summary>
        /// Overridden parent configuration
        /// </summary>
        IOverrideableConfig? ParentConfig { get; }
        /// <summary>
        /// Configuration level (starts at <c>1</c>)
        /// </summary>
        int ConfigLevel { get; }
        /// <summary>
        /// Overriding sub-configuration
        /// </summary>
        IOverrideableConfig? SubConfig { get; }
        /// <summary>
        /// Property names
        /// </summary>
        IEnumerable<string> Properties { get; }
        /// <summary>
        /// Only locally set values (key ist the property name, equals <see cref="LocalConfig"/>, if this is a sub-configuration)
        /// </summary>
        Dictionary<string, object?> SetValues { get; }
        /// <summary>
        /// Only locally set dynamic values (key ist the property name, equals <see cref="DynamicLocalConfig"/>, if this is a sub-configuration)
        /// </summary>
        Dictionary<string, dynamic?> DynamicSetValues { get; }
        /// <summary>
        /// Only locally changed values (key ist the property name)
        /// </summary>
        Dictionary<string, object?> ChangedValues { get; }
        /// <summary>
        /// Only locally changed dynamic values (key ist the property name)
        /// </summary>
        Dictionary<string, dynamic?> DynamicChangedValues { get; }
        /// <summary>
        /// Local configuration (key ist the property name, all set values, or all values for the master configuration)
        /// </summary>
        Dictionary<string, object?> LocalConfig { get; }
        /// <summary>
        /// Local dynamic configuration (key ist the property name, all set values, or all values for the master configuration)
        /// </summary>
        Dictionary<string, dynamic?> DynamicLocalConfig { get; }
        /// <summary>
        /// Local overriding configuration (key ist the property name, only parent overriding option values)
        /// </summary>
        Dictionary<string, object?> Overrides { get; }
        /// <summary>
        /// Local dynamic overriding configuration (key ist the property name, only parent overriding option values, equals <see cref="LocalConfig"/>, if this is the master configuration)
        /// </summary>
        Dictionary<string, dynamic?> DynamicOverrides { get; }
        /// <summary>
        /// Final configuration (key ist the property name, local including sub-configuration overrides, equals <see cref="LocalConfig"/>, if this is the master configuration)
        /// </summary>
        Dictionary<string, object?> FinalConfig { get; }
        /// <summary>
        /// Final dynamic configuration (key ist the property name, local including sub-configuration overrides)
        /// </summary>
        Dictionary<string, dynamic?> DynamicFinalConfig { get; }
        /// <summary>
        /// Configuration tree (key ist the property name, contains the set values and (if having a sub-configuration) the sub-configuration tree using the special key <c>_sub</c>)
        /// </summary>
        Dictionary<string, dynamic?> ConfigTree { get; set; }
        /// <summary>
        /// All options
        /// </summary>
        IEnumerable<IConfigOption> AllOptions { get; }
        /// <summary>
        /// Set (and overridden) options
        /// </summary>
        IEnumerable<IConfigOption> SetOptions { get; }
        /// <summary>
        /// All options dictionary (key ist the property name)
        /// </summary>
        Dictionary<string, IConfigOption> AllOptionsDict { get; }
        /// <summary>
        /// Missing values (no (sub-)value set, ignores default values)
        /// </summary>
        IEnumerable<IConfigOption> MissingValues { get; }
        /// <summary>
        /// Get an option
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>Option</returns>
        IConfigOption? GetOption(in string propertyName);
        /// <summary>
        /// Set a configuration
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="reset">Reset first?</param>
        /// <param name="recursive">Reset recursive?</param>
        /// <returns>This</returns>
        IOverrideableConfig SetConfig(in Dictionary<string, object?> config, in bool reset = false, in bool recursive = false);
        /// <summary>
        /// Unset all option values
        /// </summary>
        /// <param name="recursive">Recursive?</param>
        /// <returns>This</returns>
        IOverrideableConfig UnsetAll(in bool recursive = false);
        /// <summary>
        /// Unset all option overrides (recursive!)
        /// </summary>
        /// <returns>This</returns>
        IOverrideableConfig UnsetAllOverrides();
        /// <summary>
        /// Reset the changed state
        /// </summary>
        /// <param name="recursive">Recursive?</param>
        /// <returns>This</returns>
        IOverrideableConfig ResetChanged(in bool recursive = true);
        /// <summary>
        /// Delegate for configuration events
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        public delegate void Config_Delegate(IOverrideableConfig sender, ConfigEventArgs e);
        /// <summary>
        /// Raised when an option was changed (bubbles to the master configuration)
        /// </summary>
        event Config_Delegate? OnChange;
    }
}
