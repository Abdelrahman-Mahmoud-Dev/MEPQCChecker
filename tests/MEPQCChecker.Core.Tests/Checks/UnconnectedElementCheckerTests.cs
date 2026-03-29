using System.Linq;
using MEPQCChecker.Core.Checks;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Tests.Helpers;

namespace MEPQCChecker.Core.Tests.Checks
{
    public class UnconnectedElementCheckerTests
    {
        private readonly UnconnectedElementChecker _checker = new UnconnectedElementChecker();

        [Fact]
        public void OpenConnector_ReturnsWarning()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithConnector(connected: true, desc: "End 1")
                    .WithConnector(connected: false, desc: "End 2"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Single(issues);
            Assert.Equal(QCSeverity.Warning, issues[0].Severity);
        }

        [Fact]
        public void TerminalElement_Excluded()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PlumbingFixtures")
                    .WithConnector(connected: false, desc: "Outlet"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void AllConnected_ReturnsNoIssues()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithConnector(connected: true, desc: "End 1")
                    .WithConnector(connected: true, desc: "End 2"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void EndCap_NotFlagged()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_PipeCurves")
                    .WithConnector(connected: true, desc: "End 1")
                    .WithConnector(connected: false, isEndCap: true, desc: "End 2"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void SprinklerTerminal_Excluded()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_Sprinklers")
                    .WithConnector(connected: false, desc: "Inlet"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }
    }
}
