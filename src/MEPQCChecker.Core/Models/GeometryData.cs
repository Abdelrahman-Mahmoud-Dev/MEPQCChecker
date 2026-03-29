namespace MEPQCChecker.Core.Models
{
    public class GeometryData
    {
        public PointData? StartPoint { get; set; }
        public PointData? EndPoint { get; set; }
        public double Diameter { get; set; } // mm
        public string SystemClassification { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
    }
}
