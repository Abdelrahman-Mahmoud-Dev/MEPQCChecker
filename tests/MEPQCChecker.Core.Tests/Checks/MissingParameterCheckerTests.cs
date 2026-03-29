using System.Linq;
using MEPQCChecker.Core.Checks;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;
using MEPQCChecker.Core.Tests.Helpers;

namespace MEPQCChecker.Core.Tests.Checks
{
    public class MissingParameterCheckerTests
    {
        private readonly MissingParameterChecker _checker;

        public MissingParameterCheckerTests()
        {
            _checker = new MissingParameterChecker(ConfigService.GetDefaults());
        }

        [Fact]
        public void MissingSystemName_ReturnsWarning()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithParameter("System Classification", "Supply Air")
                    .WithParameter("Insulation Type", "Fiberglass")
                    .WithParameter("Flow", "500"))
                // "System Name" is missing
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Contains(issues, i => i.ParameterName == "System Name" && i.Severity == QCSeverity.Warning);
        }

        [Fact]
        public void AllParametersPresent_ReturnsNoIssues()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithParameter("System Name", "Supply Air")
                    .WithParameter("System Classification", "Supply Air")
                    .WithParameter("Insulation Type", "Fiberglass")
                    .WithParameter("Flow", "500"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Empty(issues);
        }

        [Fact]
        public void EmptyStringCountsAsMissing()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithParameter("System Name", "")
                    .WithParameter("System Classification", "Supply Air")
                    .WithParameter("Insulation Type", "Fiberglass")
                    .WithParameter("Flow", "500"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Contains(issues, i => i.ParameterName == "System Name");
        }

        [Fact]
        public void NoneValueCountsAsMissing()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_DuctCurves")
                    .WithParameter("System Name", "<none>")
                    .WithParameter("System Classification", "Supply Air")
                    .WithParameter("Insulation Type", "Fiberglass")
                    .WithParameter("Flow", "500"))
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Contains(issues, i => i.ParameterName == "System Name");
        }

        [Fact]
        public void SprinklerMissingCriticalParam_ReturnsCritical()
        {
            var snapshot = new SnapshotBuilder()
                .WithElement(e => e.Id(1).Category("OST_Sprinklers")
                    .WithParameter("Head Type", "Pendant")
                    .WithParameter("Coverage Radius", "2.25")
                    .WithParameter("Flow", "80"))
                // "System Name" missing — should be Critical for sprinklers
                .Build();

            var issues = _checker.Run(snapshot).ToList();

            Assert.Contains(issues, i => i.ParameterName == "System Name" && i.Severity == QCSeverity.Critical);
        }
    }
}
