﻿using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Implementation of IAsyncToken.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class AsyncToken<T> : IAsyncToken<T>
    {
        /// <summary>
        /// Immutable resolution of the token.
        /// </summary>
        private class Resolution
        {
            /// <summary>
            /// True iff resolution was successful.
            /// </summary>
            public readonly bool Success;

            /// <summary>
            /// The value of a success.
            /// </summary>
            public readonly T Result;

            /// <summary>
            /// The value of a failure.
            /// </summary>
            public readonly Exception Exception;

            /// <summary>
            /// Creates a new failure resolution.
            /// </summary>
            /// <param name="exception"></param>
            public Resolution(Exception exception)
            {
                Exception = exception;
                Success = false;
            }

            /// <summary>
            /// Creates a new successful resolution.
            /// </summary>
            /// <param name="result"></param>
            public Resolution(T result)
            {
                Result = result;
                Success = true;
            }
        }

        /// <summary>
        /// Set to true if aborted.
        /// </summary>
        private bool _aborted = false;

        /// <summary>
        /// Set to true if resolved.
        /// </summary>
        private bool _resolved = false;

        /// <summary>
        /// Set when resolved with Succeed or Fail.
        /// </summary>
        private Resolution _resolution;

        /// <summary>
        /// Lists of callbacks.
        /// </summary>
        private readonly List<Action<T>> _onSuccessCallbacks = new List<Action<T>>();
        private readonly List<Action<Exception>> _onFailureCallbacks = new List<Action<Exception>>();
        private readonly List<Action<IAsyncToken<T>>> _onFinallyCallbacks = new List<Action<IAsyncToken<T>>>();
        private readonly List<ExceptionDispatchInfo> _scratchExceptions = new List<ExceptionDispatchInfo>();

        /// <summary>
        /// Creates a token with no resolution.
        /// </summary>
        public AsyncToken()
        {
            
        }

        /// <summary>
        /// Creates a successful token.
        /// </summary>
        /// <param name="value">The value to resolve with.</param>
        public AsyncToken(T value)
        {
            Succeed(value);
        }

        /// <summary>
        /// Creates a failed token.
        /// </summary>
        /// <param name="exception">The exception to fail with.</param>
        public AsyncToken(Exception exception)
        {
            Fail(exception);
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IAsyncToken<T> OnSuccess(Action<T> callback)
        {
            if (_aborted)
            {
                return this;
            }

            if (_resolved)
            {
                if (_resolution.Success)
                {
                    callback(_resolution.Result);
                }
            }
            else
            {
                _onSuccessCallbacks.Add(callback);
            }

            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IAsyncToken<T> OnFailure(Action<Exception> callback)
        {
            if (_aborted)
            {
                return this;
            }

            if (_resolved)
            {
                if (!_resolution.Success)
                {
                    callback(_resolution.Exception);
                }
            }
            else
            {
                _onFailureCallbacks.Add(callback);
            }

            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IAsyncToken<T> OnFinally(Action<IAsyncToken<T>> callback)
        {
            if (_aborted)
            {
                return this;
            }

            if (_resolved)
            {
                callback(this);
            }
            else
            {
                _onFinallyCallbacks.Add(callback);
            }

            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IAsyncToken<T> Abort()
        {
            if (!_resolved)
            {
                _aborted = true;
            }
            
            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IAsyncToken<T> Token()
        {
            var token = new AsyncToken<T>();

            OnSuccess(token.Succeed);
            OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IAsyncToken<TR> Map<TR>(Func<T, TR> map)
        {
            var output = new AsyncToken<TR>();

            OnSuccess(value => output.Succeed(map(value)));
            OnFailure(output.Fail);

            return output;
        }
        
        /// <inheritdoc cref="IAsyncToken{T}"/>
        public Task<T> AsTask(int timeoutMs = 30000)
        {
            return Task.Run(() =>
            {
                var startTime = DateTime.Now;

                while (!_aborted && _resolution == null)
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
                    {
                        Fail(new TimeoutException("Token task took too long to complete. Pass a larger value to AsTask(int timeoutMs) if the default timeout is too short."));
                    }
                }

                if (_aborted)
                {
                    throw new OperationCanceledException();
                }
                
                if (!_resolution.Success)
                {
                    throw _resolution.Exception;
                }

                return _resolution.Result;
            });
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public Task<T> AsTask(CancellationToken cancellationToken, int timeoutMs = 30000)
        {
            return Task.Run(() =>
            {
                var startTime = DateTime.Now;

                while (!_aborted && _resolution == null && !cancellationToken.IsCancellationRequested)
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
                    {
                        Fail(new TimeoutException("Token task took too long to complete. Pass a larger value to AsTask(int timeoutMs) if the default timeout is too short."));
                    }
                }

                if (_aborted)
                {
                    throw new OperationCanceledException();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Abort();
                    throw new OperationCanceledException();
                }
                
                if (!_resolution.Success)
                {
                    throw _resolution.Exception;
                }

                return _resolution.Result;
            });
        }

        /// <summary>
        /// Provides a resolution for the token, which calls OnSuccess callbacks,
        /// follows by OnFinally callbacks.
        /// 
        /// Any exception thrown inside of callbacks is caught and then rethrown
        /// after all callbacks have been called.
        /// </summary>
        /// <param name="value">Resolution.</param>
        public void Succeed(T value)
        {
            if (_aborted)
            {
                return;
            }

            _resolved = true;
            _resolution = new Resolution(value);
            
            for (int i = 0, len = _onSuccessCallbacks.Count; i < len; i++)
            {
                try
                {
                    _onSuccessCallbacks[i](_resolution.Result);
                }
                catch (Exception exception)
                {
                    _scratchExceptions.Add(ExceptionDispatchInfo.Capture(exception));
                }
            }
            _onSuccessCallbacks.Clear();
            
            ExecuteOnFinallyCallbacks(_scratchExceptions);

            ThrowExceptions();
        }

        /// <summary>
        /// Provides a resolution for the token, which calls OnFailure callbacks,
        /// follows by OnFinally callbacks.
        /// 
        /// Any exception thrown inside of callbacks is caught and then rethrown
        /// after all callbacks have been called.
        /// </summary>
        /// <param name="exception">Resolution.</param>
        public void Fail(Exception exception)
        {
            if (_aborted)
            {
                return;
            }

            _resolved = true;
            _resolution = new Resolution(exception);

            for (int i = 0, len = _onFailureCallbacks.Count; i < len; i++)
            {
                try
                {
                    _onFailureCallbacks[i](_resolution.Exception);
                }
                catch (Exception caughtException)
                {
                    _scratchExceptions.Add(ExceptionDispatchInfo.Capture(caughtException));
                }
            }
            _onFailureCallbacks.Clear();

            ExecuteOnFinallyCallbacks(_scratchExceptions);

            ThrowExceptions();
        }

        /// <summary>
        /// Calls OnFinally callbacks and adds any exceptions throw to the input
        /// list.
        /// </summary>
        /// <param name="exceptions">List to add any thrown exceptions to.</param>
        private void ExecuteOnFinallyCallbacks(List<ExceptionDispatchInfo> exceptions)
        {
            for (int i = 0, len = _onFinallyCallbacks.Count; i < len; i++)
            {
                try
                {
                    _onFinallyCallbacks[i](this);
                }
                catch (Exception exception)
                {
                    exceptions.Add(ExceptionDispatchInfo.Capture(exception));
                }
            }
            _onFinallyCallbacks.Clear();
        }

        /// <summary>
        /// Throws exceptions if necessary.
        /// </summary>
        private void ThrowExceptions()
        {
            var exceptionCount = _scratchExceptions.Count;
            if (exceptionCount > 0)
            {
                if (1 == exceptionCount)
                {
                    var exception = _scratchExceptions[0];
                    _scratchExceptions.Clear();
                    exception.Throw();
                }
                else
                {
                    var aggregateException = new AggregateException();
                    for (int i = 0, len = _scratchExceptions.Count; i < len; i++)
                    {
                        aggregateException.Exceptions.Add(_scratchExceptions[i].SourceException);
                    }
                    _scratchExceptions.Clear();
                    throw aggregateException;
                }
            }
        }
    }
}