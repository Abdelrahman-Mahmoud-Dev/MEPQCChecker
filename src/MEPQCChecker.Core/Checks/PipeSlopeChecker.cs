using System;
using System.Collections.Generic;
using System.Linq;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;

namespace MEPQCChecker.Core.Checks
{
    public class PipeSlopeChecker : IQCCheck
    {
        private readonly QCConfig _config;

        public string CheckName => "PipeSlope";
        public string Discipline => "Plumbing";

        public PipeSlopeChecker(QCConfig config)
        {
            _config = config;
        }

        public IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot)
        {
            var gravitySystemNames = new HashSet<string>(
                _config.GravityDrainedSystemNames,
                StringComparer.OrdinalIgnoreCase);

            var pipes = snapshot.Elements
                .Where(e => e.Category == "OST_PipeCurves")
                .Where(e => e.Geometry?.StartPoint != null && e.Geometry?.EndPoint != null)
                .Where(e => IsGravityDrained(e, gravitySystemNames));

            foreach (var pipe in pipes)
            {
                var start = pipe.Geometry!.StartPoint!;
                var end = pipe.Geometry!.EndPoint!;

                double dx = end.X - start.X;
                double dy = end.Y - start.Y;
                double horizontalLength = Math.Sqrt(dx * dx + dy * dy);

                if (horizontalLength < 0.001) // nearly vertical
                    continue;

                double dz = start.Z - end.Z; // positive = downhill (correct)
                double slopePct = (dz / horizontalLength) * 100.0;

                double diameter = pipe.Geometry.Diameter;
                double minSlope = diameter >= _config.PipeSlope.SmallPipeThresholdMM
                    ? _config.PipeSlope.MinSlopePctLargePipe
                    : _config.PipeSlope.MinSlopePctSmallPipe;
                double maxSlope = _config.PipeSlope.MaxSlopePct;

                if (slopePct < 0)
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Critical,
                        CheckType = CheckName,
                        Discipline = Discipline,
                        Description = $"Drainage pipe (ID: {pipe.Id}) on Level {pipe.Level} has slope {slopePct:F2}% — wrong direction (uphill)",
                        ElementId = pipe.Id,
                        ElementCategory = pipe.Category,
                        Level = pipe.Level,
                        MeasuredValue = slopePct,
                        RequiredValue = minSlope
                    };
                }
                else if (slopePct < minSlope)
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Critical,
                        CheckType = CheckName,
                        Discipline = Discipline,
                        Description = $"Drainage pipe (ID: {pipe.Id}) on Level {pipe.Level} has slope {slopePct:F2}% — minimum required is {minSlope}%",
                        ElementId = pipe.Id,
                        ElementCategory = pipe.Category,
                        Level = pipe.Level,
                        MeasuredValue = slopePct,
                        RequiredValue = minSlope
                    };
                }
                else if (slopePct > maxSlope)
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Warning,
                        CheckType = CheckName,
                        Discipline = Discipline,
                        Description = $"Drainage pipe (ID: {pipe.Id}) on Level {pipe.Level} has slope {slopePct:F2}% — maximum recommended is {maxSlope}%",
                        ElementId = pipe.Id,
                        ElementCategory = pipe.Category,
                        Level = pipe.Level,
                        MeasuredValue = slopePct,
                        RequiredValue = maxSlope
                    };
                }
            }
        }

        private static bool IsGravityDrained(MEPElement pipe, HashSet<string> gravitySystemNames)
        {
            var systemName = pipe.Geometry?.SystemName;
            if (!string.IsNullOrEmpty(systemName) && gravitySystemNames.Contains(systemName))
                return true;

            // Also check parameters
            if (pipe.Parameters.TryGetValue("System Name", out var paramSystemName) && paramSystemName != null)
                return gravitySystemNames.Contains(paramSystemName);

            return false;
        }
    }
}
