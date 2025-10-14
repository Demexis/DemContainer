using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DemContainer {
    [DefaultExecutionOrder(EXECUTION_ORDER)]
    public abstract class BaseRootInstaller : MonoBehaviour {
        public const int EXECUTION_ORDER = -5000;

        public const string DEBUG_LOG_PREFIX_OK = "[<color=green>DI</color>] ";
        public const string DEBUG_LOG_PREFIX_ERROR = "[<color=red>DI</color>] ";

        public IContainerRegistrator ContainerRegistrator { get; private set; }
        public IContainerResolver ContainerResolver { get; private set; }
        public IContainerInjector ContainerInjector { get; private set; }
        public IContainerSubscriptions ContainerSubscriptions { get; private set; }

        [field: SerializeField] public bool IsStartingPoint { get; set; }
        [field: SerializeField] public BaseRootInstaller NextInstaller { get; set; }

        public List<BaseChildInstaller> childInstallers = new();

        private void Awake() {
            if (IsStartingPoint) {
                RegisterAsRoot();
                ResolveAsRoot();
            }
        }

        private void RegisterAsRoot() {
            ContainerRegistrator = new ContainerRegistrator();
            ContainerResolver = new ContainerResolver(ContainerRegistrator);
            ContainerInjector = new ContainerInjector(ContainerResolver);
            ContainerSubscriptions = new ContainerSubscriptions(ContainerRegistrator, ContainerResolver);

            RegisterContainerTypes();

            using (new OperationTimer("BASE_ROOT_INSTALLER_REGISTERING", OperationTimerDebug.Log)) {
                RegisterThis();

                if (NextInstaller != null) {
                    NextInstaller.RegisterAsChild(this);
                }
            }
            return;

            void RegisterContainerTypes() {
                ContainerRegistrator.Register<IContainerResolver, IContainerResolver>(_ => ContainerResolver);
                ContainerRegistrator.Register<IContainerInjector, IContainerInjector>(_ => ContainerInjector);
                ContainerRegistrator.Register<IContainerSubscriptions, IContainerSubscriptions>(_ =>
                    ContainerSubscriptions);
                ContainerRegistrator.Register<IGameObjectFactory, GameObjectFactory>();
                ContainerRegistrator.Register<IUnityObjectFactory, UnityObjectFactory>();
                ContainerRegistrator.Register<IObjectFactory, ObjectFactory>();
                ContainerRegistrator.Register<IConstructorDependencyContainer, ConstructorDependencyContainer>(_ =>
                    new ConstructorDependencyContainer(ContainerResolver));
            }
        }

        private void RegisterAsChild(BaseRootInstaller parentInstaller) {
            ConnectToParentContainer(parentInstaller);
            RegisterThis();
            
            if (NextInstaller != null) {
                NextInstaller.RegisterAsChild(this);
            }
            return;

            void ConnectToParentContainer(BaseRootInstaller parentRootInstaller) {
                ContainerRegistrator = parentRootInstaller.ContainerRegistrator;
                ContainerResolver = parentRootInstaller.ContainerResolver;
                ContainerInjector = parentRootInstaller.ContainerInjector;
                ContainerSubscriptions = parentRootInstaller.ContainerSubscriptions;
            }
        }

        private void RegisterThis() {
            ValidateChildInstallers();

            Debug.Log(DEBUG_LOG_PREFIX_OK + $"Configuring {GetType().Name}", this);

            Debug.Log(DEBUG_LOG_PREFIX_OK
                + "Registering child installers: "
                + string.Join(", ", childInstallers.Select(x => x.GetType().Name)), this);

            foreach (var installer in childInstallers) {
                installer.Register(ContainerRegistrator);
            }

            RegisterInherited();
            return;

            void ValidateChildInstallers() {
                for (var i = 0; i < childInstallers.Count; i++) {
                    if (childInstallers[i] == null) {
                        Debug.LogError(
                            DEBUG_LOG_PREFIX_ERROR + GetType().Name + " contains NULL child-installer at index - " + i,
                            this);
                    }
                }
            }
        }

        protected virtual void RegisterInherited() { }

        private void ResolveAsRoot() {
            using (new OperationTimer("BASE_ROOT_INSTALLER_RESOLVING", OperationTimerDebug.Log)) {
                ResolveThis();
                if (NextInstaller != null) {
                    NextInstaller.ResolveAsChild();
                }
                ResolveNonLazy();
            }

            Debug.Log("Resolver analyzer result: " + Environment.NewLine + ContainerResolver.OperationsTableAnalyzer,
                this);
            Debug.Log("Injector analyzer result: " + Environment.NewLine + ContainerInjector.OperationsTableAnalyzer,
                this);
            Debug.Log(
                "Constructor-injector analyzer result: "
                + Environment.NewLine
                + ContainerResolver.Resolve<IConstructorDependencyContainer>().OperationsTableAnalyzer, this);
        }

        private void ResolveAsChild() {
            ResolveThis();
            if (NextInstaller != null) {
                NextInstaller.ResolveAsChild();
            }
        }

        private void ResolveThis() {
            Debug.Log(DEBUG_LOG_PREFIX_OK
                + $"Resolving {GetType().Name}", this);

            Debug.Log(DEBUG_LOG_PREFIX_OK
                + "Installing child installers: "
                + string.Join(", ", childInstallers.Select(x => x.GetType().Name)), this);

            foreach (var installer in childInstallers) {
                installer.Resolve(ContainerResolver, ContainerInjector, ContainerSubscriptions);
            }

            ResolveInherited();
        }

        /// <summary>
        /// Resolve all left unresolved types with the flag 'NonLazy' set.
        /// </summary>
        private void ResolveNonLazy() {
            var installedTypes = ContainerRegistrator.Registrations
                                                     .Where(x =>
                                                         ContainerResolver.ResolvedSingletonTypes.ContainsKey(x.Key))
                                                     .Select(x => x.Key.Name);
            Debug.Log(
                DEBUG_LOG_PREFIX_OK
                + "Already installed types on non-lazy resolving: "
                + string.Join(", ", installedTypes), this);

            var notInstalledNonLazyTypes = ContainerRegistrator.Registrations
                                                               .Where(x =>
                                                                   !x.Value.Lazy
                                                                   && !ContainerResolver.ResolvedSingletonTypes
                                                                       .ContainsKey(x.Key))
                                                               .Select(x => x.Key.Name);
            Debug.Log(
                DEBUG_LOG_PREFIX_OK
                + "Not installed non-lazy types on non-lazy resolving: "
                + string.Join(", ", notInstalledNonLazyTypes), this);

            foreach (var (type, registrationRecord) in ContainerRegistrator.Registrations) {
                if (registrationRecord.Lazy) {
                    continue;
                }

                if (ContainerResolver.ResolvedSingletonTypes.ContainsKey(type)) {
                    continue;
                }

                ContainerResolver.Resolve(type);
            }
        }

        protected virtual void ResolveInherited() { }

        private void OnDestroy() {
            if (!IsStartingPoint) {
                return;
            }

            ContainerSubscriptions?.Dispose();

            if (ContainerRegistrator != null) {
                foreach (var (type, _) in ContainerRegistrator.Registrations) {
                    if (StaticInstallments.ContainerRegistrator.Registrations.ContainsKey(type)) {
                        continue;
                    }

                    if (!ContainerResolver.ResolvedSingletonTypes.TryGetValue(type, out var resolvedObject)) {
                        continue;
                    }

                    if (resolvedObject is IDisposable disposableObject) {
                        disposableObject.Dispose();
                    }
                }
            }
        }
    }
}