using Microsoft.Extensions.Primitives;
using System.ComponentModel;

namespace wan24.Core
{
    // IObservable<ConcurrentChangeTokenDictionary<string, InMemoryCacheEntry<T>>>, IChangeToken and INotifyPropertyChanged implementation
    public partial class InMemoryCache<T> : IObservable<ConcurrentChangeTokenDictionary<string, InMemoryCacheEntry<T>>>, IChangeToken, INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public bool HasChanged => IfUndisposed(() => Cache.HasChanged);

        /// <inheritdoc/>
        public bool ActiveChangeCallbacks => IfUndisposed(() => Cache.ActiveChangeCallbacks);

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged
        {
            add => IfUndisposed(() => Cache.PropertyChanged += value);
            remove => IfUndisposed(() => Cache.PropertyChanged -= value);
        }

        /// <inheritdoc/>
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            EnsureUndisposed();
            return Cache.RegisterChangeCallback(callback, state);
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<ConcurrentChangeTokenDictionary<string, InMemoryCacheEntry<T>>> observer)
        {
            EnsureUndisposed();
            return Cache.Subscribe(observer);
        }
    }
}
