using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DemContainer {
    public struct ContainerRegistratorInfo {
        public readonly string name;
        public readonly IContainerRegistrator containerRegistrator;

        public ContainerRegistratorInfo(string name, IContainerRegistrator containerRegistrator) {
            this.name = name;
            this.containerRegistrator = containerRegistrator;
        }
    }
    
    public struct ContainerResolverInfo {
        public readonly string name;
        public readonly IContainerResolver containerResolver;
        public readonly IContainerInjector containerInjector;
        public readonly IContainerSubscriptions containerSubscriptions;

        public ContainerResolverInfo(string name, IContainerResolver containerResolver, IContainerInjector containerInjector,
            IContainerSubscriptions containerSubscriptions) {
            this.name = name;
            this.containerResolver = containerResolver;
            this.containerInjector = containerInjector;
            this.containerSubscriptions = containerSubscriptions;
        }
    }
    
    [DefaultExecutionOrder(-5000)]
    public abstract class BaseRootInstaller : MonoBehaviour {
        public const string DEBUG_LOG_PREFIX_OK = "[<color=green>DI</color>] ";
        public const string DEBUG_LOG_PREFIX_ERROR = "[<color=red>DI</color>] ";
        
        public IContainerRegistrator ContainerRegistrator { get; private set; }
        public IContainerResolver ContainerResolver { get; private set; }
        public IContainerInjector ContainerInjector { get; private set; }
        public IContainerSubscriptions ContainerSubscriptions { get; private set; }
        
        public BaseRootInstaller ParentRootInstaller { get => parentRootInstaller; set => parentRootInstaller = value; }
        [SerializeField] private BaseRootInstaller parentRootInstaller;
        public List<BaseChildInstaller> childInstallers = new();
        private SubscribableGate<BaseRootInstaller> awakeGate = new();
        private SubscribableGate<ContainerRegistratorInfo> configuredGate = new();
        private SubscribableGate<ContainerResolverInfo> startedGate = new();
        
        private bool IsChild { get; set; }

        private void Awake() {
            ValidateChildInstallers();

            IsChild = parentRootInstaller != null;
            
            if (IsChild) {
                AwakeChild();
            } else {
                AwakeRoot();
            }
        }

        private void AwakeChild() {
            Debug.Log(DEBUG_LOG_PREFIX_OK + GetType().Name + ": Skipping Configure(), subscribing for parent root-installer.", this);
            parentRootInstaller.awakeGate.Subscribe(ConnectAwake);
            parentRootInstaller.configuredGate.Subscribe(ConnectRegister);
        }

        private void AwakeRoot() {
            ContainerRegistrator = new ContainerRegistrator();
            ContainerResolver = new ContainerResolver(ContainerRegistrator);
            ContainerInjector = new ContainerInjector(ContainerResolver);
            ContainerSubscriptions = new ContainerSubscriptions(ContainerRegistrator, ContainerResolver);

            RegisterContainerTypes();

            ConnectAwake(this);
            
            awakeGate.Open(this);

            using (new OperationTimer("BASE_ROOT_INSTALLER_REGISTERING", OperationTimerDebug.Log)) {
                InvokeRegistration(this, new ContainerRegistratorInfo($"Builder of {GetType().Name}", ContainerRegistrator));
            }
        }
        
        protected virtual void AwakeInherited() { }

        private void Start() {
            if (IsChild) {
                StartChild();
            } else {
                StartRoot();
            }
        }

        private void StartChild() {
            Debug.Log(DEBUG_LOG_PREFIX_OK + GetType().Name + ": Skipping Start(), subscribing for parent root-installer.", this);
            parentRootInstaller.startedGate.Subscribe(ConnectStart);
        }

        private void StartRoot() {
            using (new OperationTimer("BASE_ROOT_INSTALLER_RESOLVING", OperationTimerDebug.Log)) {
                InvokeResolving(this, new ContainerResolverInfo($"Resolver of {GetType().Name}", ContainerResolver, ContainerInjector,
                    ContainerSubscriptions));

                ResolveNonLazy();
            }
            
            Debug.Log("Resolver analyzer result: " + Environment.NewLine + ContainerResolver.OperationsTableAnalyzer, this);
            Debug.Log("Injector analyzer result: " + Environment.NewLine + ContainerInjector.OperationsTableAnalyzer, this);
            Debug.Log("Constructor-injector analyzer result: " + Environment.NewLine + ContainerResolver.Resolve<IConstructorDependencyContainer>().OperationsTableAnalyzer, this);
        }

        private void OnDestroy() {
            if (IsChild) {
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

        private void ValidateChildInstallers() {
            for (var i = 0; i < childInstallers.Count; i++) {
                if (childInstallers[i] == null) {
                    Debug.LogError(DEBUG_LOG_PREFIX_ERROR + GetType().Name + " contains NULL child-installer at index - " + i, this);
                }
            }
        }

        private void RegisterContainerTypes() {
            ContainerRegistrator.Register<IContainerResolver, IContainerResolver>(_ => ContainerResolver);
            ContainerRegistrator.Register<IContainerInjector, IContainerInjector>(_ => ContainerInjector);
            ContainerRegistrator.Register<IContainerSubscriptions, IContainerSubscriptions>(_ => ContainerSubscriptions);
            ContainerRegistrator.Register<IGameObjectFactory, GameObjectFactory>();
            ContainerRegistrator.Register<IUnityObjectFactory, UnityObjectFactory>();
            ContainerRegistrator.Register<IObjectFactory, ObjectFactory>();
            ContainerRegistrator.Register<IConstructorDependencyContainer, ConstructorDependencyContainer>(_ =>
                    new ConstructorDependencyContainer(ContainerResolver));
        }

        private void ConnectAwake(BaseRootInstaller parentRootInstaller) {
            ContainerRegistrator = parentRootInstaller.ContainerRegistrator;
            ContainerResolver = parentRootInstaller.ContainerResolver;
            ContainerInjector = parentRootInstaller.ContainerInjector;
            ContainerSubscriptions = parentRootInstaller.ContainerSubscriptions;
            
            AwakeInherited();
        }

        private void ConnectRegister(ContainerRegistratorInfo registrator) {
            InvokeRegistration(this, registrator);
        }

        private void ConnectStart(ContainerResolverInfo containerResolverInfo) {
            InvokeResolving(this, containerResolverInfo);
        }
        
        private static void InvokeRegistration(BaseRootInstaller rootInstaller, ContainerRegistratorInfo containerRegistratorInfo) {
            Debug.Log(DEBUG_LOG_PREFIX_OK
                + $"Configuring {rootInstaller.GetType().Name} with container builder named - {containerRegistratorInfo.name}", rootInstaller);

            Debug.Log(DEBUG_LOG_PREFIX_OK + "Registering child installers: "
                + string.Join(", ", rootInstaller.childInstallers.Select(x => x.GetType().Name)), rootInstaller);

            foreach (var installer in rootInstaller.childInstallers) {
                installer.Register(containerRegistratorInfo.containerRegistrator);
            }
            
            rootInstaller.RegisterInherited();

            rootInstaller.configuredGate.Open(containerRegistratorInfo);
        }
        
        protected virtual void RegisterInherited(){ }

        private static void InvokeResolving(BaseRootInstaller rootInstaller, ContainerResolverInfo containerResolverInfo) {
            Debug.Log(DEBUG_LOG_PREFIX_OK
                + $"Resolving {rootInstaller.GetType().Name} with container resolver named - {containerResolverInfo.name}", rootInstaller);

            Debug.Log(DEBUG_LOG_PREFIX_OK + "Installing child installers: "
                + string.Join(", ", rootInstaller.childInstallers.Select(x => x.GetType().Name)), rootInstaller);

            using (new OperationTimer("ROOT_INSTALLER_RESOLVING-"+rootInstaller.GetType().Name, OperationTimerDebug.Log)) {
                foreach (var installer in rootInstaller.childInstallers) {
                    installer.Resolve(containerResolverInfo.containerResolver, containerResolverInfo.containerInjector,
                        containerResolverInfo.containerSubscriptions);
                }
                
                rootInstaller.ResolveInherited();
            }
            
            rootInstaller.startedGate.Open(containerResolverInfo);
        }

        protected virtual void ResolveInherited() { }

        /// <summary>
        /// Resolve all left unresolved types with the flag 'NonLazy' set.
        /// </summary>
        private void ResolveNonLazy() {
            var installedTypes = ContainerRegistrator.Registrations
                                                     .Where(x =>
                                                         ContainerResolver.ResolvedSingletonTypes.ContainsKey(x.Key))
                                                     .Select(x => x.Key.Name);
            Debug.Log(DEBUG_LOG_PREFIX_OK + "Already installed types on non-lazy resolving: " + string.Join(", ", installedTypes), this);
            
            var notInstalledNonLazyTypes = ContainerRegistrator.Registrations
                                                     .Where(x =>
                                                         !x.Value.Lazy &&
                                                         !ContainerResolver.ResolvedSingletonTypes.ContainsKey(x.Key))
                                                     .Select(x => x.Key.Name);
            Debug.Log(DEBUG_LOG_PREFIX_OK + "Not installed non-lazy types on non-lazy resolving: " + string.Join(", ", notInstalledNonLazyTypes), this);
            
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
    }
}