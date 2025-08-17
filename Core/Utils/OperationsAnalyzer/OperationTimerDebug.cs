using System;
using UnityEngine;

namespace DemContainer {
    internal static class OperationTimerDebug {
        private static readonly Color serviceColor = new(1, 0.3f, 1);
        private static readonly Color valueColor = new(1, 0.5f, 0);
        
        public static void LogThreshold(OperationTimerResult result, TimeSpan threshold) {
            if (threshold >= result.elapsed) {
                return;
            }

            Log(result);
        }
        
        public static void Log(OperationTimerResult result) {
            var msg = $"Operation {ColorUtils.Colorize(result.label, valueColor)}. "
                + $"Elapsed time: {ColorUtils.Colorize(result.elapsed.ToString(), valueColor)}. "
                + $"GC Collections: {ColorUtils.Colorize(result.gcCollectionsCount.ToString(), valueColor)}.";
            
            Debug.Log($"[{ColorUtils.Colorize("Optimization", serviceColor)}] {msg}");
        }
    }
}