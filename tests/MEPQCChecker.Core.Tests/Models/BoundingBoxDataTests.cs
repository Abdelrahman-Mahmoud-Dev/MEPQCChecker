using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Tests.Models
{
    public class BoundingBoxDataTests
    {
        [Fact]
        public void Overlaps_WhenBoxesOverlap_ReturnsTrue()
        {
            var a = new BoundingBoxData { MinX = 0, MinY = 0, MinZ = 0, MaxX = 2, MaxY = 2, MaxZ = 2 };
            var b = new BoundingBoxData { MinX = 1, MinY = 1, MinZ = 1, MaxX = 3, MaxY = 3, MaxZ = 3 };

            Assert.True(a.Overlaps(b));
            Assert.True(b.Overlaps(a));
        }

        [Fact]
        public void Overlaps_WhenBoxesSeparate_ReturnsFalse()
        {
            var a = new BoundingBoxData { MinX = 0, MinY = 0, MinZ = 0, MaxX = 1, MaxY = 1, MaxZ = 1 };
            var b = new BoundingBoxData { MinX = 5, MinY = 5, MinZ = 5, MaxX = 6, MaxY = 6, MaxZ = 6 };

            Assert.False(a.Overlaps(b));
        }

        [Fact]
        public void Overlaps_WhenTouching_ReturnsTrue()
        {
            var a = new BoundingBoxData { MinX = 0, MinY = 0, MinZ = 0, MaxX = 1, MaxY = 1, MaxZ = 1 };
            var b = new BoundingBoxData { MinX = 1, MinY = 0, MinZ = 0, MaxX = 2, MaxY = 1, MaxZ = 1 };

            Assert.True(a.Overlaps(b));
        }

        [Fact]
        public void Overlaps_WhenSeparateOnOneAxis_ReturnsFalse()
        {
            var a = new BoundingBoxData { MinX = 0, MinY = 0, MinZ = 0, MaxX = 2, MaxY = 2, MaxZ = 2 };
            var b = new BoundingBoxData { MinX = 0, MinY = 0, MinZ = 5, MaxX = 2, MaxY = 2, MaxZ = 7 };

            Assert.False(a.Overlaps(b));
        }
    }
}
