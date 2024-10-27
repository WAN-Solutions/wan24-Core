# Enumerables

`wan24-Core` can speed up enumerables, if you're going to enumerate an array, an immutable array, or any `IList<T>` type. But there's a difference to usual .NET enumerables, which you should know in advance: .NET enumerations runs the whole processing workflow whenever an enumerator was created, which can produce different results during each enumeration. `wan24-Core` assumes that the enumeration source won't change, and processing may be executed partial in advance (before enumerating). This could lead to misunderstandings - for example:

```cs
int[] data = [1, 2, 3, 4, 5];

var enumerable1 = data.Where(i => i > 2).Take(10);// wan24-Core
var enumerable2 = data.AsEnumerable().Where(i => i > 2).Take(10);// .NET

data[0] = 6;

Assert.AreEqual(3, enumerable1.First());// wan24-Core
Assert.AreEqual(6, enumerable2.First());// .NET
```

As you can see, the `wan24-Core` enumerable was processed partial before the source was changed, which results in an incompatibility to the .NET enumerable. To be sure that you work with .NET enumerables, you should call `AsEnumerable` on the source, first. As soon as you called any .NET enumerable extension method on a `wan24-Core` enumerable, the possibly pre-processed enumeration will be processed from .NET as you know it.

To ensure that an enumeration like `...Where(...).Select(...)` has been fully processed, you can call the `Process` method for creating a new array of the result at that time and work with a new `ArrayEnumerable<T>` instance.

The implementation of enumerables by `wan24-Core` can speed up enumerations, when compared to .NET processing times, but you'll need to evaluate, if partial pre-processing of the enumeration does fit your business logic for each case to avoid a surprise for special cases. In most cases `wan24-Core` enumerables will work as expected from your code.

**TIP**: If the code can't see that you want to use `wan24-Core` enumerables instead of the .NET LINQ extension methods, you can place the `using wan24.Core;` statement within the namespace:

```cs
//using wan24.Core;// <-- Remove this line from here

namespace Your.NameSpace
{
	using wan24.Core;// <-- Place the statement here instead
	...
}
```

This should solve the problem, if you don't want to use .NET LINQ, too. To do so, you'll need to separate code into partial classes / code files.

## `ICoreEnumerable<T>`

The `ICoreEnumerable<T>` interface does implement `IEnumerable<T>` and is being used for any `wan24-Core` enumerable type in case you need a common interface which avoids having to fall back to .NET enumerables - example: A method which does return a `wan24-Core` enumerable may return an `ArrayEnumerable<T>` or an `ArrayWhereEnumerable<T>` and uses `ICoreEnumerable<T>` as return type, which is a common interface for the two possible return types.
