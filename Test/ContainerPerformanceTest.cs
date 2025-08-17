using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = System.Object;
// ReSharper disable UnusedVariable
// ReSharper disable NotAccessedVariable
// ReSharper disable RedundantAssignment

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class ContainerPerformanceTest {
        private struct PerformanceTestStruct {
            
        } 
        
        [Test]
        public void TestGetMethodsCachePerformance() {
            const int ITERATIONS_COUNT = 10000;
            var containerInjectorCache = new ContainerInjectorCache();

            containerInjectorCache.GetMethods(typeof(Object));
            containerInjectorCache.GetMethods(typeof(ValueType));
            containerInjectorCache.GetMethods(typeof(Int32));
            containerInjectorCache.GetMethods(typeof(Single));
            
            double cacheTimeMs = 0;
            double getMethodsTimeMs = 0;

            var methods = (IEnumerable<MethodInfo>)Array.Empty<MethodInfo>();
            var searchedType = typeof(PerformanceTestStruct);
            
            using (var timer = new OperationTimer("TEST_CACHE_TIME", result => {
                       cacheTimeMs = result.elapsed.TotalMilliseconds;
                   }, true)) {
                for (var i = 0; i < ITERATIONS_COUNT; i++) {
                    methods = containerInjectorCache.GetMethods(searchedType);
                }
            }
            
            using (var timer = new OperationTimer("TEST_GET_METHODS_TIME", result => {
                       getMethodsTimeMs = result.elapsed.TotalMilliseconds;
                   }, true)) {
                for (var i = 0; i < ITERATIONS_COUNT; i++) {
                    methods = searchedType.GetMethods();
                }
            }
            
            Debug.Log("Iterations count: " + ITERATIONS_COUNT);
            Debug.Log("Cache time (ms): " + cacheTimeMs);
            Debug.Log("GetMethods() time (ms): " + getMethodsTimeMs);
            
            Assert.LessOrEqual(cacheTimeMs, getMethodsTimeMs);
        } 
    }
}