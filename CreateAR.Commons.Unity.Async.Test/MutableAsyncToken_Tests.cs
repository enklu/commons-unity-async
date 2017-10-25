using System;
using NUnit.Framework;

namespace CreateAR.Commons.Unity.Async
{
    [TestFixture]
    public class MutableAsyncToken_Tests
    {
        public class MyClass
        {
            public string Foo = Guid.NewGuid().ToString();
        }

        [Test]
        public void Succeed()
        {
            var isCalled = false;
            var value = new MyClass();
            
            var token = new MutableAsyncToken<MyClass>();
            token
                .OnSuccess(result =>
                {
                    isCalled = true;

                    Assert.AreSame(value, result);
                })
                .OnFailure(exception => throw exception);

            token.Succeed(value);

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void SucceedPost()
        {
            var isCalled = false;
            var value = new MyClass();

            var token = new MutableAsyncToken<MyClass>();

            token.Succeed(value);

            token
                .OnSuccess(result =>
                {
                    isCalled = true;

                    Assert.AreSame(value, result);
                })
                .OnFailure(exception => throw exception);

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void SucceedMulti()
        {
            var called = 0;
            var value = new MyClass();

            var token = new MutableAsyncToken<MyClass>();
            
            token
                .OnSuccess(result =>
                {
                    called++;

                    Assert.AreSame(value, result);
                })
                .OnFailure(exception => throw exception);

            token.Succeed(value);
            token.Succeed(value);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void SucceedMultiPrePost()
        {
            var called = 0;
            var value = new MyClass();

            var token = new MutableAsyncToken<MyClass>();

            token.Succeed(value);

            token
                .OnSuccess(result =>
                {
                    called++;

                    Assert.AreSame(value, result);
                })
                .OnFailure(exception => throw exception);

            token.Succeed(value);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void SucceedAddRemove()
        {
            var called = 0;
            var value = new MyClass();

            var token = new MutableAsyncToken<MyClass>();

            token.Succeed(value);

            void SucceedAction(MyClass result)
            {
                called++;

                Assert.AreSame(value, result);
            }

            token.OnSuccess(SucceedAction);
            token.Remove(SucceedAction);

            token.Succeed(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void SucceedAddRemoveSelf()
        {
            var called = 0;
            var value = new MyClass();

            var token = new MutableAsyncToken<MyClass>();

            token.Succeed(value);

            void SucceedAction(MyClass result)
            {
                called++;

                Assert.AreSame(value, result);

                token.Remove(SucceedAction);
            }

            token.OnSuccess(SucceedAction);
            
            token.Succeed(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void SucceedAddRemoveThreadSafe()
        {
            var called = 0;
            var value = new MyClass();

            var token = new MutableAsyncToken<MyClass>();
            
            void SucceedAction(MyClass result)
            {
                called++;

                Assert.AreSame(value, result);

                token.Remove(SucceedAction);
            }

            token.OnSuccess(_ =>
            {
                token.Remove(SucceedAction);
            });
            token.OnSuccess(SucceedAction);

            token.Succeed(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void Fail()
        {
            var isCalled = false;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();
            token
                .OnFailure(result =>
                {
                    isCalled = true;

                    Assert.AreSame(value, result);
                })
                .OnSuccess(result => throw new Exception());

            token.Fail(value);

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void FailPost()
        {
            var isCalled = false;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            token
                .OnFailure(result =>
                {
                    isCalled = true;

                    Assert.AreSame(value, result);
                })
                .OnSuccess(result => throw new Exception());

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void FailMulti()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token
                .OnFailure(result =>
                {
                    called++;

                    Assert.AreSame(value, result);
                })
                .OnSuccess(exception => throw new Exception());

            token.Fail(value);
            token.Fail(value);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void FailMultiPrePost()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            token
                .OnFailure(result =>
                {
                    called++;

                    Assert.AreSame(value, result);
                })
                .OnSuccess(exception => throw new Exception());
 
            token.Fail(value);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void FailAddRemove()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            void FailAction(Exception result)
            {
                called++;

                Assert.AreSame(value, result);
            }

            token.OnFailure(FailAction);
            token.Remove(FailAction);

            token.Fail(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void FailAddRemoveSelf()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            void FailAction(Exception result)
            {
                called++;

                Assert.AreSame(value, result);

                token.Remove(FailAction);
            }

            token.OnFailure(FailAction);

            token.Fail(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void FailAddRemoveThreadSafe()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            void FailAction(Exception result)
            {
                called++;

                Assert.AreSame(value, result);

                token.Remove(FailAction);
            }

            token.OnFailure(_ =>
            {
                token.Remove(FailAction);
            });
            token.OnFailure(FailAction);

            token.Fail(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void Finally()
        {
            var isCalled = false;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();
            token
                .OnFinally(result =>
                {
                    isCalled = true;

                    Assert.AreSame(token, result);
                });

            token.Fail(value);

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void FinallyPost()
        {
            var isCalled = false;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            token
                .OnFinally(result =>
                {
                    isCalled = true;

                    Assert.AreSame(token, result);
                });

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void FinallyMulti()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token
                .OnFinally(result =>
                {
                    called++;

                    Assert.AreSame(token, result);
                });

            token.Fail(value);
            token.Fail(value);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void FinallyMultiPrePost()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            token
                .OnFinally(result =>
                {
                    called++;

                    Assert.AreSame(token, result);
                });

            token.Fail(value);

            Assert.AreEqual(2, called);
        }

        [Test]
        public void FinallyAddRemove()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            void FinallyAction(IMutableAsyncToken<MyClass> result)
            {
                called++;

                Assert.AreSame(token, result);
            }

            token.OnFinally(FinallyAction);
            token.Remove(FinallyAction);

            token.Fail(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void FinallyAddRemoveSelf()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            token.Fail(value);

            void FinallyAction(IMutableAsyncToken<MyClass> result)
            {
                called++;

                Assert.AreSame(token, result);

                token.Remove(FinallyAction);
            }

            token.OnFinally(FinallyAction);

            token.Fail(value);

            Assert.AreEqual(1, called);
        }

        [Test]
        public void FinallyAddRemoveThreadSafe()
        {
            var called = 0;
            var value = new Exception();

            var token = new MutableAsyncToken<MyClass>();

            void FinallyAction(IMutableAsyncToken<MyClass> result)
            {
                called++;

                Assert.AreSame(token, result);

                token.Remove(FinallyAction);
            }

            token.OnFinally(_ =>
            {
                token.Remove(FinallyAction);
            });
            token.OnFinally(FinallyAction);

            token.Fail(value);

            Assert.AreEqual(1, called);
        }
    }
}