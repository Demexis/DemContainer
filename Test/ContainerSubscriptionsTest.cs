using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class ContainerSubscriptionsTest {
        private interface ITestSubscribe {
            
        }
        
        private interface ITestSubscribeInterface {
            
        }

        public class TestSubscribeClass : ITestSubscribeInterface, ITestSubscribe {
            
        }
        
        public class TestSubscribeClass2 : ITestSubscribe {
            
        }
        
        [Test]
        public void TestSubscribe() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            using var containerSubscriptions = new ContainerSubscriptions(containerRegistrator, containerResolver);
            
            containerRegistrator.Register<TestSubscribeClass, TestSubscribeClass>();
            containerRegistrator.Register<ITestSubscribeInterface, TestSubscribeClass>();

            var counter = 0;
            
            containerSubscriptions.Subscribe<ITestSubscribe>(obj => {
                counter++;
                Debug.Log("Subscribed to " + obj.GetType().Name);
            });
            
            Assert.AreEqual(2, counter);
            
            containerRegistrator.Register<TestSubscribeClass2, TestSubscribeClass2>();
            
            Assert.AreEqual(3, counter);
        }

        [Test]
        public void TestSubscribeTwice() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            using var containerSubscriptions = new ContainerSubscriptions(containerRegistrator, containerResolver);
            
            containerRegistrator.Register<TestSubscribeClass, TestSubscribeClass>();
            containerRegistrator.Register<ITestSubscribeInterface, TestSubscribeClass>();

            var firstCounter = 0;
            var secondCounter = 0;

            var firstSubscriptions = new List<ITestSubscribe>();
            var secondSubscriptions = new List<ITestSubscribe>();
            
            containerSubscriptions.Subscribe<ITestSubscribe>(obj => {
                firstCounter++;
                firstSubscriptions.Add(obj);
                Debug.Log("First callback. Subscribed to " + obj.GetType().Name);
            });
            
            containerSubscriptions.Subscribe<ITestSubscribe>(obj => {
                secondCounter++;
                secondSubscriptions.Add(obj);
                Debug.Log("Second callback. Subscribed to " + obj.GetType().Name);
            });
            
            Assert.AreEqual(2, firstCounter);
            Assert.AreEqual(2, secondCounter);
            
            containerRegistrator.Register<TestSubscribeClass2, TestSubscribeClass2>();
            
            Assert.AreEqual(3, firstCounter);
            Assert.AreEqual(3, secondCounter);
            
            Assert.IsTrue(!firstSubscriptions.Except(secondSubscriptions).Any());
            Assert.IsTrue(!secondSubscriptions.Except(firstSubscriptions).Any());
        }
    }
}