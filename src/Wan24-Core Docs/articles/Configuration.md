# Configuration

## Using a JSON file

If you want to make your app configurable using a JSON file, you can use the `AppConfig(Base|Loader)` for an automatted configuration:

```cs
// The main configuration structure
public class YourAppConfig() : AppConfig()
{
	[AppConfig]
	public YourConfigGroup? YourConfig { get; set; }
}

// An optional sub-configuration structure
public class YourConfigGroup() : AppConfigBase()
{
	public string? AnySetting { get; set; }

	public override void Apply()
	{
		if(AnySetting is not null) YourType.AnySetting = AnySetting;
		base.Apply();
	}

	public override async Task ApplyAsync(CancellationToken cancellationToken = default)
	{
		if(AnySetting is not null) YourType.AnySetting = AnySetting;
		await base.ApplyAsync(cancellationToken);
	}
}

// Load and apply the configuration manual
YourAppConfig? config = await YourAppConfig.LoadIfExistsAsync<YourAppConfig>("/path/to/file.json");

// Using AppConfiogLoader for automatic configuration reload (and application) on change
using AppConfigLoader<YourAppConfig> configLoader = new("/path/to/file.json");
await configLoader.StartAsync();
```

By extending `AppConfig` for your `YourAppConfig` you implement the basic `wan24-Core` configuration and extend it by a sub-configuration `YourConfig`, which is defined in the `YourConfigGroup` type. The attribute `AppConfig` signals the `AppConfig.Apply(Async)` method to apply the configuration from that property, if it was defined in the JSON file, which could look like this:

```json
{
	"YourConfig":
	{
		"YourSetting": "Value"
	}
}
```

`YourConfigGroup` extends `AppConfigBase` and overrides the `Apply(Async)` method which does place the configured value of `YourSetting` to its target, if there's any value given. Of course this step is optional: After `YourAppConfig.Apply(Async)` was called, you can find the applied configuration instance in the static `YourAppConfig.Applied` property and access the `YourSetting` value from your code, instead of copying the value to another place.

Using the `LoadIfExistsAsync` method the JSON configuration file will be loaded, parsed and applied, if it exists. The `AppConfigLoader` is an `IHostedService`, which does the same, but also watches the configuration file for changes and performs an automatic reload (including a call to `Apply(Async)`) on any modification automatic. By calling `StartAsync` the background service will be started and watching the file until `StopAsync` was called, or the `configLoader` instance is being disposed.

Your customized app configuration may define sub-configurations as required. There's no depth limit at present.

## Using CLI arguments

Use the `CliConfig` attribute for static properties which allow their values to be defined from CLI arguments. For example the `Settings.CustomTempFolder` can be configured from CLI like this:

```bash
dotnet yourApp.dll --wan24.Core.Settings.CustomTempFolder /path/to/folder
```

Basic value types can be written in standard syntax:

| Type | Syntax |
| ---- | ------ |
| `bool` | `-Your.Namespace.YourType.PropertyName` to set the value to `true` |
| `string` | `--Your.Namespace.YourType.PropertyName [value]` |
| `string[]` | `--Your.Namespace.YourType.PropertyName [value1] [value2] ...` |

Other value types can be written in JSON syntax - for example:

```bash
--Your.Namespace.YourType.PropertyName "{\"Property\":false}"
```

To apply CLI arguments configuration values in your app:

```cs
CliConfig.Apply();
```

The `CliConfig` type exports some CLI configuration options as static properties, which had no other place. You can find properties with the `CliConfig` attribute in many places. Search for `[CliConfig]` in the `wan24-Core` source code to find them all.
