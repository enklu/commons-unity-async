using System;
using NUnit.Framework;

namespace CreateAR.Commons.Unity.Async
{
    [TestFixture]
    public class SynchronizedObject_Tests
    {
        public class DummyClass
        {
            public int Foo;

            public DummyClass(int foo)
            {
                Foo = foo;
            }

            public DummyClass(DummyClass other)
            {
                Foo = other.Foo;
            }
        }
        
        [Test]
        public void Single()
        {
            var calledSub = false;
            var a = new DummyClass(10);

            var obj = new SynchronizedObject<DummyClass>(
                a,
                (b, finished) =>
                {
                    calledSub = true;

                    Assert.AreEqual(b.Foo, a.Foo + 1);
                });

            obj.Queue((current, next) => next(new DummyClass(current.Foo + 1)));

            Assert.IsTrue(calledSub);
        }

        [Test]
        public void MultipleQueued()
        {
            var a = new DummyClass(10);

            var obj = new SynchronizedObject<DummyClass>(
                a,
                (e, finished) =>
                {
                    finished();
                });

            obj.Queue((current, next) => next(new DummyClass(current.Foo + 1)));
            obj.Queue((current, next) => next(new DummyClass(current.Foo + 1)));
            obj.Queue((current, next) => next(new DummyClass(current.Foo + 1)));
            
            Assert.AreEqual(obj.Data.Foo, a.Foo + 3);
        }

        [Test]
        public void DiscardQueueInSubscriber()
        {
            var called = 0;
            var a = new DummyClass(10);

            Action queue = null;
            var obj = new SynchronizedObject<DummyClass>(
                a,
                (b, finished) =>
                {
                    called++;
                    
                    queue();
                
                    finished();
                });

            queue = () =>
            {
                Assert.IsFalse(obj.Queue((current, next) => next(new DummyClass(13))));
            };

            obj.Queue((current, next) => next(new DummyClass(12)));

            Assert.AreEqual(1, called);
            Assert.AreEqual(12, obj.Data.Foo);
        }

        [Test]
        public void QueueInQueue()
        {
            var obj = new SynchronizedObject<DummyClass>(
                new DummyClass(12),
                (@new, finished) => finished());

            obj.Queue((prev, next) =>
            {
                obj.Queue((_, nnext) => nnext(new DummyClass(-14)));

                next(new DummyClass(16));
            });

            Assert.AreEqual(-14, obj.Data.Foo);
        }
    }
}
