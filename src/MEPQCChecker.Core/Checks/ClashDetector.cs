using System;
using System.Collections.Generic;
using System.Linq;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Checks
{
    public class ClashDetector : IQCCheck
    {
        private const double GridCellSize = 2.0; // metres

        public string CheckName => "ClashDetection";
        public string Discipline => "All";

        public IEnumerable<QCIssue> Run(RevitModelSnapshot snapshot)
        {
            var elements = snapshot.Elements
                .Where(e => e.BoundingBox != null)
                .ToList();

            // Group by level, then spatial grid
            var byLevel = elements.GroupBy(e => e.Level);
            var reportedPairs = new HashSet<(long, long)>();

            foreach (var levelGroup in byLevel)
            {
                var grid = BuildSpatialGrid(levelGroup.ToList());

                foreach (var kvp in grid)
                {
                    var cellKey = kvp.Key;
                    var cellElements = kvp.Value;

                    // Get elements from this cell and all 8 adjacent cells
                    var neighborhood = GetNeighborhood(grid, cellKey);

                    for (int i = 0; i < cellElements.Count; i++)
                    {
                        var a = cellElements[i];

                        foreach (var b in neighborhood)
                        {
                            if (a.Id >= b.Id) // skip self + ensure each pair once
                                continue;

                            // Skip structural vs structural
                            if (a.IsStructural && b.IsStructural)
                                continue;

                            var pairKey = (a.Id, b.Id);
                            if (reportedPairs.Contains(pairKey))
                                continue;

                            if (a.BoundingBox!.Overlaps(b.BoundingBox!))
                            {
                                reportedPairs.Add(pairKey);
                                yield return new QCIssue
                                {
                                    Severity = QCSeverity.Critical,
                                    CheckType = CheckName,
                                    Discipline = "All",
                                    Description = $"Clash between {a.Category} (ID: {a.Id}) and {b.Category} (ID: {b.Id}) on Level {a.Level}",
                                    ElementId = a.Id,
                                    ElementId2 = b.Id,
                                    ElementCategory = a.Category,
                                    Level = a.Level
                                };
                            }
                        }
                    }
                }
            }
        }

        private static Dictionary<(int, int), List<MEPElement>> BuildSpatialGrid(List<MEPElement> elements)
        {
            var grid = new Dictionary<(int, int), List<MEPElement>>();

            foreach (var element in elements)
            {
                var bb = element.BoundingBox!;

                int minCellX = (int)Math.Floor(bb.MinX / GridCellSize);
                int minCellY = (int)Math.Floor(bb.MinY / GridCellSize);
                int maxCellX = (int)Math.Floor(bb.MaxX / GridCellSize);
                int maxCellY = (int)Math.Floor(bb.MaxY / GridCellSize);

                for (int cx = minCellX; cx <= maxCellX; cx++)
                {
                    for (int cy = minCellY; cy <= maxCellY; cy++)
                    {
                        var key = (cx, cy);
                        if (!grid.TryGetValue(key, out var list))
                        {
                            list = new List<MEPElement>();
                            grid[key] = list;
                        }
                        list.Add(element);
                    }
                }
            }

            return grid;
        }

        private static List<MEPElement> GetNeighborhood(
            Dictionary<(int, int), List<MEPElement>> grid,
            (int x, int y) cellKey)
        {
            var result = new List<MEPElement>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue; // skip self cell (already iterated in outer loop)

                    var neighborKey = (cellKey.x + dx, cellKey.y + dy);
                    if (grid.TryGetValue(neighborKey, out var list))
                        result.AddRange(list);
                }
            }
            return result;
        }
    }
}
