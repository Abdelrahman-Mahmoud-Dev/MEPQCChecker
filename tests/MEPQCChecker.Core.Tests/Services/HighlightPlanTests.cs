using System.Collections.Generic;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;

namespace MEPQCChecker.Core.Tests.Services
{
    public class HighlightPlanTests
    {
        [Fact]
        public void FromReport_SeparatesCriticalAndWarning()
        {
            var report = new QCReport
            {
                Issues = new List<QCIssue>
                {
                    new QCIssue { Severity = QCSeverity.Critical, ElementId = 100 },
                    new QCIssue { Severity = QCSeverity.Critical, ElementId = 200, ElementId2 = 300 },
                    new QCIssue { Severity = QCSeverity.Warning, ElementId = 400 },
                    new QCIssue { Severity = QCSeverity.Info, ElementId = 500 }
                }
            };

            var plan = HighlightPlan.FromReport(report);

            Assert.Contains(100L, plan.CriticalElementIds);
            Assert.Contains(200L, plan.CriticalElementIds);
            Assert.Contains(300L, plan.CriticalElementIds); // ElementId2 from clash
            Assert.Contains(400L, plan.WarningElementIds);
            Assert.DoesNotContain(500L, plan.WarningElementIds); // Info not highlighted
        }

        [Fact]
        public void FromReport_CriticalTakesPrecedenceOverWarning()
        {
            var report = new QCReport
            {
                Issues = new List<QCIssue>
                {
                    new QCIssue { Severity = QCSeverity.Critical, ElementId = 100 },
                    new QCIssue { Severity = QCSeverity.Warning, ElementId = 100 } // same element
                }
            };

            var plan = HighlightPlan.FromReport(report);

            Assert.Contains(100L, plan.CriticalElementIds);
            Assert.DoesNotContain(100L, plan.WarningElementIds);
        }

        [Fact]
        public void FromReport_NoDuplicateIds()
        {
            var report = new QCReport
            {
                Issues = new List<QCIssue>
                {
                    new QCIssue { Severity = QCSeverity.Critical, ElementId = 100 },
                    new QCIssue { Severity = QCSeverity.Critical, ElementId = 100 }
                }
            };

            var plan = HighlightPlan.FromReport(report);

            Assert.Single(plan.CriticalElementIds);
        }
    }
}
