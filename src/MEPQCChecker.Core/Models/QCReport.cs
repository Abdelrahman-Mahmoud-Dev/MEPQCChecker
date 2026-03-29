using System;
using System.Collections.Generic;
using System.Linq;

namespace MEPQCChecker.Core.Models
{
    public class QCReport
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public DateTime RunAt { get; set; } = DateTime.Now;
        public string RevitVersion { get; set; } = string.Empty;
        public List<QCIssue> Issues { get; set; } = new List<QCIssue>();

        public int CriticalCount => Issues.Count(i => i.Severity == QCSeverity.Critical);
        public int WarningCount => Issues.Count(i => i.Severity == QCSeverity.Warning);
        public int InfoCount => Issues.Count(i => i.Severity == QCSeverity.Info);
        public int TotalCount => Issues.Count;
    }
}
