using System.Linq;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;
using MEPQCChecker.Core.Tests.Helpers;

namespace MEPQCChecker.Core.Tests.Services
{
    public class CheckRunnerTests
    {
        [Fact]
        public void RunAll_WithIssues_PopulatesReport()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithBoundingBox(0, 0, 0, 2, 2, 2)
                    .WithConnector(connected: false, desc: "End 1"))
                .WithElement(e => e.Id(2).Category("OST_PipeCurves")
                    .WithBoundingBox(1, 1, 1, 3, 3, 3)
                    .WithParameter("System Name", "Supply")
                    .WithParameter("System Classification", "Supply")
                    .WithParameter("Pipe Material", "Copper")
                    .WithParameter("Outside Diameter", "50")
                    .WithParameter("Insulation Type", "None"))
                .Build();

            var runner = new CheckRunner();
            var report = runner.RunAll(snapshot);

            Assert.NotNull(report);
            Assert.True(report.TotalCount > 0);
            Assert.Equal("Test Project", report.ProjectName);
        }

        [Fact]
        public void RunAll_EmptyModel_ReturnsNoIssues()
        {
            var snapshot = new SnapshotBuilder().Build();

            var runner = new CheckRunner();
            var report = runner.RunAll(snapshot);

            Assert.NotNull(report);
            Assert.Equal(0, report.TotalCount);
        }
    }
}
