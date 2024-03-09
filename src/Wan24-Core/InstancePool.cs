using System.Runtime.CompilerServices;
using System.Threading.Channels;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Instance pool
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public class InstancePool<T> : HostedServiceBase, IInstancePool<T> where T : class
    {
        /// <summary>
        /// Synchronous instance factory
        /// </summary>
        protected readonly IInstancePool<T>.Instance_Delegate? SyncFactory;
        /// <summary>
        /// Asynchronous instance factory
        /// </summary>
        protected readonly IInstancePool<T>.InstanceAsync_Delegate? AsyncFactory;
        /// <summary>
        /// Instances
        /// </summary>
        protected readonly Channel<T> Instances;
        /// <summary>
        /// Buffer thread synchronization (raised when creating instances)
        /// </summary>
        protected readonly ResetEvent BufferSync = new(initialState: true, enableSyncWaiting: false);
        /// <summary>
        /// Total number of created instances
        /// </summary>
        protected volatile int _Created = 0;
        /// <summary>
        /// Total number of on-demand created instances
        /// </summary>
        protected volatile int _CreatedOnDemand = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="serviceProvider">Service provider</param>
        public InstancePool(in int capacity, in IAsyncServiceProvider? serviceProvider = null) : this(capacity, (pool, ct) => null!, serviceProvider)
            => AsyncFactory = async (pool, ct) => (T)(await typeof(T).ConstructAutoAsync(ServiceProvider).DynamicContext()).Object;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        /// <param name="serviceProvider">Service provider</param>
        public InstancePool(in int capacity, in IInstancePool<T>.Instance_Delegate factory, in IAsyncServiceProvider? serviceProvider = null) : this(intern: true, capacity, serviceProvider)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
            SyncFactory = factory;
            AsyncFactory = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        /// <param name="serviceProvider">Service provider</param>
        public InstancePool(in int capacity, in IInstancePool<T>.InstanceAsync_Delegate factory, in IAsyncServiceProvider? serviceProvider = null) : this(intern: true, capacity, serviceProvider)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
            SyncFactory = null;
            AsyncFactory = factory;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="intern">Intern construction</param>
        /// <param name="serviceProvider">Service provider</param>
#pragma warning disable IDE0060 // Remove unused parameter
        protected InstancePool(in bool intern, in int capacity, in IAsyncServiceProvider? serviceProvider) : base()
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Capacity = capacity;
            Instances = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            ServiceProvider = serviceProvider ?? DiHelper.Instance;
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Service provider
        /// </summary>
        public IAsyncServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public int Capacity { get; }

        /// <inheritdoc/>
        public int Available => Instances.Reader.Count;

        /// <inheritdoc/>
        public int Created => _Created;

        /// <inheritdoc/>
        public int CreatedOnDemand => _CreatedOnDemand;

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("GUID"), GUID, __("Unique ID of the service object"));
                yield return new(__("Last exception"), LastException?.Message, __("Last exception message"));
                yield return new(__("Capacity"), Capacity, __("Number of hold instances"));
                yield return new(__("Available"), Available, __("Number of available instances"));
                yield return new(__("Created"), Created, __("Total number of created instances"));
                yield return new(__("On-demand"), CreatedOnDemand, __("Total number of on-demand created instances"));
            }
        }

        /// <inheritdoc/>
        public virtual T GetOne()
        {
            EnsureUndisposed();
            if (Instances.Reader.TryRead(out T? res)) return res;
            res = SyncFactory?.Invoke(this) ?? AsyncFactory!(this, default).Result;
            _Created++;
            _CreatedOnDemand++;
            return res;
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetOneAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Instances.Reader.TryRead(out T? res)) return res;
            if (AsyncFactory is not null)
            {
                res = await AsyncFactory(this, cancellationToken).DynamicContext();
            }
            else
            {
                res = SyncFactory!(this);
            }
            _Created++;
            _CreatedOnDemand++;
            return res;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> GetMany(int count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            for (int i = 0; i < count && !cancellationToken.IsCancellationRequested; i++) yield return GetOne();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<T> GetManyAsync(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            for (int i = 0; i < count && !cancellationToken.IsCancellationRequested; i++) yield return await GetOneAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        object IInstancePool.GetOneObject() => GetOne();

        /// <inheritdoc/>
        async Task<object> IInstancePool.GetOneObjectAsync(CancellationToken cancellationToken) => await GetOneAsync(cancellationToken).DynamicContext();

        /// <inheritdoc/>
        IEnumerable<object> IInstancePool.GetManyObjects(int count, CancellationToken cancellationToken) => GetMany(count, cancellationToken);

        /// <inheritdoc/>
        IAsyncEnumerable<object> IInstancePool.GetManyObjectsAsync(int count, CancellationToken cancellationToken) => GetManyAsync(count, cancellationToken);

        /// <inheritdoc/>
        protected override async Task BeforeStopAsync(CancellationToken cancellationToken)
        {
            await BufferSync.SetAsync(cancellationToken).DynamicContext();
            await base.BeforeStopAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            await BufferSync.WaitAsync(CancelToken).DynamicContext();
            if (AsyncFactory is not null)
            {
                for (; EnsureNotCanceled(); _Created++, await BufferSync.WaitAsync(CancelToken).DynamicContext())
                    await Instances.Writer.WriteAsync(await AsyncFactory(this, CancelToken).DynamicContext(), CancelToken).DynamicContext();
            }
            else
            {
                for (; EnsureNotCanceled(); _Created++, await BufferSync.WaitAsync(CancelToken).DynamicContext())
                    await Instances.Writer.WriteAsync(SyncFactory!(this), CancelToken).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            while (Instances.Reader.TryRead(out T? instance)) instance.TryDispose();
            BufferSync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            while (Instances.Reader.TryRead(out T? instance)) await instance.TryDisposeAsync().DynamicContext();
            await BufferSync.DisposeAsync().DynamicContext();
        }
    }
}
