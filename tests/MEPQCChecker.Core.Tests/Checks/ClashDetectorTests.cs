using System.Linq;
using MEPQCChecker.Core.Checks;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Tests.Helpers;

namespace MEPQCChecker.Core.Tests.Checks
{
    public class ClashDetectorTests
    {
        private readonly ClashDetector _checker = new ClashDetector();

        [Fact]
        public void TwoOverlappingDucts_ReturnsOneCriticalIssue()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithBoundingBox(0, 0, 0, 2, 2, 2))
                .WithElement(e => e.Id(2).Category("OST_PipeCurves")
                    .WithBoundingBox(1, 1, 1, 3, 3, 3))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
            Assert.Equal(QCSeverity.Critical, issues[0].Severity);
            Assert.Equal(1, issues[0].ElementId);
            Assert.Equal(2L, issues[0].ElementId2);
        }

        [Fact]
        public void TwoNonOverlappingDucts_ReturnsNoIssues()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithBoundingBox(0, 0, 0, 1, 1, 1))
                .WithElement(e => e.Id(2).Category("OST_DuctCurves")
                    .WithBoundingBox(10, 10, 10, 11, 11, 11))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void StructuralVsStructural_SkipsComparison()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_StructuralColumns").Structural()
                    .WithBoundingBox(0, 0, 0, 2, 2, 2))
                .WithElement(e => e.Id(2).Category("OST_StructuralFraming").Structural()
                    .WithBoundingBox(1, 1, 1, 3, 3, 3))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void SameElementNotComparedToItself()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithBoundingBox(0, 0, 0, 2, 2, 2))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void StructuralVsMEP_StillDetectsClash()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_StructuralColumns").Structural()
                    .WithBoundingBox(0, 0, 0, 2, 2, 2))
                .WithElement(e => e.Id(2).Category("OST_PipeCurves")
                    .WithBoundingBox(1, 1, 1, 3, 3, 3))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
        }
    }
}
