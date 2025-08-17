using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class ContainerResolverTest {
        private interface IResolveTestClass {
            bool IsConstructed { get; }
        } 
        
        private interface IResolveTestClass2 {
            bool IsConstructed { get; }
            IResolveTestClass ResolveTestClass { get; } 
        } 
        
        private class ResolveTestClass : IResolveTestClass {
            public bool IsConstructed { get; }

            public ResolveTestClass() {
                IsConstructed = true;
            }
        }
        
        private class ResolveTestClass2 : IResolveTestClass2 {
            public bool IsConstructed { get; }
            public IResolveTestClass ResolveTestClass { get; } 

            public ResolveTestClass2(IResolveTestClass resolveTestClass) {
                IsConstructed = true;
                ResolveTestClass = resolveTestClass;
            }
        }
        
        private class ResolveTestClass3 : IResolveTestClass {
            public bool IsConstructed { get; }

            public ResolveTestClass3() {
                IsConstructed = true;
            }
        }
        
        [Test]
        public void TestResolveDelegate() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            
            containerRegistrator.Register<IResolveTestClass2, ResolveTestClass2>(resolver => new ResolveTestClass2(resolver.Resolve<IResolveTestClass>()));
            containerRegistrator.Register<IResolveTestClass, ResolveTestClass>(_ => new ResolveTestClass());

            var resolveTestClass2 = containerResolver.Resolve<IResolveTestClass2>();
            var resolveTestClass = containerResolver.Resolve<IResolveTestClass>();
            
            Assert.IsNotNull(resolveTestClass);
            Assert.IsNotNull(resolveTestClass2);
            Assert.IsTrue(resolveTestClass.IsConstructed);
            Assert.IsTrue(resolveTestClass2.IsConstructed);
            
            Assert.AreEqual(resolveTestClass, resolveTestClass2.ResolveTestClass);
        }
        
        [Test]
        public void TestResolveImplementation() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            
            containerRegistrator.Register<IResolveTestClass2, ResolveTestClass2>();
            containerRegistrator.Register<IResolveTestClass, ResolveTestClass>();

            var resolveTestClass2 = containerResolver.Resolve<IResolveTestClass2>();
            var resolveTestClass = containerResolver.Resolve<IResolveTestClass>();
            
            Assert.IsNotNull(resolveTestClass);
            Assert.IsNotNull(resolveTestClass2);
            Assert.IsTrue(resolveTestClass.IsConstructed);
            Assert.IsTrue(resolveTestClass2.IsConstructed);
            
            Assert.AreEqual(resolveTestClass, resolveTestClass2.ResolveTestClass);
        }

        [Test]
        public void TestResolveDeepAll() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            
            containerRegistrator.Register<IResolveTestClass2, ResolveTestClass2>();
            containerRegistrator.Register<IResolveTestClass, ResolveTestClass>();
            containerRegistrator.Register<ResolveTestClass3, ResolveTestClass3>();

            var resolvedTypeObjects = containerResolver.ResolveDeepAll<IResolveTestClass>();

            Debug.Log("Resolved types: " + string.Join(", ", resolvedTypeObjects.Select(x => x.GetType().Name)));
            
            Assert.AreEqual(2, resolvedTypeObjects.Count);
            
            Assert.IsTrue(resolvedTypeObjects.Any(x => x is ResolveTestClass));
            Assert.IsFalse(resolvedTypeObjects.Any(x => x is ResolveTestClass2));
            Assert.IsTrue(resolvedTypeObjects.Any(x => x is ResolveTestClass3));
        }

        [Test]
        public void TestResolveSameRef() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            
            containerRegistrator.Register<IResolveTestClass, ResolveTestClass>();

            var firstResolve = containerResolver.Resolve<IResolveTestClass>();
            var secondResolve = containerResolver.Resolve<IResolveTestClass>();
            
            Assert.IsNotNull(firstResolve);
            Assert.IsNotNull(secondResolve);
            Assert.AreSame(firstResolve, secondResolve);
        }
    }
}