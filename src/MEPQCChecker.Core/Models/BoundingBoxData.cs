namespace MEPQCChecker.Core.Models
{
    public class BoundingBoxData
    {
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MinZ { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MaxZ { get; set; }

        public bool Overlaps(BoundingBoxData other) =>
            MinX <= other.MaxX && MaxX >= other.MinX &&
            MinY <= other.MaxY && MaxY >= other.MinY &&
            MinZ <= other.MaxZ && MaxZ >= other.MinZ;
    }
}
