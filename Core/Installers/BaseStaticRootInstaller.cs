using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DemContainer {
    public abstract class BaseStaticRootInstaller : BaseRootInstaller {
        public List<BaseStaticChildInstaller> staticChildInstallers = new();
        
        public IStaticContainerRegistrator StaticContainerRegistrator { get; private set; }
        public IStaticContainerResolver StaticContainerResolver { get; private set; }
        
        protected override void RegisterInherited() {
            StaticContainerRegistrator = new StaticContainerRegistrator(StaticInstallments.ContainerRegistrator);
            StaticContainerResolver = new StaticContainerResolver(StaticInstallments.ContainerResolver);
            
            Debug.Log(DEBUG_LOG_PREFIX_OK + "Registering static child installers: "
                + string.Join(", ", staticChildInstallers.Select(x => x.GetType().Name)));
            
            foreach (var staticChildInstaller in staticChildInstallers) {
                staticChildInstaller.Register(StaticContainerRegistrator);
            }
            
            ContainerRegistrator.CopyRegistrationsFrom(StaticContainerRegistrator.ContainerRegistrator);
        }

        protected override void ResolveInherited() {
            Debug.Log(DEBUG_LOG_PREFIX_OK + "Installing static child installers: "
                + string.Join(", ", staticChildInstallers.Select(x => x.GetType().Name)));
            
            foreach (var staticChildInstaller in staticChildInstallers) {
                staticChildInstaller.Resolve(StaticContainerResolver);
            }
            
            var installedTypes = StaticContainerRegistrator.ContainerRegistrator.Registrations
                                                           .Where(x =>
                                                               StaticContainerResolver.ContainerResolver.ResolvedSingletonTypes.ContainsKey(x.Key))
                                                           .Select(x => x.Key.Name);
            Debug.Log(DEBUG_LOG_PREFIX_OK + "Already installed static types on resolving: " + string.Join(", ", installedTypes));
            
            var notInstalledNonLazyTypes = StaticContainerRegistrator.ContainerRegistrator.Registrations
                                                               .Where(x =>
                                                                   !x.Value.Lazy &&
                                                                   !StaticContainerResolver.ContainerResolver.ResolvedSingletonTypes.ContainsKey(x.Key))
                                                               .Select(x => x.Key.Name);
            Debug.Log(DEBUG_LOG_PREFIX_OK + "Not installed non-lazy types on resolving: " + string.Join(", ", notInstalledNonLazyTypes));
            
            foreach (var (type, registrationRecord) in StaticContainerRegistrator.ContainerRegistrator.Registrations) {
                if (registrationRecord.Lazy) {
                    continue;
                }
                
                if (StaticContainerResolver.ContainerResolver.ResolvedSingletonTypes.ContainsKey(type)) {
                    continue;
                }
                
                StaticContainerResolver.ContainerResolver.Resolve(type);
            }
            
            Debug.Log("Resolved static types: " + string.Join(", ", StaticContainerResolver.ContainerResolver.ResolvedSingletonTypes.Select(x => x.Key.Name)));
            
            ContainerResolver.CopyResolvingsFrom(StaticContainerResolver.ContainerResolver);
        }
    }
}