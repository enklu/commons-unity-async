using System;
using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Essentially an <c>AsyncToken</c> that may be resolved repeatedly.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MutableAsyncToken<T> : IMutableAsyncToken<T>
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
        /// Set to true if resolved.
        /// </summary>
        private bool _resolved = false;

        /// <summary>
        /// Set when resolved with Succeed or Fail.
        /// </summary>
        private Resolution _resolution;

        /// <summary>
        /// True when calling resolutions.
        /// </summary>
        private bool _isExecuting = false;

        /// <summary>
        /// Lists of callbacks.
        /// </summary>
        private readonly List<Action<T>> _onSuccessCallbacks = new List<Action<T>>();
        private readonly List<Action<Exception>> _onFailureCallbacks = new List<Action<Exception>>();
        private readonly List<Action<IMutableAsyncToken<T>>> _onFinallyCallbacks = new List<Action<IMutableAsyncToken<T>>>();
        private readonly List<Exception> _scratchExceptions = new List<Exception>();

        private readonly List<Action<T>> _onSuccessToRemove = new List<Action<T>>();
        private readonly List<Action<Exception>> _onFailureToRemove = new List<Action<Exception>>();
        private readonly List<Action<IMutableAsyncToken<T>>> _onFinallyToRemove = new List<Action<IMutableAsyncToken<T>>>();

        /// <summary>
        /// Creates a token with no resolution.
        /// </summary>
        public MutableAsyncToken()
        {

        }

        /// <summary>
        /// Creates a successful token.
        /// </summary>
        /// <param name="value">The value to resolve with.</param>
        public MutableAsyncToken(T value)
        {
            Succeed(value);
        }

        /// <summary>
        /// Creates a failed token.
        /// </summary>
        /// <param name="exception">The exception to fail with.</param>
        public MutableAsyncToken(Exception exception)
        {
            Fail(exception);
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IMutableAsyncToken<T> OnSuccess(Action<T> callback)
        {
            _onSuccessCallbacks.Add(callback);

            if (_resolved)
            {
                if (_resolution.Success)
                {
                    callback(_resolution.Result);
                }
            }

            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IMutableAsyncToken<T> OnFailure(Action<Exception> callback)
        {
            _onFailureCallbacks.Add(callback);

            if (_resolved)
            {
                if (!_resolution.Success)
                {
                    callback(_resolution.Exception);
                }
            }

            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IMutableAsyncToken<T> OnFinally(Action<IMutableAsyncToken<T>> callback)
        {
            _onFinallyCallbacks.Add(callback);

            if (_resolved)
            {
                callback(this);
            }

            return this;
        }

        public IMutableAsyncToken<T> Remove(Action<T> callback)
        {
            if (_isExecuting)
            {
                _onSuccessToRemove.Add(callback);
            }
            else
            {
                _onSuccessCallbacks.Remove(callback);
            }

            return this;
        }

        public IMutableAsyncToken<T> Remove(Action<Exception> callback)
        {
            if (_isExecuting)
            {
                _onFailureToRemove.Add(callback);
            }
            else
            {
                _onFailureCallbacks.Remove(callback);
            }

            return this;
        }

        public IMutableAsyncToken<T> Remove(Action<IMutableAsyncToken<T>> callback)
        {
            if (_isExecuting)
            {
                _onFinallyToRemove.Add(callback);
            }
            else
            {
                _onFinallyCallbacks.Remove(callback);
            }

            return this;
        }

        /// <inheritdoc cref="IAsyncToken{T}"/>
        public IMutableAsyncToken<TR> Map<TR>(Func<T, TR> map)
        {
            var output = new MutableAsyncToken<TR>();

            OnSuccess(value => output.Succeed(map(value)));
            OnFailure(output.Fail);

            return output;
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
            _isExecuting = true;

            // overwrite any current resolution
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
            
            ExecuteOnFinallyCallbacks(_scratchExceptions);
            HandleRemoves();

            _isExecuting = false;

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
            _isExecuting = true;

            // overwrite any current resolution
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
            
            ExecuteOnFinallyCallbacks(_scratchExceptions);
            HandleRemoves();

            _isExecuting = false;

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
        }

        /// <summary>
        /// Removes any handlers that requested remove during execution.
        /// </summary>
        private void HandleRemoves()
        {
            var len = _onSuccessToRemove.Count;
            if (len > 0)
            {
                for (var i = 0; i < len; i++)
                {
                    _onSuccessCallbacks.Remove(_onSuccessToRemove[i]);
                }
                _onSuccessToRemove.Clear();
            }

            len = _onFailureToRemove.Count;
            if (len > 0)
            {
                for (var i = 0; i < len; i++)
                {
                    _onFailureCallbacks.Remove(_onFailureToRemove[i]);
                }
                _onFailureToRemove.Clear();
            }

            len = _onFinallyToRemove.Count;
            if (len > 0)
            {
                for (var i = 0; i < len; i++)
                {
                    _onFinallyCallbacks.Remove(_onFinallyToRemove[i]);
                }
                _onFinallyToRemove.Clear();
            }
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