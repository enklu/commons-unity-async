using System;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Represents a value that can be watched.
    /// </summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [Obsolete("Object considered dangerous. Will be removed in future release.")]
    public sealed class WatchedValue<T>
    {
        /// <summary>
        /// Internal token used to track ready state.
        /// </summary>
        private readonly AsyncToken<T> _readyToken = new AsyncToken<T>();

        /// <summary>
        /// Backing value for Value property.
        /// </summary>
        private T _value;
        
        /// <summary>
        /// Value Accessor
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                {
                    return;
                }

                _value = value;

                // resolve ready-- will only fire the first time
                _readyToken.Succeed(_value);

                // call event
                OnChanged?.Invoke(_value);
            }
        }

        /// <summary>
        /// Called when the value has been changed.
        /// </summary>
        public event Action<T> OnChanged;

        /// <summary>
        /// Creates a WatchedValue.
        /// </summary>
        public WatchedValue()
        {
            // empty
        }

        /// <summary>
        /// Creates a WatchedValue with an initial value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public WatchedValue(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns a token that is resolved when the value is first set.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<T> Ready()
        {
            return _readyToken.Token();
        }

        /// <summary>
        /// Called when value could not be loaded.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Fail(Exception exception)
        {
            _readyToken.Fail(exception);
        }
    }
}