using NUnit.Framework;

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class UtilsTest {
        private class TestMemberHasInjectAttributeClass {
            [Inject]
            public void InjectMethod() {
                
            }
        }
        
        [Test]
        public void TestMemberHasInjectAttribute() {
            var member = typeof(TestMemberHasInjectAttributeClass)
                .GetMethod(nameof(TestMemberHasInjectAttributeClass.InjectMethod));
            
            var result = ReflectionUtils.MemberHasInjectAttribute(member);
            
            Assert.IsTrue(result);
        }
        
        [Test]
        public void TestDefaultImplementationConstructor() {
            var implementationConstructor = ReflectionUtils.GetImplementationConstructor(typeof(TestMemberHasInjectAttributeClass));
            
            Assert.IsNotNull(implementationConstructor);
            Assert.Zero(implementationConstructor.GetParameters().Length);
        }
    }
}