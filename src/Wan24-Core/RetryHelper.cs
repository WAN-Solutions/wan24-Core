namespace wan24.Core
{
    /// <summary>
    /// Retry helper
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Try to execute an action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="action">Action to execute</param>
        /// <param name="maxNumberOfTries">Max. number of tries</param>
        /// <param name="timeout">Total timeout</param>
        /// <param name="delay">Delay between retries</param>
        /// <param name="retryOnError">Delegate to determine if to retry after an error</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Retry information, which include the result</returns>
        public static RetryInfo<T> TryAction<T>(
            in Try_Delegate<T> action,
            in int maxNumberOfTries,
            in TimeSpan? timeout = null,
            in TimeSpan? delay = null,
            in RetryOnError_Delegate? retryOnError = null,
            in CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxNumberOfTries, 1);
            RetryInfo<T> res = new();
            DateTime started = res.Started;
            TimeSpan to = timeout ?? TimeSpan.Zero;
            for (int i = 1; i <= maxNumberOfTries; i++)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (timeout.HasValue && !TimeSpanHelper.UpdateTimeout(ref started, ref to))
                    {
                        TimeoutException ex = new();
                        ex.Data[timeout.Value] = true;
                        res.Exceptions.Add(ex);
                        Finalize(res, i);
                        break;
                    }
                    res.Result = action(i, cancellationToken);
                    Finalize(res, i);
                    res.Succeed = true;
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    res.Exceptions.Add(ex);
                    if (ex.CancellationToken.IsEqualTo(cancellationToken) || !HandleException(ex, maxNumberOfTries, i, retryOnError, delay))
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (TimeoutException ex)
                {
                    res.Exceptions.Add(ex);
                    if ((timeout.HasValue && ex.Data.Contains(timeout.Value)) || !HandleException(ex, maxNumberOfTries, i, retryOnError, delay))
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    res.Exceptions.Add(ex);
                    if (HandleException(ex, maxNumberOfTries, i, retryOnError, delay)) continue;
                    Finalize(res, i);
                    break;
                }
            if (res.NumberOfTries == 0) Finalize(res, maxNumberOfTries);
            return res;
        }

        /// <summary>
        /// Try to execute an action
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="maxNumberOfTries">Max. number of retries</param>
        /// <param name="timeout">Total timeout</param>
        /// <param name="delay">Delay between retries</param>
        /// <param name="retryOnError">Delegate to determine if to retry after an error</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Retry information, which include the result</returns>
        public static RetryInfo<object> TryAction(
            in Try_Delegate action,
            in int maxNumberOfTries,
            in TimeSpan? timeout = null,
            in TimeSpan? delay = null,
            in RetryOnError_Delegate? retryOnError = null,
            in CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxNumberOfTries, 1);
            RetryInfo<object> res = new();
            DateTime started = res.Started;
            TimeSpan to = timeout ?? TimeSpan.Zero;
            for (int i = 1; i <= maxNumberOfTries; i++)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (timeout.HasValue && !TimeSpanHelper.UpdateTimeout(ref started, ref to))
                    {
                        TimeoutException ex = new();
                        ex.Data[timeout.Value] = true;
                        res.Exceptions.Add(ex);
                        Finalize(res, i);
                        break;
                    }
                    action(i, cancellationToken);
                    Finalize(res, i);
                    res.Succeed = true;
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    res.Exceptions.Add(ex);
                    if (ex.CancellationToken.IsEqualTo(cancellationToken) || !HandleException(ex, maxNumberOfTries, i, retryOnError, delay))
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (TimeoutException ex)
                {
                    res.Exceptions.Add(ex);
                    if ((timeout.HasValue && ex.Data.Contains(timeout.Value)) || !HandleException(ex, maxNumberOfTries, i, retryOnError, delay))
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    res.Exceptions.Add(ex);
                    if (HandleException(ex, maxNumberOfTries, i, retryOnError, delay)) continue;
                    Finalize(res, i);
                    break;
                }
            if (res.NumberOfTries == 0) Finalize(res, maxNumberOfTries);
            return res;
        }

        /// <summary>
        /// Try to execute an action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="action">Action to execute</param>
        /// <param name="maxNumberOfTries">Max. number of tries</param>
        /// <param name="timeout">Total timeout</param>
        /// <param name="delay">Delay between retries</param>
        /// <param name="retryOnError">Delegate to determine if to retry after an error</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Retry information, which include the result</returns>
        public static async Task<RetryInfo<T>> TryActionAsync<T>(
            TryAsync_Delegate<T> action, 
            int maxNumberOfTries, 
            TimeSpan? timeout = null, 
            TimeSpan? delay = null,
            RetryOnError_Delegate? retryOnError = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxNumberOfTries, 1);
            RetryInfo<T> res = new();
            DateTime started = res.Started;
            TimeSpan to = timeout ?? TimeSpan.Zero;
            for (int i = 1; i <= maxNumberOfTries; i++)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (timeout.HasValue && !TimeSpanHelper.UpdateTimeout(ref started, ref to))
                    {
                        TimeoutException ex = new();
                        ex.Data[timeout.Value] = true;
                        res.Exceptions.Add(ex);
                        Finalize(res, i);
                        break;
                    }
                    res.Result = timeout.HasValue
                        ? await action(i, cancellationToken).WaitAsync(to, cancellationToken).DynamicContext()
                        : await action(i, cancellationToken).WaitAsync(cancellationToken).DynamicContext();
                    Finalize(res, i);
                    res.Succeed = true;
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    res.Exceptions.Add(ex);
                    if (ex.CancellationToken.IsEqualTo(cancellationToken) || !await HandleExceptionAsync(ex, maxNumberOfTries, i, retryOnError, delay).DynamicContext())
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (TimeoutException ex)
                {
                    res.Exceptions.Add(ex);
                    if ((timeout.HasValue && ex.Data.Contains(timeout.Value)) || !await HandleExceptionAsync(ex, maxNumberOfTries, i, retryOnError, delay).DynamicContext())
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    res.Exceptions.Add(ex);
                    if (await HandleExceptionAsync(ex, maxNumberOfTries, i, retryOnError, delay).DynamicContext()) continue;
                    Finalize(res, i);
                    break;
                }
            if (res.NumberOfTries == 0) Finalize(res, maxNumberOfTries);
            return res;
        }

        /// <summary>
        /// Try to execute an action
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="maxNumberOfTries">Max. number of retries</param>
        /// <param name="timeout">Total timeout</param>
        /// <param name="delay">Delay between retries</param>
        /// <param name="retryOnError">Delegate to determine if to retry after an error</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Retry information, which include the result</returns>
        public static async Task<RetryInfo<object>> TryActionAsync(
            TryAsync_Delegate action,
            int maxNumberOfTries,
            TimeSpan? timeout = null,
            TimeSpan? delay = null,
            RetryOnError_Delegate? retryOnError = null,
            CancellationToken cancellationToken = default
            )
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxNumberOfTries, 1);
            RetryInfo<object> res = new();
            DateTime started = res.Started;
            TimeSpan to = timeout ?? TimeSpan.Zero;
            for (int i = 1; i <= maxNumberOfTries; i++)
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (timeout.HasValue)
                    {
                        if (!TimeSpanHelper.UpdateTimeout(ref started, ref to))
                        {
                            TimeoutException ex = new();
                            ex.Data[timeout.Value] = true;
                            res.Exceptions.Add(ex);
                            Finalize(res, i);
                            break;
                        }
                        await action(i, cancellationToken).WaitAsync(to, cancellationToken).DynamicContext();
                    }
                    else
                    {
                        await action(i, cancellationToken).WaitAsync(cancellationToken).DynamicContext();
                    }
                    Finalize(res, i);
                    res.Succeed = true;
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    res.Exceptions.Add(ex);
                    if (ex.CancellationToken.IsEqualTo(cancellationToken) || !await HandleExceptionAsync(ex, maxNumberOfTries, i, retryOnError, delay).DynamicContext())
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (TimeoutException ex)
                {
                    res.Exceptions.Add(ex);
                    if (timeout.HasValue || !await HandleExceptionAsync(ex, maxNumberOfTries, i, retryOnError, delay).DynamicContext())
                    {
                        Finalize(res, i);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    res.Exceptions.Add(ex);
                    if (await HandleExceptionAsync(ex, maxNumberOfTries, i, retryOnError, delay).DynamicContext()) continue;
                    Finalize(res, i);
                    break;
                }
            if (res.NumberOfTries == 0) Finalize(res, maxNumberOfTries);
            return res;
        }

        /// <summary>
        /// Handle an exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="maxNumberOfTries">Max. number of retries</param>
        /// <param name="currentTry">Current try</param>
        /// <param name="retryOnError">Delegate to determine if to retry after an error</param>
        /// <param name="delay">Delay between retries</param>
        /// <returns>Continue?</returns>
        private static bool HandleException(
            in Exception exception,
            in int maxNumberOfTries,
            in int currentTry,
            in RetryOnError_Delegate? retryOnError,
            in TimeSpan? delay
            )
        {
            TimeSpan? useDelay = delay;
            if (currentTry == maxNumberOfTries || !(retryOnError?.Invoke(exception, delay, ref useDelay) ?? true)) return false;
            if (useDelay.HasValue) Thread.Sleep((int)useDelay.Value.TotalMilliseconds);
            return true;
        }

        /// <summary>
        /// Handle an exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="maxNumberOfTries">Max. number of retries</param>
        /// <param name="currentTry">Current try</param>
        /// <param name="retryOnError">Delegate to determine if to retry after an error</param>
        /// <param name="delay">Delay between retries</param>
        /// <returns>Continue?</returns>
        private static async Task<bool> HandleExceptionAsync(
            Exception exception,
            int maxNumberOfTries, 
            int currentTry,
            RetryOnError_Delegate? retryOnError, 
            TimeSpan? delay
            )
        {
            TimeSpan? useDelay = delay;
            if (currentTry == maxNumberOfTries || !(retryOnError?.Invoke(exception, delay, ref useDelay) ?? true)) return false;
            if (useDelay.HasValue) await Task.Delay(useDelay.Value).DynamicContext();
            return true;
        }

        /// <summary>
        /// Finalize tries
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="info">Retry information</param>
        /// <param name="currentTry">Current try number</param>
        private static void Finalize<T>(in RetryInfo<T> info, in int currentTry)
        {
            info.Done = DateTime.Now;
            info.NumberOfTries = currentTry;
        }

        /// <summary>
        /// Delegate for a try action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="currentTry">Current try number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public delegate T Try_Delegate<T>(int currentTry, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a try action
        /// </summary>
        /// <param name="currentTry">Current try number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate void Try_Delegate(int currentTry, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a try action
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <param name="currentTry">Current try number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public delegate Task<T> TryAsync_Delegate<T>(int currentTry, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a try action
        /// </summary>
        /// <param name="currentTry">Current try number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task TryAsync_Delegate(int currentTry, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate to decide if to continue with the next try after an error (is being called before the delay)
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="givenDelay">Given delay</param>
        /// <param name="delayToUse">Delay to use</param>
        /// <returns>Continue with the next try?</returns>
        public delegate bool RetryOnError_Delegate(Exception exception, TimeSpan? givenDelay, ref TimeSpan? delayToUse);
    }
}
