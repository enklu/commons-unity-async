using System;
using System.Collections.Generic;

namespace CreateAR.Commons.Unity.Async
{
    /// <summary>
    /// Wraps an object and queues asynchronous actions on object. Instead of
    /// an OnChanged event, the subscriber to changes is also asyncronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SynchronizedObject<T>
    {
        /// <summary>
        /// Actions to perform.
        /// </summary>
        private readonly Queue<Action<T, Action<T>>> _actions = new Queue<Action<T, Action<T>>>();

        /// <summary>
        /// The subscriber.
        /// </summary>
        private readonly Action<T, Action> _subscriber;

        /// <summary>
        /// True iff the an action is processing.
        /// </summary>
        private bool _isWaitingOnProcessing;

        /// <summary>
        /// True iff the subscriber is processing.
        /// </summary>
        private bool _isWaitingOnSubscriber;

        /// <summary>
        /// The underlying data.
        /// </summary>
        public T Data { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">The initial data.</param>
        /// <param name="subscriber">The subscriber. Called every time the data is changed.</param>
        public SynchronizedObject(T data, Action<T, Action> subscriber)
        {
            Data = data;

            _subscriber = subscriber;
        }

        /// <summary>
        /// Queues an asynchronous action on an object.
        /// </summary>
        /// <param name="action">The action to take.</param>
        public bool Queue(Action<T, Action<T>> action)
        {
            if (_isWaitingOnSubscriber)
            {
                return false;
            }

            _actions.Enqueue(action);

            ProcessQueue();

            return true;
        }

        /// <summary>
        /// Processes the next item in the queue.
        /// </summary>
        private void ProcessQueue()
        {
            if (_isWaitingOnProcessing)
            {
                return;
            }

            if (_actions.Count == 0)
            {
                return;
            }

            _isWaitingOnProcessing = true;

            var action = _actions.Dequeue();
            action(Data, OnComplete);
        }

        /// <summary>
        /// Called when the action is complete.
        /// </summary>
        /// <param name="val">The new value.</param>
        private void OnComplete(T val)
        {
            Data = val;

            _isWaitingOnProcessing = false;
            _isWaitingOnSubscriber = true;
            _subscriber(Data, () =>
            {
                _isWaitingOnSubscriber = false;

                ProcessQueue();
            });
        }
    }
}