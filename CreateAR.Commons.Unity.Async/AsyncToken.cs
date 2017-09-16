using System;
using System.Collections.Generic;

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
        private readonly List<Exception> _scratchExceptions = new List<Exception>();

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
                    _scratchExceptions.Add(exception);
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
                    _scratchExceptions.Add(caughtException);
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
        private void ExecuteOnFinallyCallbacks(List<Exception> exceptions)
        {
            for (int i = 0, len = _onFinallyCallbacks.Count; i < len; i++)
            {
                try
                {
                    _onFinallyCallbacks[i](this);
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
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
                Exception exception;
                if (1 == exceptionCount)
                {
                    exception = _scratchExceptions[0];
                }
                else
                {
                    var aggregateException = new AggregateException();
                    aggregateException.Exceptions.AddRange(_scratchExceptions);
                    exception = aggregateException;
                }

                _scratchExceptions.Clear();
                throw exception;
            }
        }
    }
}