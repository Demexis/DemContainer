using System;

namespace DemContainer {
    public sealed class ActionSubscriptionRecord {
        public readonly Type type;
        public readonly Action<object> callback;

        public ActionSubscriptionRecord(Type type, Action<object> callback) {
            this.type = type;
            this.callback = callback;
        }
    }
}