using System;
using System.Collections.Generic;
using System.Linq;

namespace DemContainer {
    public interface IContainerRegistrator {
        /// <summary>
        /// Is invoked whenever new registration is added.
        /// </summary>
        event Action<FuncRegistrationRecord> AddedRegistration;

        /// <summary>
        /// All registrations by type.
        /// </summary>
        IReadOnlyDictionary<Type, FuncRegistrationRecord> Registrations { get; }

        /// <summary>
        /// Registers object assignable to type TInterface, which is returned by delegate.
        /// </summary>
        /// <param name="resolvingCallback">Resolving delegate.</param>
        /// <typeparam name="TInterface">Registration type.</typeparam>
        /// <typeparam name="TImplementation">Type with implementation (constructor).</typeparam>
        RegistrationBuilder Register<TInterface, TImplementation>(Func<IContainerResolver, TImplementation> resolvingCallback) where TImplementation : TInterface;

        /// <summary>
        /// Registers object assignable to type TInterface, which is returned by implementation.
        /// The constructor with [Inject] attribute has greater priority.
        /// </summary>
        /// <typeparam name="TInterface">Registration type.</typeparam>
        /// <typeparam name="TImplementation">Type with implementation (constructor).</typeparam>
        RegistrationBuilder Register<TInterface, TImplementation>() where TImplementation : TInterface;

        /// <summary>
        /// Gets registrations from given registrator and sets them into this instance.
        /// </summary>
        /// <param name="copyRegistrator">Registrator from which the registrations will be copied.</param>
        void CopyRegistrationsFrom(IContainerRegistrator copyRegistrator);
    }

    public sealed class ContainerRegistrator : IContainerRegistrator {
        public event Action<FuncRegistrationRecord> AddedRegistration = delegate { };

        public IReadOnlyDictionary<Type, FuncRegistrationRecord> Registrations => registrations;
        private readonly Dictionary<Type, FuncRegistrationRecord> registrations = new();

        /// <summary>
        /// Registers down-casted resolved callback to registration type.
        /// </summary>
        /// <param name="funcRegistrationRecord">Registration record.</param>
        /// <exception cref="Exception">Does not support registration of generic types.
        /// Throws exception on registration of already registered type.</exception>
        private RegistrationBuilder Register(FuncRegistrationRecord funcRegistrationRecord) {
            // TODO: Add generic types support
            if (funcRegistrationRecord.interfaceType.IsGenericType) {
                throw new Exception("Generic types are not supported.");
            }

            if (!registrations.TryAdd(funcRegistrationRecord.interfaceType, funcRegistrationRecord)) {
                throw new Exception("Container already has registration for type "
                    + funcRegistrationRecord.interfaceType.Name);
            }
            
            AddedRegistration.Invoke(funcRegistrationRecord);

            return new RegistrationBuilder(this, funcRegistrationRecord);
        }

        public RegistrationBuilder Register<TInterface, TImplementation>(Func<IContainerResolver, TImplementation> resolvingCallback) where TImplementation : TInterface {
            var interfaceType = typeof(TInterface);
            var implementationType = typeof(TImplementation);
            var funcRegistrationRecord =
                new FuncRegistrationRecord(interfaceType, implementationType, resolver => resolvingCallback(resolver));

            return Register(funcRegistrationRecord);
        }

        public RegistrationBuilder Register<TInterface, TImplementation>() where TImplementation : TInterface {
            var constructor = ReflectionUtils.GetImplementationConstructor(typeof(TImplementation));

            var parameters = constructor.GetParameters();

            var interfaceType = typeof(TInterface);
            var implementationType = typeof(TImplementation);
            var funcRegistrationRecord =
                new FuncRegistrationRecord(interfaceType, implementationType, resolver => {
                    var args = parameters.Select(x => resolver.Resolve(x.ParameterType));
                    return constructor.Invoke(args.ToArray());
                });

            return Register(funcRegistrationRecord);
        }

        public void CopyRegistrationsFrom(IContainerRegistrator copyRegistrator) {
            foreach (var (type, funcRegistrationRecord) in copyRegistrator.Registrations) {
                registrations[type] = funcRegistrationRecord;
            }
        }
    }
}