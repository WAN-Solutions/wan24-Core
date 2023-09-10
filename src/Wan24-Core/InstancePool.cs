﻿using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

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
        public InstancePool(int capacity) : this(capacity, async (pool, ct) => (T)(await typeof(T).ConstructAutoAsync(DiHelper.Instance).DynamicContext()).Object) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        public InstancePool(int capacity, IInstancePool<T>.Instance_Delegate factory) : this(capacity, intern: true)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            SyncFactory = factory;
            AsyncFactory = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="factory">Instance factory</param>
        public InstancePool(int capacity, IInstancePool<T>.InstanceAsync_Delegate factory) : this(capacity, intern: true)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            SyncFactory = null;
            AsyncFactory = factory;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Capacity</param>
        /// <param name="intern">Intern construction</param>
#pragma warning disable IDE0060 // Remove unused parameter
        protected InstancePool(int capacity, bool intern) : base()
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Capacity = capacity;
            Instances = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

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
                yield return new("GUID", GUID, "Unique ID of the service object");
                yield return new("Last exception", LastException?.Message, "Last exception message");
                yield return new("Capacity", Capacity, "Number of hold instances");
                yield return new("Available", Available, "Number of available instances");
                yield return new("Created", Created, "Total number of created instances");
                yield return new("On-demand", CreatedOnDemand, "Total number of on-demand created instances");
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
                Contract.Assert(SyncFactory is not null);
                res = SyncFactory(this);
            }
            _Created++;
            _CreatedOnDemand++;
            return res;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> GetMany(int count, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            for (int i = 0; i != count && !cancellationToken.IsCancellationRequested; i++) yield return GetOne();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<T> GetManyAsync(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            for (int i = 0; i != count && !cancellationToken.IsCancellationRequested; i++) yield return await GetOneAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
        {
            if (SyncFactory is not null)
            {
                for (; !Cancellation!.IsCancellationRequested; _Created++) await Instances.Writer.WriteAsync(SyncFactory(this), Cancellation.Token).DynamicContext();
            }
            else
            {
                Contract.Assert(AsyncFactory is not null);
                for (; !Cancellation!.IsCancellationRequested; _Created++)
                    await Instances.Writer.WriteAsync(await AsyncFactory(this, Cancellation.Token).DynamicContext(), Cancellation.Token).DynamicContext();
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            while (Instances.Reader.TryRead(out T? instance)) instance.TryDispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            while (Instances.Reader.TryRead(out T? instance)) await instance.TryDisposeAsync().DynamicContext();
        }
    }
}