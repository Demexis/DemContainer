using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DemContainer.Tests {
    [TestFixture]
    internal sealed class UnityObjectExtensionsTest {
        public interface IData { }
        
        [Serializable]
        public class Data : IData {
            public int id;
            [SerializeReference] public List<InnerData> innerData;
        }
        
        [Serializable]
        public class InnerData : IData {
            public char character;
        }
        
        public class TestUnityObjectWithSerializeReferences : ScriptableObject {
            [SerializeReference] public IData singleData;
            [SerializeReference] public List<IData> multipleData;
            
            public void FillData() {
                singleData = new Data() {
                    id = 1,
                    innerData = new List<InnerData>() {
                        new InnerData() { character = 'a' },
                        new InnerData() { character = 'b' }
                    }
                };
                multipleData = new List<IData>() {
                    new Data() {
                        id = 2,
                        innerData = new List<InnerData>() {
                            new InnerData() { character = 'c' }
                        }
                    },
                    new Data() {
                        id = 3,
                        innerData = new List<InnerData>() {
                            new InnerData() { character = 'd' }
                        }
                    }
                };
            }
        }
        
        [Test]
        public void TestGetSerializeReferences() {
            var unityObject = ScriptableObject.CreateInstance<TestUnityObjectWithSerializeReferences>();
            unityObject.FillData();
            
            var serializeReferences = unityObject.GetSerializeReferences();
            Assert.IsTrue(serializeReferences.Count == 7);
            
            var dataSerializeReferences = unityObject.GetSerializeReferences<Data>();
            Assert.IsTrue(dataSerializeReferences.Count == 3);
            
            var innerDataSerializeReferences = unityObject.GetSerializeReferences<InnerData>();
            Assert.IsTrue(innerDataSerializeReferences.Count == 4);
        }
    }
}