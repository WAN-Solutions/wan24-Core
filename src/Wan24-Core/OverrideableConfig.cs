using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an overrideable configuration
    /// </summary>
    /// <typeparam name="tFinal">Final type</typeparam>
    public abstract class OverrideableConfig<tFinal> : IOverrideableConfig where tFinal : OverrideableConfig<tFinal>, new()
    {
        /// <summary>
        /// Default sub-configuration tree key
        /// </summary>
        public const string DEFAULT_SUB_KEY = "_sub";

        /// <summary>
        /// Change token
        /// </summary>
        protected readonly ChangeToken ChangeToken;
        /// <summary>
        /// Properties
        /// </summary>
        protected Dictionary<string, PropertyInfo>? OptionProperties = null;
        /// <summary>
        /// Sub-configuration tree key
        /// </summary>
        protected string SubKey = DEFAULT_SUB_KEY;

        /// <summary>
        /// Constructor
        /// </summary>
        protected OverrideableConfig() => ChangeToken = new(() => ChangedValues.Any());

        /// <summary>
        /// Constructor
        /// </summary>
        protected OverrideableConfig(tFinal parent)
        {
            ParentConfig = parent;
            ChangeToken = new(() => ChangedValues.Any());
        }

        /// <inheritdoc/>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Master configuration
        /// </summary>
        public tFinal MasterConfig => ParentConfig == null ? (tFinal)this : ParentConfig.MasterConfig;

        /// <summary>
        /// Parent configuration
        /// </summary>
        public tFinal? ParentConfig { get; protected set; }

        /// <inheritdoc/>
        public int ConfigLevel => ParentConfig == null ? 1 : ParentConfig.ConfigLevel + 1;

        /// <summary>
        /// Overriding sub-configuration
        /// </summary>
        public tFinal? SubConfig { get; protected set; }

        /// <inheritdoc/>
        public IEnumerable<string> Properties
            => from pi in GetOptionProperties()
               select pi.Name;

        /// <inheritdoc/>
        public Dictionary<string, object?> SetValues
            => new(from pi in GetOptionProperties()
                   where GetOption(pi).IsSet
                   select new KeyValuePair<string, object?>(pi.Name, GetPropertyValue(pi)));

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicSetValues => SetValues;

        /// <inheritdoc/>
        public Dictionary<string, object?> ChangedValues
            => new(from pi in GetOptionProperties()
                   where GetOption(pi).IsChanged
                   select new KeyValuePair<string, object?>(pi.Name, GetPropertyValue(pi)));

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicChangedValues => ChangedValues;

        /// <inheritdoc/>
        public Dictionary<string, object?> LocalConfig
        {
            get
            {
                bool isMaster = ParentConfig == null;
                return new(from pi in GetOptionProperties()
                           where isMaster || GetOption(pi).IsSet
                           select new KeyValuePair<string, object?>(pi.Name, GetPropertyValue(pi)));
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicLocalConfig => LocalConfig;

        /// <inheritdoc/>
        public Dictionary<string, object?> Overrides
        {
            get
            {
                if (ParentConfig == null) return LocalConfig;
                return new(from pi in GetOptionProperties()
                           where GetOption(pi).CanOverride && GetOption(pi).IsSet
                           select new KeyValuePair<string, object?>(pi.Name, GetPropertyValue(pi)));
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicOverrides => Overrides;

        /// <inheritdoc/>
        public Dictionary<string, object?> FinalConfig
            => ParentConfig == null
                ? new(from pi in GetOptionProperties()
                      select new KeyValuePair<string, object?>(pi.Name, GetPropertyValue(pi, final: true)))
                : ParentConfig.FinalConfig;

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicFinalConfig => FinalConfig;

        /// <inheritdoc/>
        public IEnumerable<IConfigOption> AllOptions
            => from pi in GetOptionProperties()
               select GetOption(pi);

        /// <inheritdoc/>
        public IEnumerable<IConfigOption> SetOptions
            => from pi in GetOptionProperties()
               where GetOption(pi).IsSet || GetOption(pi).IsOverridden
               select GetOption(pi);

        /// <inheritdoc/>
        public Dictionary<string, IConfigOption> AllOptionsDict
            => new(from pi in GetOptionProperties()
                   select new KeyValuePair<string, IConfigOption>(pi.Name, GetOption(pi)));

        /// <inheritdoc/>
        public IEnumerable<IConfigOption> MissingValues
            => from option in AllOptions
               where !option.HasValue
               select option;

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> ConfigTree
        {
            get
            {
                Dictionary<string, dynamic?> res = DynamicSetValues;
                if (SubConfig != null) res[SubKey] = SubConfig.ConfigTree;
                return res;
            }
            set
            {
                GetOptionProperties();
                foreach(KeyValuePair<string, dynamic?> kvp in value)
                {
                    if (!OptionProperties!.ContainsKey(kvp.Key)) continue;
                    GetOption(kvp.Value)!.SetDynamicValue(kvp.Value);
                }
                if (SubConfig != null && value.ContainsKey(SubKey) && value[SubKey] != null)
                    SubConfig!.ConfigTree = value[SubKey]!;
            }
        }

        /// <inheritdoc/>
        public bool HasChanged => ChangeToken.HasChanged;

        /// <inheritdoc/>
        public bool ActiveChangeCallbacks => ChangeToken.ActiveChangeCallbacks;

        #region IOverrideableConfig properties
        /// <inheritdoc/>
        IOverrideableConfig IOverrideableConfig.MasterConfig => MasterConfig;

        /// <inheritdoc/>
        IOverrideableConfig? IOverrideableConfig.ParentConfig => ParentConfig;

        /// <inheritdoc/>
        IOverrideableConfig? IOverrideableConfig.SubConfig => SubConfig;
        #endregion

        /// <summary>
        /// Set a configuration
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="reset">Reset first?</param>
        /// <param name="recursive">Reset recursive?</param>
        /// <returns>This</returns>
        public virtual tFinal SetConfig(Dictionary<string, object?> config, bool reset = false, bool recursive = false)
        {
            if (reset) UnsetAll(recursive);
            IConfigOption? option;
            foreach (KeyValuePair<string, object?> kvp in config)
            {
                option = GetOption(kvp.Key);
                if (option != null) option.Value = kvp.Value;
            }
            return (tFinal)this;
        }

        /// <summary>
        /// Merge to another configuration (will copy the <see cref="IConfigOption.Value"/> (if set) and <see cref="IConfigOption.CanBeOverridden"/>)
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="recursive">Recursive?</param>
        /// <param name="setCanBeOverridden">Set the <see cref="IConfigOption.CanBeOverridden"/> flags?</param>
        /// <returns>This</returns>
        public tFinal MergeTo(tFinal config, bool recursive = true, bool setCanBeOverridden = true)
        {
            if (setCanBeOverridden)
                foreach (IConfigOption option in AllOptions)
                    GetOption(option.PropertyName)!.CanBeOverridden = option.CanBeOverridden;
            foreach (IConfigOption option in SetValues.Cast<IConfigOption>()) GetOption(option.PropertyName)!.Value = option.Value;
            if (recursive && SubConfig != null && config.SubConfig != null) SubConfig.MergeTo(config.SubConfig, recursive);
            return (tFinal)this;
        }

        /// <inheritdoc/>
        public IConfigOption? GetOption(string propertyName)
        {
            if (OptionProperties == null) GetOptionProperties();
            return OptionProperties!.TryGetValue(propertyName, out PropertyInfo? pi)
                ? pi.GetValue(this) as IConfigOption
                : null;
        }

        /// <summary>
        /// Unset all option values
        /// </summary>
        /// <param name="recursive">Recursive?</param>
        /// <returns>This</returns>
        public virtual tFinal UnsetAll(bool recursive = false)
        {
            foreach (IConfigOption option in AllOptions) option.Unset(recursive);
            return (tFinal)this;
        }

        /// <summary>
        /// Unset all option overrides (recursive!)
        /// </summary>
        /// <returns>This</returns>
        public virtual tFinal UnsetAllOverrides()
        {
            foreach (IConfigOption option in AllOptions) option.UnsetOverrides();
            return (tFinal)this;
        }

        /// <inheritdoc/>
        public virtual tFinal ResetChanged(bool recursive = true)
        {
            foreach (IConfigOption option in from option in AllOptions
                                             where option.IsChanged
                                             select option)
                option.ResetChanged(recursive);
            if (recursive) SubConfig?.ResetChanged(recursive);
            return (tFinal)this;
        }

        /// <inheritdoc/>
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => ChangeToken.RegisterChangeCallback(callback, state);

        /// <summary>
        /// Get the option
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Option</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected IConfigOption GetOption(PropertyInfo pi) => (IConfigOption)(pi.GetValue(this) ?? throw new ArgumentException("No property value", nameof(pi)));

        /// <summary>
        /// Get option properties
        /// </summary>
        /// <returns>Properties</returns>
        protected IEnumerable<PropertyInfo> GetOptionProperties()
            => (OptionProperties ??= new(from pi in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                         where typeof(IConfigOption).IsAssignableFrom(pi.PropertyType)
                                         select new KeyValuePair<string, PropertyInfo>(pi.Name, pi))).Values;

        /// <summary>
        /// Get a property value
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="final">Final value?</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected object? GetPropertyValue(PropertyInfo pi, bool final = false) => final ? GetOption(pi).FinalValue : GetOption(pi).Value;

        #region IOverrideableConfig methods
        /// <inheritdoc/>
        IOverrideableConfig IOverrideableConfig.SetConfig(Dictionary<string, object?> config, bool reset, bool recursive) => SetConfig(config, reset, recursive);

        /// <inheritdoc/>
        IOverrideableConfig IOverrideableConfig.UnsetAll(bool recursive) => UnsetAll(recursive);

        /// <inheritdoc/>
        IOverrideableConfig IOverrideableConfig.UnsetAllOverrides() => UnsetAllOverrides();

        /// <inheritdoc/>
        IOverrideableConfig IOverrideableConfig.ResetChanged(bool recursive) => ResetChanged(recursive);
        #endregion

        /// <inheritdoc/>
        public event IOverrideableConfig.Config_Delegate? OnChange;
        /// <summary>
        /// Raise the <see cref="OnChange"/> event
        /// </summary>
        /// <param name="option">Option</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="changed">Was this instance changed?</param>
        protected virtual void RaiseOnChange(IConfigOption option, object? oldValue, bool changed = true)
        {
            if (changed) ChangeToken.InvokeCallbacks();
            ConfigEventArgs e = new(option, oldValue);
            OnChange?.Invoke(this, e);
            ParentConfig?.RaiseOnChange(option, oldValue, changed: false);
        }
        /// <summary>
        /// Raise the <see cref="OnChange"/> event (called internal from options)
        /// </summary>
        /// <param name="option">Option</param>
        /// <param name="oldValue">Old value</param>
        internal void RaiseOnChangeInt(IConfigOption option, object? oldValue) => RaiseOnChange(option, oldValue);
    }
}
