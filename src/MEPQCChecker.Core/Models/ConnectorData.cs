namespace MEPQCChecker.Core.Models
{
    public class ConnectorData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool IsConnected { get; set; }
        public bool IsEndCap { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
