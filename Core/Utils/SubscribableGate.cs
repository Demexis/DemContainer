using System;
using System.Collections.Generic;

namespace DemContainer {
    internal sealed class SubscribableGate {
        private readonly List<Action> subscribers = new();

        public bool IsOpened { get; private set; }

        public void Subscribe(Action callback) {
            if (IsOpened) {
                callback.Invoke();
                return;
            }
            
            subscribers.Add(callback);
        }
        
        public void Open() {
            if (IsOpened) {
                return;
            }
            
            IsOpened = true;
            InvokeAllSubscribers();
        }

        public void Close() {
            IsOpened = false;
        }

        private void InvokeAllSubscribers() {
            foreach (var subscriber in subscribers) {
                subscriber.Invoke();
            }
            
            subscribers.Clear();
        }
    }
    
    internal sealed class SubscribableGate<T> {
        private readonly List<Action<T>> subscribers = new();

        public bool IsOpened { get; private set; }
        public T OpenedValue { get; private set; }

        public void Subscribe(Action<T> callback) {
            if (IsOpened) {
                callback.Invoke(OpenedValue);
                return;
            }
            
            subscribers.Add(callback);
        }
        
        public void Open(T value) {
            if (IsOpened) {
                return;
            }

            OpenedValue = value;
            IsOpened = true;
            InvokeAllSubscribers();
        }

        public void Close() {
            IsOpened = false;
        }

        private void InvokeAllSubscribers() {
            foreach (var subscriber in subscribers) {
                subscriber.Invoke(OpenedValue);
            }
            
            subscribers.Clear();
        }
    }
}