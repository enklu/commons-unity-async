using System;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Interface for asynchronous calls.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncToken<T>
    {
        /// <summary>
        /// Allows assigning a callback that is called when the operation is
        /// successful.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        IAsyncToken<T> OnSuccess(Action<T> callback);

        /// <summary>
        /// Allows assigning a callback that is called when the operation fails.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        IAsyncToken<T> OnFailure(Action<Exception> callback);

        /// <summary>
        /// Allows assigning a callback that is called regardless of whether or
        /// not the operation fails.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        IAsyncToken<T> OnFinally(Action<IAsyncToken<T>> callback);

        /// <summary>
        /// Kills the token. No further actions will be called.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<T> Abort();
    }
}