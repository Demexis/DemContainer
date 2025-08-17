using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DemContainer.Tests {
    [TestFixture]
    public class BaseConstructorDependencyContainerTest {
        public interface ITestService { }
        
        public interface ISecondTestService {
            
        }

        public class TestService : ITestService { }

        public class SecondTestService : ISecondTestService {
            
        }

        public class TestMonoBehaviour : MonoBehaviour, IConstructor<ITestService> {
            public ITestService testService = null!;

            public void Construct(ITestService t1) {
                Debug.Log($"Constructed {nameof(TestMonoBehaviour)} with dependency - " + t1.GetType().Name);
                testService = t1;
            }
        }
        
        public class TestInheritedMonoBehaviour : TestMonoBehaviour, IConstructor<ISecondTestService, ITestService> {
            [FormerlySerializedAs("secondService")] public ISecondTestService secondTestService = null!;

            public void Construct(ISecondTestService t1, ITestService t2) {
                Debug.Log($"Constructed {nameof(TestInheritedMonoBehaviour)} with two dependencies - " + t1.GetType().Name
                + " and " + t2.GetType().Name);
                secondTestService = t1;
                
                base.Construct(t2);
            }
        }

        [Test]
        public void TestInjectTestObject() {
            var testService = new TestService();

            var typeResolver = new Dictionary<Type, object> {
                { typeof(ITestService), testService }
            };

            var baseConstructorDependencyContainer = new BaseConstructorDependencyContainer(type => typeResolver[type]);

            var gameObject = new GameObject("SUS_TEST");
            var testMonoBehaviour = gameObject.AddComponent<TestMonoBehaviour>();

            baseConstructorDependencyContainer.ConstructComponents(gameObject);

            Assert.AreEqual(testService, testMonoBehaviour.testService);
        }
        
        [Test]
        public void TestInjectInheritedTestObject() {
            var testService = new TestService();
            var secondTestService = new SecondTestService();

            var typeResolver = new Dictionary<Type, object> {
                { typeof(ITestService), testService },
                { typeof(ISecondTestService), secondTestService }
            };

            var baseConstructorDependencyContainer = new BaseConstructorDependencyContainer(type => typeResolver[type]);

            var gameObject = new GameObject("SUS_TEST_2");
            var testMonoBehaviour = gameObject.AddComponent<TestInheritedMonoBehaviour>();

            baseConstructorDependencyContainer.ConstructComponents(gameObject);

            Assert.AreEqual(testService, testMonoBehaviour.testService);
            Assert.AreEqual(secondTestService, testMonoBehaviour.secondTestService);
        }
    }
}