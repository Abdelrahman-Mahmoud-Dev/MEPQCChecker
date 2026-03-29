using System.Linq;
using MEPQCChecker.Core.Checks;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;
using MEPQCChecker.Core.Tests.Helpers;

namespace MEPQCChecker.Core.Tests.Checks
{
    public class PipeSlopeCheckerTests
    {
        private readonly PipeSlopeChecker _checker;

        public PipeSlopeCheckerTests()
        {
            _checker = new PipeSlopeChecker(ConfigService.GetDefaults());
        }

        [Fact]
        public void FlatDrainagePipe_ReturnsCritical()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithGeometry(0, 0, 3.0, 10, 0, 3.0, diameterMM: 100, systemName: "Sanitary"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
            Assert.Equal(QCSeverity.Critical, issues[0].Severity);
        }

        [Fact]
        public void CorrectSlope_ReturnsNoIssues()
        {
            // 2% slope on a 10m horizontal pipe = 0.2m drop
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithGeometry(0, 0, 3.2, 10, 0, 3.0, diameterMM: 100, systemName: "Sanitary"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void UpwardSlopingPipe_ReturnsCritical()
        {
            // Pipe goes uphill (wrong direction)
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithGeometry(0, 0, 3.0, 10, 0, 3.5, diameterMM: 100, systemName: "Sanitary"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
            Assert.Equal(QCSeverity.Critical, issues[0].Severity);
            Assert.Contains("wrong direction", issues[0].Description);
        }

        [Fact]
        public void PressurizedPipeIgnored_ReturnsNoIssues()
        {
            // Fire Protection is not gravity-drained
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithGeometry(0, 0, 3.0, 10, 0, 3.0, diameterMM: 100, systemName: "Fire Protection"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void ExcessiveSlope_ReturnsWarning()
        {
            // 20% slope = 2m drop over 10m horizontal
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithGeometry(0, 0, 5.0, 10, 0, 3.0, diameterMM: 100, systemName: "Sanitary"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
            Assert.Equal(QCSeverity.Warning, issues[0].Severity);
        }

        [Fact]
        public void SmallPipe_UsesHigherMinSlope()
        {
            // DN40 pipe with 1.5% slope — ok for large pipe but too low for small pipe (min 2%)
            // 1.5% slope on 10m = 0.15m drop
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithGeometry(0, 0, 3.15, 10, 0, 3.0, diameterMM: 40, systemName: "Sanitary"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
            Assert.Equal(QCSeverity.Critical, issues[0].Severity);
        }
    }
}
