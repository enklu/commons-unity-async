using System;

namespace CreateAR.Commons.Unity.Async
{
    public interface IMutableAsyncToken<T> : IAsyncToken<T>
    {
        //
    }

    public class MutableAsyncToken<T> : IMutableAsyncToken<T>
    {
        public IAsyncToken<T> OnSuccess(Action<T> callback)
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<T> OnFailure(Action<Exception> callback)
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<T> OnFinally(Action<IAsyncToken<T>> callback)
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<T> Abort()
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<T> Token()
        {
            throw new NotImplementedException();
        }
    }
}
