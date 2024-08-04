using System.Collections;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a disposable type
    /// </summary>
    /// <typeparam name="T">Final type</typeparam>
    public abstract class DisposableBase<T> : DisposableBase where T : DisposableBase<T>
    {
        /// <summary>
        /// Disposable fields
        /// </summary>
        protected static readonly FieldInfoExt[] DisposableFields;
        /// <summary>
        /// Disposable properties
        /// </summary>
        protected static readonly Func<T, IDisposable?>[] SyncDisposableProperties;
        /// <summary>
        /// Disposable properties
        /// </summary>
        protected static readonly Func<T, IAsyncDisposable?>[] AsyncDisposableProperties;
        /// <summary>
        /// Disposable properties
        /// </summary>
        protected static readonly Func<T, object?>[] HybridDisposableProperties;
        /// <summary>
        /// Other disposable properties
        /// </summary>
        protected static readonly Func<T, object?>[] OtherDisposableProperties;

        /// <summary>
        /// Static constructor
        /// </summary>
        static DisposableBase()
        {
            DisposableFields = (from fi in typeof(T).GetFieldsCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                where fi.GetCustomAttributeCached<DisposeAttribute>() is not null && 
                                    fi.Getter is not null
                                select fi).ToArray();
            SyncDisposableProperties = (from pi in typeof(T).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        where pi.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                            pi.Getter is not null &&
                                            typeof(IDisposable).IsAssignableFrom(pi.PropertyType) &&
                                            !typeof(IAsyncDisposable).IsAssignableFrom(pi.PropertyType)
                                        select pi.Property.CreateTypedInstancePropertyGetter<T, IDisposable>()).ToArray();
            AsyncDisposableProperties = (from pi in typeof(T).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         where pi.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                             pi.Getter is not null &&
                                             typeof(IAsyncDisposable).IsAssignableFrom(pi.PropertyType) &&
                                             !typeof(IDisposable).IsAssignableFrom(pi.PropertyType)
                                         select pi.Property.CreateTypedInstancePropertyGetter<T, IAsyncDisposable>()).ToArray();
            HybridDisposableProperties = (from pi in typeof(T).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                          where pi.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                              pi.Getter is not null &&
                                              typeof(IAsyncDisposable).IsAssignableFrom(pi.PropertyType) &&
                                              typeof(IDisposable).IsAssignableFrom(pi.PropertyType)
                                          select pi.Property.CreateTypedInstancePropertyGetter<T, object>()).ToArray();
            OtherDisposableProperties = (from pi in typeof(T).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         where pi.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                             pi.Getter is not null &&
                                             !typeof(IAsyncDisposable).IsAssignableFrom(pi.PropertyType) &&
                                             !typeof(IDisposable).IsAssignableFrom(pi.PropertyType)
                                         select pi.Property.CreateTypedInstancePropertyGetter<T, object>()).ToArray();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected DisposableBase() : base() { }

        /// <inheritdoc/>
        protected override void DisposeAttributes()
        {
            Queue<IEnumerable> enumerables = new();
            foreach (FieldInfoExt fi in DisposableFields)
                switch (fi.Getter!(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) dObj.Dispose();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        break;
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            T self = (T)this;
            foreach (Func<T, object?> getter in SyncDisposableProperties.Concat(HybridDisposableProperties))
                switch (getter(self))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) dObj.Dispose();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            foreach (Func<T, IAsyncDisposable?> getter in AsyncDisposableProperties)
                if (getter(self) is IAsyncDisposable asyncDisposable)
                    asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
            foreach (Func<T, object?> getter in OtherDisposableProperties)
                switch (getter(self))
                {
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            while(enumerables.TryDequeue(out IEnumerable? enumerable))
                foreach(object? item in enumerable)
                    switch (item)
                    {
                        case IDisposableObject dObj:
                            if (!dObj.IsDisposing) dObj.Dispose();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                        case IAsyncDisposable asyncDisposable:
                            asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                            break;
                        case byte[] bytes:
                            bytes.Clear();
                            break;
                        case char[] characters:
                            characters.Clear();
                            break;
                        case IEnumerable e:
                            enumerables.Enqueue(e);
                            break;
                    }
        }

        /// <inheritdoc/>
        protected override async Task DisposeAttributesAsync()
        {
            Queue<IEnumerable> enumerables = new();
            foreach (FieldInfoExt fi in DisposableFields)
                switch (fi.Getter!(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) await dObj.DisposeAsync().DynamicContext();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().DynamicContext();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            T self = (T)this;
            foreach (Func<T, object?> getter in AsyncDisposableProperties.Concat(HybridDisposableProperties))
                switch (getter(self))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) await dObj.DisposeAsync().DynamicContext();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().DynamicContext();
                        break;
                }
            foreach (Func<T, IDisposable?> getter in SyncDisposableProperties)
                if (getter(self) is IDisposable disposable)
                    disposable.Dispose();
            foreach (Func<T, object?> getter in OtherDisposableProperties)
                switch (getter(self))
                {
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                }
            while (enumerables.TryDequeue(out IEnumerable? enumerable))
                foreach (object? item in enumerable)
                    switch (item)
                    {
                        case IDisposableObject dObj:
                            if (!dObj.IsDisposing) await dObj.DisposeAsync().DynamicContext();
                            break;
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync().DynamicContext();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                        case byte[] bytes:
                            bytes.Clear();
                            break;
                        case char[] characters:
                            characters.Clear();
                            break;
                        case IEnumerable e:
                            enumerables.Enqueue(e);
                            break;
                    }
        }
    }
}
