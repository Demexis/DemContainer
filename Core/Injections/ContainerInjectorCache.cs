using System;
using System.Collections.Generic;
using System.Reflection;

namespace DemContainer {
    public sealed class ContainerInjectorCache {
        private readonly Dictionary<Type, IEnumerable<FieldInfo>> cachedTypeFieldsInfo = new();
        private readonly Dictionary<Type, IEnumerable<PropertyInfo>> cachedTypePropertiesInfo = new();
        private readonly Dictionary<Type, IEnumerable<MethodInfo>> cachedTypeMethodsInfo = new();

        public IEnumerable<FieldInfo> GetFields(Type type) {
            if (!cachedTypeFieldsInfo.TryGetValue(type, out var fieldInfos)) {
                cachedTypeFieldsInfo[type] = fieldInfos = ReflectionUtils.FindFieldsWithInjectAttribute(type);
            }

            return fieldInfos;
        }
        
        public IEnumerable<PropertyInfo> GetProperties(Type type) {
            if (!cachedTypePropertiesInfo.TryGetValue(type, out var propertyInfos)) {
                cachedTypePropertiesInfo[type] = propertyInfos = ReflectionUtils.FindPropertiesWithInjectAttribute(type);
            }

            return propertyInfos;
        }
        
        public IEnumerable<MethodInfo> GetMethods(Type type) {
            if (!cachedTypeMethodsInfo.TryGetValue(type, out var methodInfos)) {
                cachedTypeMethodsInfo[type] = methodInfos = ReflectionUtils.FindMethodsWithInjectAttribute(type);
            }

            return methodInfos;
        }
    }
}