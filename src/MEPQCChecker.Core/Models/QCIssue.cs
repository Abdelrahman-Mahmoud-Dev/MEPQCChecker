using System;
using System.Collections.Generic;

namespace MEPQCChecker.Core.Models
{
    public class QCIssue
    {
        public string IssueId { get; set; } = Guid.NewGuid().ToString();
        public QCSeverity Severity { get; set; }
        public string CheckType { get; set; } = string.Empty;
        public string Discipline { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long ElementId { get; set; }
        public long? ElementId2 { get; set; }
        public string ElementCategory { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double? MeasuredValue { get; set; }
        public double? RequiredValue { get; set; }
        public string? ParameterName { get; set; }
        public string? SourceModelId { get; set; }
        public string? SourceModelName { get; set; }
        public string? SourceModelId2 { get; set; }
        public string? SourceModelName2 { get; set; }
        public List<FixProposal> FixProposals { get; set; } = new List<FixProposal>();
    }
}
