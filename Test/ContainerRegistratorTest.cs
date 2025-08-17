using NUnit.Framework;

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class ContainerRegistratorTest {
        private interface ITestRegistration {
            
        }

        private class TestRegistrationClass : ITestRegistration {
            
        }
        
        [Test]
        public void TestRegistrationDelegate() {
            var containerRegistrator = new ContainerRegistrator();
            
            containerRegistrator.Register<ITestRegistration, TestRegistrationClass>(_ => new TestRegistrationClass());
            
            Assert.IsTrue(containerRegistrator.Registrations.ContainsKey(typeof(ITestRegistration)));
        }
        
        [Test]
        public void TestRegistrationImplementation() {
            var containerRegistrator = new ContainerRegistrator();
            
            containerRegistrator.Register<ITestRegistration, TestRegistrationClass>();
            
            Assert.IsTrue(containerRegistrator.Registrations.ContainsKey(typeof(ITestRegistration)));
        }
    }
}