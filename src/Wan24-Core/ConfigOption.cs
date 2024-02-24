using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Configuration option
    /// </summary>
    /// <typeparam name="tValue">Value type</typeparam>
    /// <typeparam name="tConfig">Configuration type</typeparam>
    public sealed class ConfigOption<tValue, tConfig> : ChangeToken<ConfigOption<tValue, tConfig>>, IConfigOption where tConfig : OverridableConfig<tConfig>, new()
    {
        /// <summary>
        /// Parent configuration option
        /// </summary>
        private ConfigOption<tValue, tConfig>? _ParentOption = null;
        /// <summary>
        /// Sub-configuration option
        /// </summary>
        private ConfigOption<tValue, tConfig>? _SubOption = null;
        /// <summary>
        /// Value
        /// </summary>
        private tValue? _Value = default;
        /// <summary>
        /// Can be overridden?
        /// </summary>
        private bool _CanBeOverridden;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="canBeOverridden">Can be overridden?</param>
        public ConfigOption(in tConfig config, in string propertyName, in bool canBeOverridden = true) : base()
        {
            ChangeIdentifier = () => IsChanged;
            Configuration = config;
            PropertyName = propertyName;
            _CanBeOverridden = canBeOverridden;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="canBeOverridden">Can be overridden?</param>
        /// <param name="defaultValue">Default value</param>
        public ConfigOption(in tConfig config, in string propertyName, in bool canBeOverridden, in tValue? defaultValue) : base()
        {
            ChangeIdentifier = () => IsChanged;
            Configuration = config;
            PropertyName = propertyName;
            Default = defaultValue;
            _CanBeOverridden = canBeOverridden;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="canBeOverridden">Can be overridden?</param>
        /// <param name="value">Value to set</param>
        /// <param name="defaultValue">Default value</param>
        public ConfigOption(in tConfig config, in string propertyName, in bool canBeOverridden, in tValue? value, in tValue? defaultValue) : base()
        {
            ChangeIdentifier = () => IsChanged;
            Configuration = config;
            PropertyName = propertyName;
            Default = defaultValue;
            _CanBeOverridden = canBeOverridden;
            Value = value;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public Guid GUID { get; } = Guid.NewGuid();

        /// <summary>
        /// Configuration
        /// </summary>
        public tConfig Configuration { get; }

        /// <inheritdoc/>
        public string PropertyName { get; }

        /// <summary>
        /// Master configuration option
        /// </summary>
        public ConfigOption<tValue, tConfig> MasterOption => ParentOption?.MasterOption ?? this;

        /// <summary>
        /// Parent configuration option
        /// </summary>
        public ConfigOption<tValue, tConfig>? ParentOption => _ParentOption ??= (ConfigOption<tValue, tConfig>?)Configuration.ParentConfig?.GetOption(PropertyName);

        /// <summary>
        /// Sub-configuration option
        /// </summary>
        public ConfigOption<tValue, tConfig>? SubOption => _SubOption ??= (ConfigOption<tValue, tConfig>?)Configuration.SubConfig?.GetOption(PropertyName);

        /// <inheritdoc/>
        public bool HasValue => IsSet || (CanBeOverridden && (SubOption?.HasValue ?? false));

        /// <summary>
        /// Value
        /// </summary>
        public tValue? Value
        {
            get => IsSet ? _Value : Default;
            set
            {
                object? oldValue = _Value;
                lock (SyncObject)
                {
                    _Value = value;
                    IsSet = true;
                    IsChanged = true;
                }
                if (oldValue is IOverridableConfig oldConfig) oldConfig.OnChange -= HandleConfigValueChange;
                if (value is IOverridableConfig newConfig) newConfig.OnChange += HandleConfigValueChange;
                RaiseOnChange(oldValue);
                Configuration.RaiseOnChangeInt(this, oldValue);
            }
        }

        /// <summary>
        /// Dynamic value converter callback
        /// </summary>
        public Func<dynamic?,object?>? DynamicValueConverter { get; set; }

        /// <summary>
        /// Final value
        /// </summary>
        public tValue? FinalValue => IsOverridden ? SubOption!.FinalValue : Value;

        /// <summary>
        /// Default value (returned, if is not set)
        /// </summary>
        public tValue? Default { get; set; }

        /// <summary>
        /// Sub-configuration value
        /// </summary>
        public tValue? SubValue => SubOption is null ? default : SubOption.Value;

        /// <summary>
        /// Overriding parent option value
        /// </summary>
        public tValue? ParentValue => ParentOption is null ? default : ParentOption.Value;

        /// <inheritdoc/>
        public bool IsSet { get; private set; }

        /// <inheritdoc/>
        public bool IsChanged { get; private set; }

        /// <inheritdoc/>
        public bool DoesOverride => CanOverride && IsSet;

        /// <inheritdoc/>
        public bool CanBeOverridden
        {
            get => _CanBeOverridden && (ParentOption?.CanBeOverridden ?? true) && SubOption is not null;
            set => _CanBeOverridden = value;
        }

        /// <inheritdoc/>
        public bool CanOverride => ParentOption?.CanBeOverridden ?? false;

        /// <inheritdoc/>
        public bool SubWantsOverride => SubOption?.IsSet ?? false;

        /// <inheritdoc/>
        public bool ParentDoesOverride => ParentOption?.DoesOverride ?? false;

        /// <inheritdoc/>
        public bool IsOverridden => CanBeOverridden && (SubWantsOverride || (SubOption?.IsOverridden ?? false));

        #region IConfigOption properties
        /// <inheritdoc/>
        IOverridableConfig IConfigOption.Configuration => Configuration;

        /// <inheritdoc/>
        IConfigOption IConfigOption.MasterOption => MasterOption;

        /// <inheritdoc/>
        IConfigOption? IConfigOption.ParentOption => ParentOption;

        /// <inheritdoc/>
        IConfigOption? IConfigOption.SubOption => SubOption;

        /// <inheritdoc/>
        object? IConfigOption.Value
        {
            get => Value;
            set => Value = (tValue?)value;
        }

        /// <inheritdoc/>
        dynamic? IConfigOption.DynamicValue => Value;

        /// <inheritdoc/>
        object? IConfigOption.FinalValue => FinalValue;

        /// <inheritdoc/>
        dynamic? IConfigOption.DynamicFinalValue => FinalValue;

        /// <inheritdoc/>
        object? IConfigOption.SubValue => ParentValue;

        /// <inheritdoc/>
        dynamic? IConfigOption.DynamicSubValue => ParentValue;

        /// <inheritdoc/>
        object? IConfigOption.ParentValue => ParentValue;

        /// <inheritdoc/>
        dynamic? IConfigOption.DynamicParentValue => ParentValue;
        #endregion

        /// <inheritdoc/>
        public void SetDynamicValue(in dynamic? value) => Value = DynamicValueConverter?.Invoke(value);

        /// <inheritdoc/>
        public void Unset(in bool recursive = false)
        {
            object? oldValue = _Value;
            lock (SyncObject)
            {
                IsSet = false;
                _Value = default;
                IsChanged = true;
            }
            if (oldValue is IOverridableConfig oldConfig) oldConfig.OnChange -= HandleConfigValueChange;
            RaiseOnChange(oldValue);
            Configuration.RaiseOnChangeInt(this, oldValue);
            if (recursive) UnsetOverrides();
        }

        /// <inheritdoc/>
        public void UnsetOverrides() => SubOption?.Unset(recursive: true);

        /// <inheritdoc/>
        public void ResetChanged(in bool recursive = true)
        {
            lock (SyncObject)
            {
                if (_Value is IOverridableConfig config) config.ResetChanged();
                IsChanged = false;
            }
            if (recursive) SubOption?.ResetChanged(recursive);
        }

        /// <inheritdoc/>
        public event IConfigOption.Option_Delegate? OnChange;
        /// <summary>
        /// Raise the <see cref="OnChange"/> event
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="changed">Was this instance changed?</param>
        private void RaiseOnChange(object? oldValue, bool changed = true)
        {
            if (changed)
            {
                InvokeCallbacks();
                RaisePropertyChanged(nameof(Value));
            }
            OnChange?.Invoke(this, oldValue);
            ParentOption?.RaiseOnChange(oldValue, changed: false);
        }

        /// <summary>
        /// Handle a value change of a overridable configuration value
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArguments</param>
        private void HandleConfigValueChange(IOverridableConfig sender, ConfigEventArgs e)
        {
            lock (SyncObject) IsChanged = true;
            InvokeCallbacks();
            RaisePropertyChanged();
            Configuration.RaiseOnChangeInt(this, e.OldValue);
        }

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="option">Option</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator tValue?(ConfigOption<tValue, tConfig> option) => option.FinalValue;
    }
}
