using System;
using System.Diagnostics;

namespace DemContainer {
    public struct OperationTimerResult {
        public readonly string label;
        public readonly TimeSpan elapsed;
        public readonly int gcCollectionsCount;

        public OperationTimerResult(string label, TimeSpan elapsed, int gcCollectionsCount) {
            this.label = label;
            this.elapsed = elapsed;
            this.gcCollectionsCount = gcCollectionsCount;
        }
    }
    
    /// <summary>
    /// Utility class made to measure time without manually handling the stopwatch class.
    /// It's disposable, use this with 'using statement'.
    /// </summary>
    public sealed class OperationTimer : IDisposable {
        private readonly Stopwatch stopwatch;
        private readonly string label;
        private readonly int collectionCount;
        private readonly Action<OperationTimerResult> callback;

        public OperationTimer(string label, Action<OperationTimerResult> callback, bool collectGarbage = false) {
            if (collectGarbage) {
                PrepareForOperation();
            }
            
            this.label = label;
            this.callback = callback;
            collectionCount = GC.CollectionCount(0);
            stopwatch = Stopwatch.StartNew();
        }

        private void PrepareForOperation() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public void Dispose() {
            callback.Invoke(new OperationTimerResult(label, stopwatch.Elapsed,
                GC.CollectionCount(0) - collectionCount));
        }
    }
}