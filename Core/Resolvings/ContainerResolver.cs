using System;
using System.Collections.Generic;
using System.Linq;

namespace DemContainer {
    public interface IContainerResolver {
        /// <summary>
        /// Dictionary of resolved types (cache).
        /// </summary>
        IReadOnlyDictionary<Type, object> ResolvedSingletonTypes { get; }
        
        OperationsTableAnalyzer OperationsTableAnalyzer { get; }

        /// <summary>
        /// Resolves registration of a given parameter type. Returns object from cache if contains.
        /// </summary>
        /// <typeparam name="T">Parameter type.</typeparam>
        /// <returns>Resolved object of type T.</returns>
        T Resolve<T>();

        /// <summary>
        /// Resolves registration of a given parameter type. Returns object from cache if contains.
        /// </summary>
        /// <param name="type">Parameter type.</param>
        /// <returns>Resolved object.</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolves all registrations where their object type is assignable to a given parameter type.
        /// </summary>
        /// <typeparam name="T">Parameter type.</typeparam>
        /// <returns>List of resolved objects casted to T.</returns>
        List<T> ResolveDeepAll<T>();

        /// <summary>
        /// Resolves all registrations where their object type is assignable to a given parameter type.
        /// </summary>
        /// <param name="type">Parameter type.</param>
        /// <returns>List of resolved objects.</returns>
        List<object> ResolveDeepAll(Type type);

        /// <summary>
        /// Gets resolvings from given resolver and sets them into this instance.
        /// </summary>
        /// <param name="containerResolver">Resolver from which the resolvings will be copied.</param>
        void CopyResolvingsFrom(IContainerResolver containerResolver);
    }

    public sealed class ContainerResolver : IContainerResolver {
        public OperationsTableAnalyzer OperationsTableAnalyzer { get; } = new();
        
        private readonly IContainerRegistrator containerRegistrator;

        public IReadOnlyDictionary<Type, object> ResolvedSingletonTypes => resolvedSingletonTypes;
        private readonly Dictionary<Type, object> resolvedSingletonTypes = new();

        private readonly List<Type> currentlyResolvingSingletonTypes = new();

        public ContainerResolver(IContainerRegistrator containerRegistrator) {
            this.containerRegistrator = containerRegistrator;
        }

        public T Resolve<T>() {
            var type = typeof(T);
            return (T)Resolve(type);
        }

        public object Resolve(Type type) {
            if (!containerRegistrator.Registrations.TryGetValue(type, out var registrationRecord)) {
                throw new Exception($"Type {type.Name} is not registered.");
            }

            var typeObject = ResolveRegistrationIfNotCached(registrationRecord);

            return typeObject;
        }

        public List<T> ResolveDeepAll<T>() {
            var type = typeof(T);
            return new List<T>(ResolveDeepAll(type).Cast<T>());
        }

        public List<object> ResolveDeepAll(Type type) {
            var typeObjects = new List<object>();

            foreach (var (_, registrationRecord) in containerRegistrator.Registrations) {
                if (!type.IsAssignableFrom(registrationRecord.interfaceType)
                    && !type.IsAssignableFrom(registrationRecord.implementationType)) {
                    continue;
                }

                var typeObject = ResolveRegistrationIfNotCached(registrationRecord);

                typeObjects.Add(typeObject);
            }

            return typeObjects;
        }

        public void CopyResolvingsFrom(IContainerResolver containerResolver) {
            foreach (var (type, value) in containerResolver.ResolvedSingletonTypes) {
                resolvedSingletonTypes[type] = value;
            }
        }

        /// <summary>
        /// Resolves registration record or returns result from cache if contains.
        /// </summary>
        /// <param name="funcRegistrationRecord">Registration record.</param>
        /// <returns>Object.</returns>
        private object ResolveRegistrationIfNotCached(FuncRegistrationRecord funcRegistrationRecord) {
            var type = funcRegistrationRecord.interfaceType;

            if (!resolvedSingletonTypes.TryGetValue(type, out var typeObject)) {
                using (new OperationTimer("Resolving expensive type: " + type.Name,
                           result => {
                               OperationTimerDebug.LogThreshold(result, TimeSpan.FromMilliseconds(100));
                               OperationsTableAnalyzer.AddNewRecord(type.Name, result.elapsed);
                           })) {
                    if (currentlyResolvingSingletonTypes.Contains(type)) {
                        throw new Exception($"Type {type.Name} is trying to resolve twice. Possible two-way injection.");
                    }
                    
                    currentlyResolvingSingletonTypes.Add(type);
                    resolvedSingletonTypes[type] = typeObject = funcRegistrationRecord.callback.Invoke(this);
                    currentlyResolvingSingletonTypes.Remove(type);
                }
            }

            return typeObject;
        }
    }
}