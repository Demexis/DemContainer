using NUnit.Framework;

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class ContainerInjectorTest {
        private class TestInjectionFieldClass {
            
        }
        
        private class TestInjectionFieldClass2 {
            
        }
        
        private class TestInjectionParameterClass {
            
        }
        
        private class TestInjectionParameterClass2 {
            
        }
        
        private class TestInjectionClass {
            public TestInjectionFieldClass TestInjectionFieldClass => testInjectionFieldClass;
            [Inject] private TestInjectionFieldClass testInjectionFieldClass;
            
            public TestInjectionParameterClass TestInjectionParameterClass { get; private set; }
            
            [Inject]
            private void Construct(TestInjectionParameterClass testInjectionParameterClass) {
                TestInjectionParameterClass = testInjectionParameterClass;
            }
        }

        private class TestInjectionExtendedClass : TestInjectionClass {
            public TestInjectionFieldClass2 TestInjectionFieldClass2 => testInjectionFieldClass2;
            [Inject] private TestInjectionFieldClass2 testInjectionFieldClass2;
            
            public TestInjectionParameterClass2 TestInjectionParameterClass2 { get; private set; }
            
            [Inject]
            private void Construct(TestInjectionParameterClass2 testInjectionParameterClass) {
                TestInjectionParameterClass2 = testInjectionParameterClass;
            }
        }
        
        [Test]
        public void TestInjection() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            var containerInjector = new ContainerInjector(containerResolver);

            var testInjectionFieldClass = new TestInjectionFieldClass();
            var testInjectionParameterClass = new TestInjectionParameterClass();
            var testInjectionClass = new TestInjectionClass();
            
            containerRegistrator.Register<TestInjectionFieldClass, TestInjectionFieldClass>(_ => testInjectionFieldClass);
            containerRegistrator.Register<TestInjectionParameterClass, TestInjectionParameterClass>(_ => testInjectionParameterClass);
            containerInjector.Inject(testInjectionClass);
            
            Assert.IsNotNull(testInjectionClass.TestInjectionParameterClass);
            Assert.AreEqual(testInjectionParameterClass, testInjectionClass.TestInjectionParameterClass);
            
            Assert.IsNotNull(testInjectionClass.TestInjectionFieldClass);
            Assert.AreEqual(testInjectionFieldClass, testInjectionClass.TestInjectionFieldClass);
        }

        [Test]
        public void TestExtendedInjection() {
            var containerRegistrator = new ContainerRegistrator();
            var containerResolver = new ContainerResolver(containerRegistrator);
            var containerInjector = new ContainerInjector(containerResolver);

            var testInjectionFieldClass = new TestInjectionFieldClass();
            var testInjectionFieldClass2 = new TestInjectionFieldClass2();
            var testInjectionParameterClass = new TestInjectionParameterClass();
            var testInjectionParameterClass2 = new TestInjectionParameterClass2();
            
            var testInjectionExtendedClass = new TestInjectionExtendedClass();
            
            containerRegistrator.Register<TestInjectionFieldClass, TestInjectionFieldClass>(_ => testInjectionFieldClass);
            containerRegistrator.Register<TestInjectionFieldClass2, TestInjectionFieldClass2>(_ => testInjectionFieldClass2);
            containerRegistrator.Register<TestInjectionParameterClass, TestInjectionParameterClass>(_ => testInjectionParameterClass);
            containerRegistrator.Register<TestInjectionParameterClass2, TestInjectionParameterClass2>(_ => testInjectionParameterClass2);
            containerInjector.Inject(testInjectionExtendedClass);
            
            Assert.IsNotNull(testInjectionExtendedClass.TestInjectionParameterClass2);
            Assert.AreEqual(testInjectionParameterClass2, testInjectionExtendedClass.TestInjectionParameterClass2);
            
            Assert.IsNotNull(testInjectionExtendedClass.TestInjectionFieldClass2);
            Assert.AreEqual(testInjectionFieldClass2, testInjectionExtendedClass.TestInjectionFieldClass2);
            
            Assert.IsNotNull(testInjectionExtendedClass.TestInjectionParameterClass);
            Assert.AreEqual(testInjectionParameterClass, testInjectionExtendedClass.TestInjectionParameterClass);
            
            Assert.IsNotNull(testInjectionExtendedClass.TestInjectionFieldClass);
            Assert.AreEqual(testInjectionFieldClass, testInjectionExtendedClass.TestInjectionFieldClass);
        }
    }
}