using NUnit.Framework;

namespace CreateAR.Commons.Unity.Async
{
    [TestFixture]
    public class WatchedValue_Tests
    {
        public class TestClass
        {
            public string Foo;
        }

        [Test]
        public void CallWatcher()
        {
            var called = false;
            var instance = new TestClass();

            var watcher = new WatchedValue<TestClass>();
            watcher.OnChanged += value =>
            {
                called = true;

                Assert.AreSame(instance, value);
            };

            watcher.Value = instance;

            Assert.IsTrue(called);
        }

        [Test]
        public void MissWatcherConstructor()
        {
            var called = false;
            var instance = new TestClass();

            var watcher = new WatchedValue<TestClass>(instance);
            watcher.OnChanged += value =>
            {
                called = true;

                Assert.AreSame(instance, value);
            };

            Assert.IsFalse(called);
        }

        [Test]
        public void CallWatcherMultiple()
        {
            var numCalled = 0;
            
            var watcher = new WatchedValue<TestClass>();
            watcher.OnChanged += value =>
            {
                numCalled++;
            };

            watcher.Value = new TestClass();
            watcher.Value = new TestClass();
            watcher.Value = new TestClass();

            Assert.AreEqual(3, numCalled);
        }

        [Test]
        public void IgnoreEqualValues()
        {
            var numCalled = 0;

            var watcher = new WatchedValue<TestClass>();
            watcher.OnChanged += value =>
            {
                numCalled++;
            };

            var instance = new TestClass();
            watcher.Value = instance;
            watcher.Value = instance;
            watcher.Value = instance;

            Assert.AreEqual(1, numCalled);
        }

        [Test]
        public void CallReady()
        {
            var called = false;
            var instance = new TestClass();

            var watcher = new WatchedValue<TestClass>();
            watcher
                .Ready()
                .OnSuccess(value =>
                {
                    called = true;

                    Assert.AreSame(instance, value);
                });

            watcher.Value = instance;

            Assert.IsTrue(called);
        }

        [Test]
        public void CallReadyRetro()
        {
            var called = false;
            var instance = new TestClass();

            var watcher = new WatchedValue<TestClass>();
            watcher.Value = instance;
            watcher
                .Ready()
                .OnSuccess(value =>
                {
                    called = true;

                    Assert.AreSame(instance, value);
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void CallReadyConstructor()
        {
            var called = false;
            var instance = new TestClass();

            var watcher = new WatchedValue<TestClass>();
            watcher.Value = instance;
            watcher
                .Ready()
                .OnSuccess(value =>
                {
                    called = true;

                    Assert.AreSame(instance, value);
                });

            Assert.IsTrue(called);
        }

        [Test]
        public void CallReadyIndividualTokens()
        {
            var called = false;
            var abortCalled = false;
            var instance = new TestClass();

            var watcher = new WatchedValue<TestClass>();
            watcher
                .Ready()
                .OnSuccess(value =>
                {
                    called = true;
                });
            watcher
                .Ready()
                .OnSuccess(value =>
                {
                    abortCalled = true;
                })
                .Abort();

            watcher.Value = instance;

            Assert.IsTrue(called);
            Assert.IsFalse(abortCalled);
        }
    }
}