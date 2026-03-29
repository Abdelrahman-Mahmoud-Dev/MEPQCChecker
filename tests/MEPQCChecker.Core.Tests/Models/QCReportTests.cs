using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Tests.Models
{
    public class QCReportTests
    {
        [Fact]
        public void ComputedCounts_WithMixedSeverities_ReturnCorrectCounts()
        {
            var report = new QCReport
            {
                Issues = new System.Collections.Generic.List<QCIssue>
                {
                    new QCIssue { Severity = QCSeverity.Critical },
                    new QCIssue { Severity = QCSeverity.Critical },
                    new QCIssue { Severity = QCSeverity.Warning },
                    new QCIssue { Severity = QCSeverity.Info },
                    new QCIssue { Severity = QCSeverity.Info },
                    new QCIssue { Severity = QCSeverity.Info }
                }
            };

            Assert.Equal(2, report.CriticalCount);
            Assert.Equal(1, report.WarningCount);
            Assert.Equal(3, report.InfoCount);
            Assert.Equal(6, report.TotalCount);
        }

        [Fact]
        public void ComputedCounts_WhenEmpty_ReturnZero()
        {
            var report = new QCReport();

            Assert.Equal(0, report.CriticalCount);
            Assert.Equal(0, report.WarningCount);
            Assert.Equal(0, report.InfoCount);
            Assert.Equal(0, report.TotalCount);
        }
    }
}
