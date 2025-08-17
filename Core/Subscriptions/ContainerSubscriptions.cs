using System;
using System.Collections.Generic;

namespace DemContainer {
    public interface IContainerSubscriptions : IDisposable {
        /// <summary>
        /// All subscriptions by type.
        /// </summary>
        IReadOnlyDictionary<Type, List<ActionSubscriptionRecord>> ActionSubscriptions { get; }
        
        /// <summary>
        /// Subscribes callback to be invoked by all current and future registered types assignable to type T.
        /// </summary>
        /// <param name="callback">Callback.</param>
        /// <typeparam name="T">Assignable type.</typeparam>
        void Subscribe<T>(Action<T> callback);
    }
    
    public sealed class ContainerSubscriptions : IContainerSubscriptions {
        private readonly IContainerRegistrator containerRegistrator;
        private readonly IContainerResolver containerResolver;

        public IReadOnlyDictionary<Type, List<ActionSubscriptionRecord>> ActionSubscriptions => actionSubscriptions;
        private readonly Dictionary<Type, List<ActionSubscriptionRecord>> actionSubscriptions = new();

        public ContainerSubscriptions(IContainerRegistrator containerRegistrator, IContainerResolver containerResolver) {
            this.containerRegistrator = containerRegistrator;
            this.containerResolver = containerResolver;

            containerRegistrator.AddedRegistration += CheckAddedRegistration;
        }

        public void Dispose() {
            containerRegistrator.AddedRegistration -= CheckAddedRegistration;
        }

        public void Subscribe<T>(Action<T> callback) {
            var type = typeof(T);
            var typeObjects = containerResolver.ResolveDeepAll<T>();

            foreach (var typeObject in typeObjects) {
                callback.Invoke(typeObject);
            }

            if (!actionSubscriptions.TryGetValue(type, out var actionSubscriptionRecords)) {
                actionSubscriptions[type] = actionSubscriptionRecords = new List<ActionSubscriptionRecord>();
            }
            
            actionSubscriptionRecords.Add(new ActionSubscriptionRecord(type, o => callback((T)o)));
        }

        private void CheckAddedRegistration(FuncRegistrationRecord funcRegistrationRecord) {
            if (!CheckDeepType(funcRegistrationRecord.interfaceType)) {
                CheckDeepType(funcRegistrationRecord.implementationType);
            }
            
            bool CheckDeepType(Type registrationType) {
                var baseType = registrationType.BaseType;
                var interfaceTypes = registrationType.GetInterfaces();

                if (CheckType(registrationType)) {
                    return true;
                }
                
                if (baseType != null) {
                    if (CheckType(baseType)) {
                        return true;
                    }
                }
                
                foreach (var interfaceType in interfaceTypes) {
                    if (CheckType(interfaceType)) {
                        return true;
                    }
                }

                return false;
                
                bool CheckType(Type type) {
                    if (!actionSubscriptions.TryGetValue(type, out var actionSubscriptionRecords)) {
                        return false;
                    }

                    foreach (var actionSubscriptionRecord in actionSubscriptionRecords) {
                        var registrationObject = containerResolver.Resolve(funcRegistrationRecord.interfaceType);
                        
                        actionSubscriptionRecord.callback.Invoke(registrationObject);
                    }

                    return true;
                }
            }
        }
    }
}