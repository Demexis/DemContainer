using System;

namespace DemContainer {
    public sealed class FuncRegistrationRecord {
        public readonly Type interfaceType;
        public readonly Type implementationType;
        public readonly Func<IContainerResolver, object> callback;
        
        public bool Lazy { get; set; }

        public FuncRegistrationRecord(Type interfaceType, Type implementationType, Func<IContainerResolver, object> callback) {
            this.interfaceType = interfaceType;
            this.implementationType = implementationType;
            this.callback = callback;
        }
    }
}