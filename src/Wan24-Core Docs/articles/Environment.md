# Environment

## Settings and runtime information

You should have a look at the `Settings` properties, which allow a basic `wan24-Core` app environment setup. The `ENV` properties provide some basic runtime environment information.

## Runtime information and statistics

The `EnvironmentService.State` property provides general runtime information of your app and collects statistical data in an interval, if being started as hosted service. This information, and much more of any `IStatusProvider` type, which was registered to `StatusProviderTable`, is also available using the `StatusProviderTable.State` property.

## Instance tables

Some types register instances to `InstanceTables`, and unregister them, when they're being disposed. This helps with investigating your app at runtime. The instance table for a type needs to export a static readonly-field with a concurrent dictionary which stores object instances. The field must have the `InstanceTable` attribute. The dictionary must use a string key (may be a GUID, for example), and the object instance as value.
