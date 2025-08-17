using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DemContainer {
    public static class ReflectionUtils {
        public static ConstructorInfo GetImplementationConstructor(Type type) {
            var constructors = type.GetConstructors();

            if (constructors.Length == 0) {
                throw new Exception($"Type {type.Name} has no valid constructor.");
            }
            
            foreach (var constructor in constructors) {
                if (MemberHasInjectAttribute(constructor)) {
                    return constructor;
                }
            }

            return constructors[0];
        }
        
        public static IEnumerable<MethodInfo> FindMethodsWithInjectAttribute(Type type) {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance).Where(MemberHasInjectAttribute);
        }
        
        public static IEnumerable<FieldInfo> FindFieldsWithInjectAttribute(Type type) {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance).Where(MemberHasInjectAttribute);
        }
        
        public static IEnumerable<PropertyInfo> FindPropertiesWithInjectAttribute(Type type) {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance).Where(MemberHasInjectAttribute);
        }

        public static bool MemberHasInjectAttribute(MemberInfo memberInfo) {
            return memberInfo.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0;
        }
    }
}