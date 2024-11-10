namespace wan24.Core
{
    /// <summary>
    /// Fallback helper
    /// </summary>
    public static class Fallback
    {
        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="actions">Action and fallback actions</param>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentOutOfRangeException">At last one action is required</exception>
        /// <exception cref="AggregateException">Exceptions of all actions, if no action succeed</exception>
        public static void TryExecute(this Action[] actions, in ExceptionHandler_Delegate? exceptionHandler = null, in CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(actions.Length, other: 1, nameof(actions));
            List<Exception>? exceptions = null;
            for (int i = 0, len = actions.Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                try
                {
                    actions[i]();
                    return;
                }
                catch (Exception ex)
                {
                    HandleException(i, len, ex, ref exceptions, exceptionHandler, cancellationToken);
                }
            throw new InvalidProgramException();
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="actions">Action and fallback actions</param>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        /// <exception cref="ArgumentOutOfRangeException">At last one action is required</exception>
        /// <exception cref="AggregateException">Exceptions of all actions, if no action succeed</exception>
        public static T TryExecute<T>(this Func<T>[] actions, in ExceptionHandler_Delegate? exceptionHandler = null, in CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(actions.Length, other: 1, nameof(actions));
            List<Exception>? exceptions = null;
            for (int i = 0, len = actions.Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                try
                {
                    return actions[i]();
                }
                catch (Exception ex)
                {
                    HandleException(i, len, ex, ref exceptions, exceptionHandler, cancellationToken);
                }
            throw new InvalidProgramException();
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <param name="actions">Action and fallback actions</param>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentOutOfRangeException">At last one action is required</exception>
        /// <exception cref="AggregateException">Exceptions of all actions, if no action succeed</exception>
        public static async Task TryExecuteAsync(
            this Func<CancellationToken, Task>[] actions,
            ExceptionHandler_Delegate? exceptionHandler = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(actions.Length, other: 1, nameof(actions));
            List<Exception>? exceptions = null;
            for (int i = 0, len = actions.Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                try
                {
                    await actions[i](cancellationToken).DynamicContext();
                    return;
                }
                catch (Exception ex)
                {
                    HandleException(i, len, ex, ref exceptions, exceptionHandler, cancellationToken);
                }
            throw new InvalidProgramException();
        }

        /// <summary>
        /// Execute an action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="actions">Action and fallback actions</param>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Return value</returns>
        /// <exception cref="ArgumentOutOfRangeException">At last one action is required</exception>
        /// <exception cref="AggregateException">Exceptions of all actions, if no action succeed</exception>
        public static async Task<T> TryExecuteAsync<T>(
            this Func<CancellationToken, Task<T>>[] actions,
            ExceptionHandler_Delegate? exceptionHandler = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(actions.Length, other: 1, nameof(actions));
            List<Exception>? exceptions = null;
            for (int i = 0, len = actions.Length; i < len && !cancellationToken.GetIsCancellationRequested(); i++)
                try
                {
                    return await actions[i](cancellationToken).DynamicContext();
                }
                catch (Exception ex)
                {
                    HandleException(i, len, ex, ref exceptions, exceptionHandler, cancellationToken);
                }
            throw new InvalidProgramException();
        }

        /// <summary>
        /// Handle an exception
        /// </summary>
        /// <param name="index">Action index</param>
        /// <param name="length">Number of actions</param>
        /// <param name="ex">Exception</param>
        /// <param name="exceptions">All exceptions</param>
        /// <param name="exceptionHandler">Exception handler</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private static void HandleException(
            in int index,
            in int length,
            in Exception ex,
            ref List<Exception>? exceptions,
            in ExceptionHandler_Delegate? exceptionHandler,
            in CancellationToken cancellationToken
            )
        {
            exceptions ??= [];
            exceptions.Add(ex);
            exceptionHandler?.Invoke(index, ex, exceptions, cancellationToken);
            if (index >= length - 1) throw new AggregateException(exceptions);
        }

        /// <summary>
        /// Delegate for an exception handler
        /// </summary>
        /// <param name="index">Action index</param>
        /// <param name="ex">Exception</param>
        /// <param name="exceptions">All exceptions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate void ExceptionHandler_Delegate(int index, Exception ex, IReadOnlyList<Exception> exceptions, CancellationToken cancellationToken);
    }
}
