using System;
using System.Collections.Generic;
using System.Linq;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;

namespace MEPQCChecker.Core.Checks
{
    public class SprinklerCoverageChecker : IQCCheck
    {
        private readonly QCConfig _config;

        public string CheckName => "SprinklerCoverage";
        public string Discipline => "FireProtection";

        public SprinklerCoverageChecker(QCConfig config)
        {
            _config = config;
        }

        public IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot)
        {
            var sprinklers = snapshot.Elements
                .Where(e => e.Category == "OST_Sprinklers")
                .ToList();

            var levels = new HashSet<string>(snapshot.Levels.Select(l => l.Name));

            // Check for levels without rooms
            var roomLevels = new HashSet<string>(snapshot.Rooms.Select(r => r.Level));
            foreach (var level in levels)
            {
                if (!roomLevels.Contains(level) && sprinklers.Any(s => s.Level == level))
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Info,
                        CheckType = CheckName,
                        Discipline = Discipline,
                        Description = $"No rooms found on Level {level} — sprinkler coverage check skipped.",
                        Level = level
                    };
                }
            }

            double gridRes = _config.SprinklerCoverage.GridSamplingResolutionM;
            double coverageRadius = _config.SprinklerCoverage.DefaultCoverageRadiusM;
            double coverageRadiusSq = coverageRadius * coverageRadius;

            foreach (var room in snapshot.Rooms)
            {
                if (room.BoundaryPoints == null || room.BoundaryPoints.Count < 3)
                    continue;

                var roomSprinklers = sprinklers
                    .Where(s => s.Level == room.Level)
                    .Select(s => s.BoundingBox != null
                        ? new PointData(
                            (s.BoundingBox.MinX + s.BoundingBox.MaxX) / 2.0,
                            (s.BoundingBox.MinY + s.BoundingBox.MaxY) / 2.0,
                            0)
                        : null)
                    .Where(p => p != null)
                    .ToList();

                // Grid-sample the room
                double minX = room.BoundaryPoints.Min(p => p.X);
                double maxX = room.BoundaryPoints.Max(p => p.X);
                double minY = room.BoundaryPoints.Min(p => p.Y);
                double maxY = room.BoundaryPoints.Max(p => p.Y);

                int totalSamples = 0;
                int uncoveredSamples = 0;
                double nearestUncoveredDist = double.MaxValue;

                for (double x = minX; x <= maxX; x += gridRes)
                {
                    for (double y = minY; y <= maxY; y += gridRes)
                    {
                        if (!IsPointInPolygon(x, y, room.BoundaryPoints))
                            continue;

                        totalSamples++;

                        bool covered = false;
                        foreach (var head in roomSprinklers)
                        {
                            double dx = x - head!.X;
                            double dy = y - head.Y;
                            double distSq = dx * dx + dy * dy;
                            if (distSq <= coverageRadiusSq)
                            {
                                covered = true;
                                break;
                            }
                        }

                        if (!covered)
                        {
                            uncoveredSamples++;
                            // Track nearest head distance for reporting
                            foreach (var head in roomSprinklers)
                            {
                                double dx = x - head!.X;
                                double dy = y - head.Y;
                                double dist = Math.Sqrt(dx * dx + dy * dy);
                                if (dist < nearestUncoveredDist)
                                    nearestUncoveredDist = dist;
                            }
                        }
                    }
                }

                if (totalSamples == 0)
                    continue;

                double uncoveredPct = (double)uncoveredSamples / totalSamples * 100.0;
                double uncoveredArea = room.Area * uncoveredPct / 100.0;

                if (uncoveredPct > _config.SprinklerCoverage.CriticalUncoveredPct)
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Critical,
                        CheckType = CheckName,
                        Discipline = Discipline,
                        Description = $"Room {room.Name} on Level {room.Level} has ~{uncoveredArea:F1}m² outside sprinkler coverage. Nearest head is {nearestUncoveredDist:F1}m from uncovered zone.",
                        ElementId = room.Id,
                        ElementCategory = "OST_Rooms",
                        Level = room.Level,
                        MeasuredValue = uncoveredPct,
                        RequiredValue = _config.SprinklerCoverage.CriticalUncoveredPct
                    };
                }
                else if (uncoveredPct > _config.SprinklerCoverage.WarningUncoveredPct)
                {
                    yield return new QCIssue
                    {
                        Severity = QCSeverity.Warning,
                        CheckType = CheckName,
                        Discipline = Discipline,
                        Description = $"Room {room.Name} on Level {room.Level} has ~{uncoveredArea:F1}m² outside sprinkler coverage. Nearest head is {nearestUncoveredDist:F1}m from uncovered zone.",
                        ElementId = room.Id,
                        ElementCategory = "OST_Rooms",
                        Level = room.Level,
                        MeasuredValue = uncoveredPct,
                        RequiredValue = _config.SprinklerCoverage.WarningUncoveredPct
                    };
                }
            }
        }

        /// <summary>
        /// Ray casting algorithm for point-in-polygon test.
        /// </summary>
        internal static bool IsPointInPolygon(double x, double y, List<PointData> polygon)
        {
            bool inside = false;
            int count = polygon.Count;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];

                if ((pi.Y > y) != (pj.Y > y) &&
                    x < (pj.X - pi.X) * (y - pi.Y) / (pj.Y - pi.Y) + pi.X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
