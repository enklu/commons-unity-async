using System;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// Creates a new token that is chained to this token.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<T> Token();

        /// <summary>
        /// Maps a token to another token type.
        /// </summary>
        /// <typeparam name="TR">Result generic parameter/</typeparam>
        /// <param name="map">Function to map between types.</param>
        /// <returns></returns>
        IAsyncToken<TR> Map<TR>(Func<T, TR> map);

        /// <summary>
        /// Converts the token into a task that can be awaited.
        /// An aborted token throws an OperationCanceledException instance.
        /// A task that times out also fails the underlying token for parity.
        /// </summary>
        /// <returns></returns>
        /// <param name="timeoutMs">The time to wait for the task to complete before failing.</param>
        Task<T> AsTask(int timeoutMs = 30000);

        /// <summary>
        /// Converts the token into a task that can be awaited.
        /// An aborted token throws an OperationCanceledException instance.
        /// A task that is cancelled does not fail the underlying token, for parity with Abort().
        /// </summary>
        /// <returns></returns>
        /// <param name="timeoutMs">The time to wait for the task to complete before failing.</param>
        /// <param name="cancellationToken">Custom cancellation.</param>
        Task<T> AsTask(CancellationToken cancellationToken, int timeoutMs = 30000);
    }
}