using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DemContainer.Editor {
    public static class DemContainerTools {
        [MenuItem("Tools/Demegraunt/Check Duplicate Installers")]
        public static void CheckDuplicateInstallers() {
            var rootInstallers = Object.FindObjectsByType<BaseRootInstaller>(FindObjectsSortMode.None);
            var childInstallers = Object.FindObjectsByType<BaseChildInstaller>(FindObjectsSortMode.None);

            var checkedRootInstallers = new List<BaseRootInstaller>();
            var checkedChildInstallers = new List<BaseChildInstaller>();

            var duplicateChildInstallersCount = 0;
            var duplicateReferencesInstallersCount = 0;
            
            foreach (var childInstaller in childInstallers) {
                if (checkedChildInstallers.Contains(childInstaller)) {
                    duplicateChildInstallersCount++;
                    Debug.LogError("Found duplicate installer - " + childInstaller.GetType().Name);
                }
                
                checkedChildInstallers.Add(childInstaller);
            }

            foreach (var rootInstaller in rootInstallers) {
                foreach (var checkedRootInstaller in checkedRootInstallers) {
                    foreach (var childInstaller in rootInstaller.childInstallers) {
                        if (checkedRootInstaller.childInstallers.Contains(childInstaller)) {
                            duplicateReferencesInstallersCount++;
                            Debug.LogError($"Found duplicate reference ({childInstaller.GetType().Name}) " + 
                                $"between two root installers: ({rootInstaller.GetType().Name} <-> {checkedRootInstaller.GetType().Name})");
                        }
                    }
                }
                
                checkedRootInstallers.Add(rootInstaller);
            }
            
            Debug.Log("Total duplicate child installers: " + duplicateChildInstallersCount);
            Debug.Log("Total duplicate references: " + duplicateReferencesInstallersCount);
        }
    }
}