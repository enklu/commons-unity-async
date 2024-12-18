using System;
using System.Runtime.CompilerServices;

namespace CreateAR.Commons.Unity.Async
{
    public static class IAsyncTokenExtensions
    {
        /// <summary>
        /// Enables 'await' functionality on IAsyncToken<T> using OnSuccess and OnFailure.
        /// </summary>
        /// <typeparam name="T">The result type.</typeparam>
        /// <param name="token">The IAsyncToken instance.</param>
        /// <returns>A custom awaiter for the IAsyncToken.</returns>
        public static AsyncTokenAwaiter<T> GetAwaiter<T>(this IAsyncToken<T> token)
        {
            return new AsyncTokenAwaiter<T>(token);
        }
    }

    /// <summary>
    /// Custom awaiter for IAsyncToken<T>.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    public class AsyncTokenAwaiter<T> : INotifyCompletion
    {
        private readonly IAsyncToken<T> _token;
        private Action _continuation;
        private T _result;
        private Exception _exception;
        private bool _isCompleted;

        public AsyncTokenAwaiter(IAsyncToken<T> token)
        {
            _token = token;

            // Set up callbacks for OnSuccess and OnFailure
            _token
                .OnSuccess(result =>
                {
                    _result = result;
                    _isCompleted = true;
                    _continuation?.Invoke();
                })
                .OnFailure(ex =>
                {
                    _exception = ex;
                    _isCompleted = true;
                    _continuation?.Invoke();
                });
        }

        /// <summary>
        /// Indicates if the operation has completed.
        /// </summary>
        public bool IsCompleted => _isCompleted;

        /// <summary>
        /// Retrieves the result or throws the stored exception.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public T GetResult()
        {
            if (_exception != null)
            {
                throw _exception;
            }

            return _result;
        }

        /// <summary>
        /// Schedules the continuation when the operation completes.
        /// </summary>
        /// <param name="continuation">The continuation to execute.</param>
        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
            if (_isCompleted)
            {
                _continuation.Invoke();
            }
        }
    }
}
