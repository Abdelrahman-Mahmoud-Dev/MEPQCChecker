using System.Collections.Generic;

namespace MEPQCChecker.Core.Models
{
    public class RoomData
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public List<PointData> BoundaryPoints { get; set; } = new List<PointData>();
        public double Area { get; set; } // m²
    }
}
