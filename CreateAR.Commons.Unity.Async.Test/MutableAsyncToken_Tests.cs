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

            Assert.IsTrue(isCalled);
        }
    }
}