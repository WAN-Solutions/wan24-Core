# Bootstrapping

The `wan24-Core` bootstrapper allows asynchronous app and module initialization:

```cs
await Bootstrap.Async();
```

The bootstrapper uses `TypeHelper.Instance` for scanning assemblies for defined bootstrappers:

```cs
[assembly: Bootstrapper(typeof(Your.Namespace.Bootstrapper), nameof(Your.Namespace.Bootstrapper.BootAsync))]

namespace Your.Namespace
{
    public static class Bootstrapper
    {
        public static async Task BootAsync()// This method may be synchronous also
        {
			// Perform your assembly initialization here
        }
    }
}
```

Using the `Bootstrapper` attribute you can define how and when to bootstrap your assembly. It's possible to define priorities, for example. To speed up bootstrapping, you may configure the `Bootstrap` static properties before running the bootstrapper: Here you can specify if to scan assemblies, types and methods for `Bootstrapper` attributes. The less to scan, the faster the bootstrapper runs. Assemblies which are being lazy loaded during runtime will be scanned and bootstrapped automatic.

**CAUTION**: At present in some environments the assembly list seems not to be determinable at bootstrapping time. When this happens, bootstrappers won't run, and you'll have to call their boot-methods manual from code. This is a bug which needs a fix in upcoming `wan24-Core` versions.
