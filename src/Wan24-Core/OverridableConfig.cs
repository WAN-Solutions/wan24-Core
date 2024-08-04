using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for an overridable configuration
    /// </summary>
    /// <typeparam name="tFinal">Final type</typeparam>
    public abstract class OverridableConfig<tFinal> : ChangeToken<tFinal>, IOverridableConfig where tFinal : OverridableConfig<tFinal>, new()
    {
        /// <summary>
        /// Default sub-configuration tree key
        /// </summary>
        public const string DEFAULT_SUB_KEY = "_sub";

        /// <summary>
        /// Properties
        /// </summary>
        protected Dictionary<string, (PropertyInfoExt Property, Func<object, IConfigOption?> Getter)>? OptionProperties = null;
        /// <summary>
        /// Sub-configuration tree key
        /// </summary>
        protected string SubKey = DEFAULT_SUB_KEY;

        /// <summary>
        /// Constructor
        /// </summary>
        protected OverridableConfig() : base() => ChangeIdentifier = () => ChangedValues.Count != 0;

        /// <summary>
        /// Constructor
        /// </summary>
        protected OverridableConfig(in tFinal parent) : base()
        {
            ParentConfig = parent;
            ChangeIdentifier = () => ChangedValues.Count != 0;
        }

        /// <inheritdoc/>
        public Guid GUID { get; } = Guid.NewGuid();

        /// <summary>
        /// Master configuration
        /// </summary>
        public tFinal MasterConfig => ParentConfig is null ? (tFinal)this : ParentConfig.MasterConfig;

        /// <summary>
        /// Parent configuration
        /// </summary>
        public tFinal? ParentConfig { get; protected set; }

        /// <inheritdoc/>
        public int ConfigLevel => ParentConfig is null ? 1 : ParentConfig.ConfigLevel + 1;

        /// <summary>
        /// Overriding sub-configuration
        /// </summary>
        public tFinal? SubConfig { get; protected set; }

        /// <inheritdoc/>
        public IEnumerable<string> Properties
            => from info in GetOptionProperties()
               select info.Property.Name;

        /// <inheritdoc/>
        public Dictionary<string, object?> SetValues
            => new(from info in GetOptionProperties()
                   where GetOption(info.Getter).IsSet
                   select new KeyValuePair<string, object?>(info.Property.Name, GetPropertyValue(info.Getter)));

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicSetValues => SetValues;

        /// <inheritdoc/>
        public Dictionary<string, object?> ChangedValues
            => new(from info in GetOptionProperties()
                   where GetOption(info.Getter).IsChanged
                   select new KeyValuePair<string, object?>(info.Property.Name, GetPropertyValue(info.Getter)));

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicChangedValues => ChangedValues;

        /// <inheritdoc/>
        public Dictionary<string, object?> LocalConfig
        {
            get
            {
                bool isMaster = ParentConfig is null;
                return new(from info in GetOptionProperties()
                           where isMaster || GetOption(info.Getter).IsSet
                           select new KeyValuePair<string, object?>(info.Property.Name, GetPropertyValue(info.Getter)));
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicLocalConfig => LocalConfig;

        /// <inheritdoc/>
        public Dictionary<string, object?> Overrides
        {
            get
            {
                if (ParentConfig is null) return LocalConfig;
                return new(from info in GetOptionProperties()
                           where GetOption(info.Getter).CanOverride && GetOption(info.Getter).IsSet
                           select new KeyValuePair<string, object?>(info.Property.Name, GetPropertyValue(info.Getter)));
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicOverrides => Overrides;

        /// <inheritdoc/>
        public Dictionary<string, object?> FinalConfig
            => ParentConfig is null
                ? new(from info in GetOptionProperties()
                      select new KeyValuePair<string, object?>(info.Property.Name, GetPropertyValue(info.Getter, final: true)))
                : ParentConfig.FinalConfig;

        /// <inheritdoc/>
        public Dictionary<string, dynamic?> DynamicFinalConfig => FinalConfig;

        /// <inheritdoc/>
        public IEnumerable<IConfigOption> AllOptions
            => from info in GetOptionProperties()
               select GetOption(info.Getter);

        /// <inheritdoc/>
        public IEnumerable<IConfigOption> SetOptions
            => from info in GetOptionProperties()
               where GetOption(info.Getter).IsSet || GetOption(info.Getter).IsOverridden
               select GetOption(info.Getter);

        /// <inheritdoc/>
        public Dictionary<string, IConfigOption> AllOptionsDict
            => new(from info in GetOptionProperties()
                   select new KeyValuePair<string, IConfigOption?>(info.Property.Name, GetOption(info.Getter)));

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
                if (SubConfig is not null) res[SubKey] = SubConfig.ConfigTree;
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
                if (SubConfig is not null && value.TryGetValue(SubKey, out dynamic? v) && v is not null)
                    SubConfig!.ConfigTree = v;
            }
        }

        #region IOverrideableConfig properties
        /// <inheritdoc/>
        IOverridableConfig IOverridableConfig.MasterConfig => MasterConfig;

        /// <inheritdoc/>
        IOverridableConfig? IOverridableConfig.ParentConfig => ParentConfig;

        /// <inheritdoc/>
        IOverridableConfig? IOverridableConfig.SubConfig => SubConfig;
        #endregion

        /// <summary>
        /// Set a configuration
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="reset">Reset first?</param>
        /// <param name="recursive">Reset recursive?</param>
        /// <returns>This</returns>
        public virtual tFinal SetConfig(in Dictionary<string, object?> config, in bool reset = false, in bool recursive = false)
        {
            if (reset) UnsetAll(recursive);
            IConfigOption? option;
            foreach (KeyValuePair<string, object?> kvp in config)
            {
                option = GetOption(kvp.Key);
                if (option is not null) option.Value = kvp.Value;
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
        public tFinal MergeTo(in tFinal config, in bool recursive = true, in bool setCanBeOverridden = true)
        {
            if (setCanBeOverridden)
                foreach (IConfigOption option in AllOptions)
                    GetOption(option.PropertyName)!.CanBeOverridden = option.CanBeOverridden;
            foreach (IConfigOption option in SetValues.Values.Cast<IConfigOption>()) GetOption(option.PropertyName)!.Value = option.Value;
            if (recursive && SubConfig is not null && config.SubConfig is not null) SubConfig.MergeTo(config.SubConfig, recursive);
            return (tFinal)this;
        }

        /// <inheritdoc/>
        public IConfigOption? GetOption(in string propertyName)
        {
            if (OptionProperties is null) GetOptionProperties();
            return OptionProperties!.TryGetValue(propertyName, out var info)
                ? info.Getter(this)
                : null;
        }

        /// <summary>
        /// Unset all option values
        /// </summary>
        /// <param name="recursive">Recursive?</param>
        /// <returns>This</returns>
        public virtual tFinal UnsetAll(in bool recursive = false)
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
        public virtual tFinal ResetChanged(in bool recursive = true)
        {
            foreach (IConfigOption option in from option in AllOptions
                                             where option.IsChanged
                                             select option)
                option.ResetChanged(recursive);
            if (recursive) SubConfig?.ResetChanged(recursive);
            return (tFinal)this;
        }

        /// <summary>
        /// Get the option
        /// </summary>
        /// <param name="getter">Property getter</param>
        /// <returns>Option</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected IConfigOption GetOption(in Func<object, IConfigOption?> getter) => getter(this) ?? throw new ArgumentException("No property value", nameof(getter));

        /// <summary>
        /// Get option properties
        /// </summary>
        /// <returns>Properties</returns>
        protected IEnumerable<(PropertyInfoExt Property, Func<object, IConfigOption?> Getter)> GetOptionProperties()
            => (OptionProperties ??= new(from pi in typeof(tFinal).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                                         where typeof(IConfigOption).IsAssignableFrom(pi.Property.PropertyType)
                                         select new KeyValuePair<string, (PropertyInfoExt Property, Func<object, IConfigOption?> Getter)>(
                                             pi.Property.Name,
                                             (pi, pi.Property.CreateTypedInstancePropertyGetter<object, IConfigOption>())
                                             ))).Values;

        /// <summary>
        /// Get a property value
        /// </summary>
        /// <param name="getter">Property getter</param>
        /// <param name="final">Final value?</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected object? GetPropertyValue(in Func<object, IConfigOption?> getter, in bool final = false) => final ? GetOption(getter).FinalValue : GetOption(getter).Value;

        #region IOverrideableConfig methods
        /// <inheritdoc/>
        IOverridableConfig IOverridableConfig.SetConfig(in Dictionary<string, object?> config, in bool reset, in bool recursive) => SetConfig(config, reset, recursive);

        /// <inheritdoc/>
        IOverridableConfig IOverridableConfig.UnsetAll(in bool recursive) => UnsetAll(recursive);

        /// <inheritdoc/>
        IOverridableConfig IOverridableConfig.UnsetAllOverrides() => UnsetAllOverrides();

        /// <inheritdoc/>
        IOverridableConfig IOverridableConfig.ResetChanged(in bool recursive) => ResetChanged(recursive);
        #endregion

        /// <inheritdoc/>
        public event IOverridableConfig.Config_Delegate? OnChange;
        /// <summary>
        /// Raise the <see cref="OnChange"/> event
        /// </summary>
        /// <param name="option">Option</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="changed">Was this instance changed?</param>
        protected virtual void RaiseOnChange(in IConfigOption option, in object? oldValue, in bool changed = true)
        {
            if (changed)
            {
                InvokeCallbacks();
                RaisePropertyChanged(option.Configuration == this ? option.PropertyName : null);
            }
            ConfigEventArgs e = new(option, oldValue);
            OnChange?.Invoke(this, e);
            ParentConfig?.RaiseOnChange(option, oldValue, changed: false);
        }
        /// <summary>
        /// Raise the <see cref="OnChange"/> event (called internal from options)
        /// </summary>
        /// <param name="option">Option</param>
        /// <param name="oldValue">Old value</param>
        internal void RaiseOnChangeInt(in IConfigOption option, in object? oldValue) => RaiseOnChange(option, oldValue);
    }
}
