using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CreateAR.Commons.Unity.Async
{
    [TestFixture]
    public class AsyncToken_Tests
    {
        public class TestResult
        {
            
        }

        [Test]
        public void OnSuccessConstructor()
        {
            var called = false;
            var failureCalled = false;
            
            new AsyncToken<TestResult>(new TestResult())
                .OnSuccess(_ => called = true)
                .OnFailure(_ => failureCalled = true);

            Assert.IsTrue(called);
            Assert.IsFalse(failureCalled);
        }

        [Test]
        public void OnFailureConstructor()
        {
            var called = false;
            var succeedCalled = false;

            new AsyncToken<TestResult>(new Exception())
                .OnFailure(_ => called = true)
                .OnSuccess(_ => succeedCalled = true);

            Assert.IsTrue(called);
            Assert.IsFalse(succeedCalled);
        }

        [Test]
        public void OnSuccessCalledExclusively()
        {
            var token = new AsyncToken<TestResult>();
            var value = new TestResult();
            var successCalled = false;
            var failureCalled = false;

            token
                .OnSuccess(returnedValue =>
                {
                    successCalled = true;

                    Assert.AreSame(value, returnedValue);
                })
                .OnFailure(exception =>
                {
                    failureCalled = true;
                });
            token.Succeed(value);

            Assert.IsTrue(successCalled);
            Assert.IsFalse(failureCalled);
        }

        [Test]
        public void OnSuccessAborted()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.OnSuccess(_ => called = true);
            token.Abort();
            token.Succeed(new TestResult());
            token.OnSuccess(_ => called = true);

            Assert.IsFalse(called);
        }
        
        [Test]
        public void OnSuccessCalledAfterResolved()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.Succeed(new TestResult());
            token.OnSuccess(_ => called = true);

            Assert.IsTrue(called);
        }

        [Test]
        public void OnSuccessNotCalledAfterResolved()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.Fail(new Exception());
            token.OnSuccess(_ => called = true);

            Assert.IsFalse(called);
        }

        [Test]
        public void MultiOrderedOnSuccessCalled()
        {
            var token = new AsyncToken<TestResult>();

            var successACalled = false;
            var successBCalled = false;
            var successCCalled = false;

            token
                .OnSuccess(result => successACalled = true)
                .OnSuccess(result =>
                {
                    Assert.IsTrue(successACalled);

                    successBCalled = true;
                })
                .OnSuccess(result =>
                {
                    Assert.IsTrue(successACalled);
                    Assert.IsTrue(successBCalled);

                    successCCalled = true;
                });
            token.Succeed(new TestResult());

            Assert.IsTrue(successCCalled);
        }

        [Test]
        public void OnFailureCalledExclusively()
        {
            var token = new AsyncToken<TestResult>();
            var exception = new Exception();
            var successCalled = false;
            var failureCalled = false;

            token
                .OnSuccess(value =>
                {
                    successCalled = true;
                })
                .OnFailure(returnedException =>
                {
                    failureCalled = true;

                    Assert.AreSame(exception, returnedException);
                });
            token.Fail(exception);

            Assert.IsTrue(failureCalled);
            Assert.IsFalse(successCalled);
        }

        [Test]
        public void OnFailureAborted()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.OnFailure(_ => called = true);
            token.Abort();
            token.OnFailure(_ => called = true);
            token.Fail(new Exception());

            Assert.IsFalse(called);
        }

        [Test]
        public void OnFailureCalledAfterResolved()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.Fail(new Exception());
            token.OnFailure(_ => called = true);

            Assert.IsTrue(called);
        }

        [Test]
        public void OnFailureNotCalledAfterResolved()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.Succeed(new TestResult());
            token.OnFailure(_ => called = true);

            Assert.IsFalse(called);
        }

        [Test]
        public void MultiOrderedOnFailureCalled()
        {
            var token = new AsyncToken<TestResult>();

            var failureACalled = false;
            var failureBCalled = false;
            var failureCCalled = false;

            token
                .OnFailure(exception => failureACalled = true)
                .OnFailure(exception =>
                {
                    Assert.IsTrue(failureACalled);

                    failureBCalled = true;
                })
                .OnFailure(exception =>
                {
                    Assert.IsTrue(failureACalled);
                    Assert.IsTrue(failureBCalled);

                    failureCCalled = true;
                });
            token.Fail(new Exception());

            Assert.IsTrue(failureCCalled);
        }

        [Test]
        public void AbortAfterResolve()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.Succeed(new TestResult());
            token.Abort();
            token.OnSuccess(_ => called = true);

            Assert.IsTrue(called);
        }

        [Test]
        public void OnFinallyCalledFromSucceed()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.OnFinally(_ => called = true);
            token.Succeed(new TestResult());

            Assert.IsTrue(called);
        }

        [Test]
        public void OnFinallyCalledFromFail()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.OnFinally(_ => called = true);
            token.Fail(new Exception());

            Assert.IsTrue(called);
        }

        [Test]
        public void OnFinallyNotCalledAfterAbort()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token.OnFinally(_ => called = true);
            token.Abort();
            token.Fail(new Exception());

            Assert.IsFalse(called);
        }

        [Test]
        public void OnFinallyCalledInOrderOnSuccess()
        {
            var token = new AsyncToken<TestResult>();
            var successCalled = false;
            var finallyCalled = false;

            token
                .OnSuccess(_ => successCalled = true)
                .OnFinally(_ =>
                {
                    Assert.IsTrue(successCalled);

                    finallyCalled = true;
                });
            token.Succeed(new TestResult());

            Assert.IsTrue(finallyCalled);
        }

        [Test]
        public void OnFinallyCalledInOrderOnFailure()
        {
            var token = new AsyncToken<TestResult>();
            var failCalled = false;
            var finallyCalled = false;

            token
                .OnFailure(_ => failCalled = true)
                .OnFinally(_ =>
                {
                    Assert.IsTrue(failCalled);

                    finallyCalled = true;
                });
            token.Fail(new Exception());

            Assert.IsTrue(finallyCalled);
        }

        [Test]
        public void ExceptionIsCaughtOnSuccess()
        {
            var token = new AsyncToken<TestResult>();
            var exception = new Exception();
            var called = false;

            try
            {
                token
                    .OnSuccess(_ => throw exception)
                    .OnSuccess(_ => called = true);
                token.Succeed(new TestResult());
            }
            catch (Exception thrown)
            {
                Assert.AreSame(exception, thrown);
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void ExceptionIsCaughtOnFail()
        {
            var token = new AsyncToken<TestResult>();
            var exception = new Exception();
            var called = false;

            try
            {
                token
                    .OnFailure(_ => throw exception)
                    .OnFailure(_ => called = true);
                token.Fail(new Exception());
            }
            catch (Exception thrown)
            {
                Assert.AreSame(exception, thrown);
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void ExceptionIsCaughtOnFinally()
        {
            var token = new AsyncToken<TestResult>();
            var exception = new Exception();
            var called = false;

            try
            {
                token
                    .OnSuccess(_ => throw exception)
                    .OnFinally(_ => called = true);
                token.Succeed(new TestResult());
            }
            catch (Exception thrown)
            {
                Assert.AreSame(exception, thrown);
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void AggregateExceptionCreated()
        {
            var token = new AsyncToken<TestResult>();
            var exception1 = new Exception();
            var exception2 = new Exception();
            var called = false;

            try
            {
                token
                    .OnSuccess(_ => throw exception1)
                    .OnSuccess(_ => throw exception2)
                    .OnSuccess(_ => called = true);
                token.Succeed(new TestResult());
            }
            catch (AggregateException exception)
            {
                Assert.AreEqual(2, exception.Exceptions.Count);
            }

            Assert.IsTrue(called);
        }

        [Test]
        public void AbortInCallbackNotRespected()
        {
            var token = new AsyncToken<TestResult>();
            var called = false;

            token
                .OnSuccess(_ => token.Abort())
                .OnSuccess(_ => called = true)
                .OnFinally(_ => called = true);
            token.Succeed(new TestResult());

            Assert.IsTrue(called);
        }

        [Test]
        public async Task TaskSuccess()
        {
            var token = new AsyncToken<float>();

            var callbackResult = 0f;
            token.OnSuccess(val => callbackResult = val);

            var taskResult = 0f;
            var task = token
                .AsTask()
                .ContinueWith(cTask =>
                {
                    taskResult = cTask.Result;
                });

            var expectedValue = 2.26f;
            
            token.Succeed(expectedValue);
            await task;
            
            Assert.AreEqual(expectedValue, callbackResult);
            Assert.AreEqual(expectedValue, taskResult);
        }
        
        [Test]
        public async Task TaskFail()
        {
            var token = new AsyncToken<float>();

            Exception callbackException = null;
            token.OnFailure(exception => callbackException = exception);
            
            var task = token.AsTask();

            var expectedException = new InvalidOperationException("Test exception");
            token.Fail(expectedException);

            try
            {
                await task;
            }
            catch (Exception exception)
            {
                Assert.AreEqual(expectedException, exception);
            }
            Assert.AreEqual(expectedException, callbackException);
        }
        
        [Test]
        public async Task TaskAborted()
        {
            var token = new AsyncToken<float>();

            Exception callbackException = null;
            token.OnFailure(exception => callbackException = exception);
            
            var task = token.AsTask();
            var taskSuccess = false;
            token.Abort();
            
            try
            {
                await task;
                taskSuccess = true;
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception is OperationCanceledException);
            }
            Assert.IsFalse(taskSuccess);
        }

        [Test]
        public async Task TaskTimeout()
        {
            var token = new AsyncToken<float>();

            Exception callbackException = null;
            token.OnFailure(exception => callbackException = exception);
            
            var task = token.AsTask(5000);
            var taskSuccess = false;
            
            try
            {
                await task;
                taskSuccess = true;
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception is TimeoutException);
            }
            Assert.IsFalse(taskSuccess);
            
            // Ensure the backing token fails as well.
            Assert.IsTrue(callbackException is TimeoutException);
        }
        
        [Test]
        public async Task TaskCancelled()
        {
            var token = new AsyncToken<float>();

            Exception callbackException = null;
            token.OnFailure(exception => callbackException = exception);
            
            var cancellation = new CancellationTokenSource();

            var startTime = DateTime.Now;
            var timeoutTime = 60000;
            var task = token.AsTask(cancellation.Token, timeoutTime);
            var taskSuccess = false;

            cancellation.Cancel();
            
            try
            {
                await task;
                taskSuccess = true;
            }
            catch (Exception exception)
            {
                Assert.IsTrue(exception is OperationCanceledException);
            }
            
            Assert.IsFalse(taskSuccess);
            
            // Ensure the backing token has not failed.
            Assert.IsNull(callbackException);
            
            // Finally, ensure that the cancellation came from the token and not a timeout!
            Assert.IsTrue((DateTime.Now - startTime).TotalMilliseconds <= timeoutTime);
        }

        [Test]
        public void Token()
        {
            var token = new AsyncToken<TestResult>();
            var successCalled = true;
            
            token
                .Token()
                .OnSuccess(_ => successCalled = true);

            token.Succeed(new TestResult());

            Assert.IsTrue(successCalled);
        }
    }
}
