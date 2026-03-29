using System.Linq;
using MEPQCChecker.Core.Checks;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;
using MEPQCChecker.Core.Tests.Helpers;

namespace MEPQCChecker.Core.Tests.Checks
{
    public class SprinklerCoverageCheckerTests
    {
        private readonly SprinklerCoverageChecker _checker;

        public SprinklerCoverageCheckerTests()
        {
            _checker = new SprinklerCoverageChecker(ConfigService.GetDefaults());
        }

        [Fact]
        public void RoomFullyCovered_ReturnsNoIssues()
        {
            // Small 3x3 room with a centered sprinkler — max corner distance ~2.12m < 2.25m radius
            var snapshot = new SnapshotBuilder()
                .WithLevel("Level 1")
                .WithRoom(r => r.Name("Office")
                    .Level("Level 1")
                    .WithRectangularBoundary(0, 0, 3, 3))
                .WithElement(e => e.Id(1).Category("OST_Sprinklers").Level("Level 1")
                    .WithBoundingBox(1.4, 1.4, 2.5, 1.6, 1.6, 2.7))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            // Filter out info-level "no rooms" issues
            var coverageIssues = issues.Where(i =>
                i.Severity == QCSeverity.Critical || i.Severity == QCSeverity.Warning).ToList();
            Assert.Empty(coverageIssues);
        }

        [Fact]
        public void LargeRoomOneHead_ReturnsCritical()
        {
            // Large 20x20 room with single sprinkler in corner — most area uncovered
            var snapshot = new SnapshotBuilder()
                .WithLevel("Level 1")
                .WithRoom(r => r.Name("Warehouse")
                    .Level("Level 1")
                    .WithRectangularBoundary(0, 0, 20, 20))
                .WithElement(e => e.Id(1).Category("OST_Sprinklers").Level("Level 1")
                    .WithBoundingBox(0, 0, 2.5, 0.2, 0.2, 2.7))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Contains(issues, i => i.Severity == QCSeverity.Critical);
        }

        [Fact]
        public void NoRooms_ReturnsInfoIssue()
        {
            // Level with sprinklers but no rooms
            var snapshot = new SnapshotBuilder()
                .WithLevel("Level 1")
                .WithElement(e => e.Id(1).Category("OST_Sprinklers").Level("Level 1")
                    .WithBoundingBox(1, 1, 2.5, 1.2, 1.2, 2.7))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Contains(issues, i => i.Severity == QCSeverity.Info
                && i.Description.Contains("No rooms found"));
        }
    }
}
