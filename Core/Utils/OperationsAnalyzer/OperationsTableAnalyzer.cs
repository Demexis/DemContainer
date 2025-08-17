using System;
using System.Collections.Generic;
using System.Linq;

namespace DemContainer {
    public sealed class OperationAnalyzerRecord {
        public string name;
        public int count;
        public TimeSpan time;

        public OperationAnalyzerRecord(string name, TimeSpan time) {
            this.name = name;
            this.time = time;
            count = 1;
        }
    }
    
    public sealed class OperationsTableAnalyzer {
        private const int TABLE_SHOW_LIMIT = 50;
        
        public IReadOnlyList<OperationAnalyzerRecord> Records => records;
        private readonly List<OperationAnalyzerRecord> records = new();

        public void AddNewRecord(string name, TimeSpan time) {
            foreach (var record in records) {
                if (record.name == name) {
                    record.time += time;
                    record.count += 1;
                    return;
                }
            }
            records.Add(new OperationAnalyzerRecord(name, time));
        }

        public void Clear() {
            records.Clear();
        }

        public override string ToString() {
            var formattedMessage = string.Empty;

            var timeSummary = TimeSpan.Zero;
            var totalCount = 0;

            var sortedRecords = records.OrderByDescending(x => x.time).ToList();

            PadMessage("Name", "Count", "Time");

            for (var i = 0; i < sortedRecords.Count; i++) {
                var record = sortedRecords[i];

                if (i < TABLE_SHOW_LIMIT) {
                    PadMessage(record.name, record.count.ToString(), record.time.ToString());
                }
                
                timeSummary += record.time;
                totalCount += record.count;
            }

            formattedMessage += "Total time: " + timeSummary + Environment.NewLine;
            formattedMessage += "Total count: " + totalCount + Environment.NewLine;
            
            return formattedMessage;

            void PadMessage(string arg1, string arg2, string arg3) {
                var lineStr = string.Empty;
                
                lineStr += arg1;
                lineStr = lineStr.PadRight(30);

                lineStr += arg2;
                lineStr = lineStr.PadRight(60);
                
                lineStr += arg3;
                lineStr = lineStr.PadRight(90);
                lineStr += Environment.NewLine;

                formattedMessage += lineStr;
            }
        }
        
        public TimeSpan GetTotalTime() {
            var timeSummary = TimeSpan.Zero;

            foreach (var record in records) {
                timeSummary += record.time;
            }

            return timeSummary;
        }

        public int GetTotalCount() {
            var totalCount = 0;
            
            foreach (var record in records) {
                totalCount += record.count;
            }

            return totalCount;
        }
    }
}