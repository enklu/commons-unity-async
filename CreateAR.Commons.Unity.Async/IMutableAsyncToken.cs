using System;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Reusable object for asynchronous calls. Unlike <c>IAsyncToken</c>, this
    /// object can be resolved many times.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMutableAsyncToken<T>
    {
        /// <summary>
        /// Allows assigning a callback that is called when the operation is
        /// successful.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        IMutableAsyncToken<T> OnSuccess(Action<T> callback);

        /// <summary>
        /// Allows assigning a callback that is called when the operation fails.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        IMutableAsyncToken<T> OnFailure(Action<Exception> callback);

        /// <summary>
        /// Allows assigning a callback that is called regardless of whether or
        /// not the operation fails.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns></returns>
        IMutableAsyncToken<T> OnFinally(Action<IMutableAsyncToken<T>> callback);

        /// <summary>
        /// Removes an OnSuccess callback.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        /// <returns></returns>
        IMutableAsyncToken<T> Remove(Action<T> callback);

        /// <summary>
        /// Removes an OnFailure callback.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        /// <returns></returns>
        IMutableAsyncToken<T> Remove(Action<Exception> callback);

        /// <summary>
        /// Removes an OnFinally callback.
        /// </summary>
        /// <param name="callback">The callback to remove.</param>
        /// <returns></returns>
        IMutableAsyncToken<T> Remove(Action<IMutableAsyncToken<T>> callback);
    }
}