using System.Collections.Generic;

namespace MEPQCChecker.Core.Models
{
    public class MEPElement
    {
        public long Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public BoundingBoxData? BoundingBox { get; set; }
        public List<ConnectorData> Connectors { get; set; } = new List<ConnectorData>();
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public bool IsStructural { get; set; }
        public GeometryData? Geometry { get; set; }
    }
}
