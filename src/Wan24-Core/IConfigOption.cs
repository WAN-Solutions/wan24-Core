using Microsoft.Extensions.Primitives;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a configuration option
    /// </summary>
    public interface IConfigOption : IChangeToken
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        object SyncObject { get; }
        /// <summary>
        /// Hosting configuration
        /// </summary>
        IOverridableConfig Configuration { get; }
        /// <summary>
        /// Property name
        /// </summary>
        string PropertyName { get; }
        /// <summary>
        /// Master configuration option
        /// </summary>
        IConfigOption MasterOption { get; }
        /// <summary>
        /// Parent configuration option
        /// </summary>
        IConfigOption? ParentOption { get; }
        /// <summary>
        /// Sub-configuration option
        /// </summary>
        IConfigOption? SubOption { get; }
        /// <summary>
        /// Has this option a (sub-)value?
        /// </summary>
        bool HasValue { get; }
        /// <summary>
        /// Value (may be the default value, if not set)
        /// </summary>
        object? Value { get; set; }
        /// <summary>
        /// Dynamic value (may be the default value, if not set)
        /// </summary>
        dynamic? DynamicValue { get; }
        /// <summary>
        /// Final value (may be the default value, if not set and not overridden)
        /// </summary>
        object? FinalValue { get; }
        /// <summary>
        /// Dynamic final value (may be the default value, if not set and not overridden)
        /// </summary>
        dynamic? DynamicFinalValue { get; }
        /// <summary>
        /// Overriding parent option value
        /// </summary>
        object? ParentValue { get; }
        /// <summary>
        /// Overriding parent option dynamic value
        /// </summary>
        dynamic? DynamicParentValue { get; }
        /// <summary>
        /// Sub-configuration value
        /// </summary>
        object? SubValue { get; }
        /// <summary>
        /// Sub-configuration dynamic value
        /// </summary>
        dynamic? DynamicSubValue { get; }
        /// <summary>
        /// Is set?
        /// </summary>
        bool IsSet { get; }
        /// <summary>
        /// Is changed?
        /// </summary>
        bool IsChanged { get; }
        /// <summary>
        /// Does override the parent option?
        /// </summary>
        bool DoesOverride { get; }
        /// <summary>
        /// Can override the parent option?
        /// </summary>
        bool CanOverride { get; }
        /// <summary>
        /// Can be overridden by a sub-option?
        /// </summary>
        bool CanBeOverridden { get; set; }
        /// <summary>
        /// If the sub-option wants to override this option
        /// </summary>
        bool SubWantsOverride { get; }
        /// <summary>
        /// Does the parent option override its parents option?
        /// </summary>
        bool ParentDoesOverride { get; }
        /// <summary>
        /// If the sub-option overrides this option
        /// </summary>
        bool IsOverridden { get; }
        /// <summary>
        /// Set a dynamic value
        /// </summary>
        /// <param name="value">Value</param>
        void SetDynamicValue(in dynamic? value);
        /// <summary>
        /// Unset the value
        /// </summary>
        /// <param name="recursive">Recursive?</param>
        void Unset(in bool recursive = false);
        /// <summary>
        /// Unset overrides (recursive!)
        /// </summary>
        void UnsetOverrides();
        /// <summary>
        /// Reset the changed state
        /// </summary>
        /// <param name="recursive">Recursive?</param>
        void ResetChanged(in bool recursive = true);
        /// <summary>
        /// Delegate for option events
        /// </summary>
        /// <param name="option">Sender</param>
        /// <param name="oldValue">Old value</param>
        public delegate void Option_Delegate(IConfigOption option, object? oldValue);
        /// <summary>
        /// Raised when the value was changed (bubbles to the root option)
        /// </summary>
        event Option_Delegate? OnChange;
    }
}
