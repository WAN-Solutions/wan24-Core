﻿using System.Collections.Immutable;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// <see cref="ImmutableArray{T}"/> enumerator
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ImmutableArrayEnumerator<T> : EnumeratorBase<T>
    {
        /// <summary>
        /// Array
        /// </summary>
        protected readonly ImmutableArray<T> Array;
        /// <summary>
        /// Start state
        /// </summary>
        protected readonly int StartState;
        /// <summary>
        /// End state
        /// </summary>
        protected readonly int EndState;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ImmutableArrayEnumerator(in ImmutableArray<T> arr, in int offset = 0, in int? count = null) : base()
        {
            Array = arr;
            ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, arr.Length, nameof(offset));
            StartState = State = offset - 1;
            EndState = offset + (count ?? arr.Length);
            if (EndState > arr.Length)
            {
                if (count.HasValue) throw new ArgumentOutOfRangeException(nameof(count));
                EndState = arr.Length;
            }
        }

        /// <inheritdoc/>
        public override bool MoveNext()
        {
            EnsureUndisposed();
            if (++State < EndState)
            {
                _Current = Array[State];
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            State = StartState;
        }
    }
}
