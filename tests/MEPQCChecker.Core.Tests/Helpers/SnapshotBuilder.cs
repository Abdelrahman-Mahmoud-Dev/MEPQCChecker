using System;
using System.Collections.Generic;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Core.Tests.Helpers
{
    public class SnapshotBuilder
    {
        private readonly RevitModelSnapshot _snapshot = new RevitModelSnapshot
        {
            ProjectName = "Test Project",
            ModelPath = @"C:\Test\model.rvt",
            RevitVersion = "2024"
        };

        public SnapshotBuilder WithElement(Action<ElementBuilder> configure)
        {
            var builder = new ElementBuilder();
            configure(builder);
            _snapshot.Elements.Add(builder.Build());
            return this;
        }

        public SnapshotBuilder WithRoom(Action<RoomBuilder> configure)
        {
            var builder = new RoomBuilder();
            configure(builder);
            _snapshot.Rooms.Add(builder.Build());
            return this;
        }

        public SnapshotBuilder WithLevel(string name, double elevation = 0)
        {
            _snapshot.Levels.Add(new LevelData
            {
                Id = _snapshot.Levels.Count + 1,
                Name = name,
                Elevation = elevation
            });
            return this;
        }

        public RevitModelSnapshot Build() => _snapshot;
    }

    public class ElementBuilder
    {
        private readonly MEPElement _element = new MEPElement();
        private static long _idCounter = 1000;

        public ElementBuilder()
        {
            _element.Id = _idCounter++;
            _element.Level = "Level 1";
        }

        public ElementBuilder Id(long id) { _element.Id = id; return this; }
        public ElementBuilder Category(string cat) { _element.Category = cat; return this; }
        public ElementBuilder Level(string level) { _element.Level = level; return this; }
        public ElementBuilder Family(string name) { _element.FamilyName = name; return this; }
        public ElementBuilder Structural(bool val = true) { _element.IsStructural = val; return this; }

        public ElementBuilder WithBoundingBox(double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ)
        {
            _element.BoundingBox = new BoundingBoxData
            {
                MinX = minX, MinY = minY, MinZ = minZ,
                MaxX = maxX, MaxY = maxY, MaxZ = maxZ
            };
            return this;
        }

        public ElementBuilder WithConnector(bool connected, bool isEndCap = false, string desc = "End 1")
        {
            _element.Connectors.Add(new ConnectorData
            {
                IsConnected = connected,
                IsEndCap = isEndCap,
                Description = desc
            });
            return this;
        }

        public ElementBuilder WithParameter(string name, string value)
        {
            _element.Parameters[name] = value;
            return this;
        }

        public ElementBuilder WithGeometry(double startX, double startY, double startZ,
            double endX, double endY, double endZ,
            double diameterMM = 100, string systemName = "", string systemClassification = "")
        {
            _element.Geometry = new GeometryData
            {
                StartPoint = new PointData(startX, startY, startZ),
                EndPoint = new PointData(endX, endY, endZ),
                Diameter = diameterMM,
                SystemName = systemName,
                SystemClassification = systemClassification
            };
            return this;
        }

        public MEPElement Build() => _element;
    }

    public class RoomBuilder
    {
        private readonly RoomData _room = new RoomData();
        private static long _idCounter = 5000;

        public RoomBuilder()
        {
            _room.Id = _idCounter++;
            _room.Name = "Room 1";
            _room.Level = "Level 1";
        }

        public RoomBuilder Name(string name) { _room.Name = name; return this; }
        public RoomBuilder Level(string level) { _room.Level = level; return this; }
        public RoomBuilder Area(double area) { _room.Area = area; return this; }

        public RoomBuilder WithRectangularBoundary(double minX, double minY, double maxX, double maxY)
        {
            _room.BoundaryPoints = new List<PointData>
            {
                new PointData(minX, minY, 0),
                new PointData(maxX, minY, 0),
                new PointData(maxX, maxY, 0),
                new PointData(minX, maxY, 0)
            };
            _room.Area = (maxX - minX) * (maxY - minY);
            return this;
        }

        public RoomData Build() => _room;
    }
}
