# Object observing

These base types can notify your code about changes using events and callbacks:

| Type | Implementations | Description |
| ---- | --------------- | ----------- |
| `ChangeToken` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging` | Should be used as a base class |
| `ChangeToken<T>` | `ChangeToken`, `IObservable<T>` | Change token + observable base class |
| `DisposableChangeToken` | `ChangeToken`, `IDisposable` | Disposable change token base class (disposes callback registrations when disposing) |
| `DisposableChangeToken<T>` | `ChangeToken`, `IObservable<T>`, `IDisposable` | Disposable change token + observable base class (disposes callback registrations when disposing) |

Example:

```cs
public class YourType(): DisposableChangeToken()
{
	private string? _StringValue = null;

	public string StringValue
	{
		get => _StringValue;
		set => SetNewPropertyValue(ref _StringValue, value, nameof(StringValue));
	}
}
```

Observers will be notified, if a new value was set to the `StringValue` property. Using the `InvokeCallbacks` method your code can invoke registered callbacks at any time, too. You may also give a custom change identifier to the constructor for implementing more advanced object observings.

Use these base types if you want more control over your objects, which code generators for observables won't give you.
