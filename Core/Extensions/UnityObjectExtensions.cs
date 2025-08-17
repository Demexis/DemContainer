using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace DemContainer {
    public static class UnityObjectExtensions {
        [Pure]
        public static List<T> GetSerializeReferences<T>(this Object obj) {
            var allReferences = ManagedReferenceUtility.GetManagedReferenceIds(obj);
            if (allReferences == null || allReferences.Length == 0) {
                return Enumerable.Empty<T>().ToList();
            }
            
            var serializeReferences = new List<T>(allReferences.Length);
            foreach (var id in allReferences) {
                if (id is ManagedReferenceUtility.RefIdNull or ManagedReferenceUtility.RefIdUnknown) {
                    continue;
                }
                
                var serializeReference = ManagedReferenceUtility.GetManagedReference(obj, id);
                if (serializeReference is T castedReference) {
                    serializeReferences.Add(castedReference);
                }
            }
            
            return serializeReferences;
        }
        
        [Pure]
        public static List<object> GetSerializeReferences(this Object obj) => GetSerializeReferences<object>(obj);
    }
}