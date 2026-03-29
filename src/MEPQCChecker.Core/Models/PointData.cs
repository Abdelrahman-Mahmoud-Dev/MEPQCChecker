namespace MEPQCChecker.Core.Models
{
    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public PointData() { }

        public PointData(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
