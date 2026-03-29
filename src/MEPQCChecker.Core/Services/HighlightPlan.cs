using System.Collections.Generic;
using System.Linq;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Services
{
    public class HighlightPlan
    {
        public List<long> CriticalElementIds { get; set; } = new List<long>();
        public List<long> WarningElementIds { get; set; } = new List<long>();

        public static HighlightPlan FromReport(QCReport report)
        {
            var plan = new HighlightPlan();

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == QCSeverity.Critical)
                {
                    if (!plan.CriticalElementIds.Contains(issue.ElementId))
                        plan.CriticalElementIds.Add(issue.ElementId);
                    if (issue.ElementId2.HasValue && !plan.CriticalElementIds.Contains(issue.ElementId2.Value))
                        plan.CriticalElementIds.Add(issue.ElementId2.Value);
                }
                else if (issue.Severity == QCSeverity.Warning)
                {
                    if (!plan.WarningElementIds.Contains(issue.ElementId)
                        && !plan.CriticalElementIds.Contains(issue.ElementId))
                        plan.WarningElementIds.Add(issue.ElementId);
                }
            }

            return plan;
        }
    }
}
