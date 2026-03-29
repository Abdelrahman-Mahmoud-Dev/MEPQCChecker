using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Revit.Adapters
{
    public class RevitModelAdapter
    {
        private const double FeetToMetres = 0.3048;

        private readonly Document _doc;

        private static readonly BuiltInCategory[] MepCategories =
        {
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_DuctFitting,
            BuiltInCategory.OST_DuctAccessory,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_Sprinklers,
            BuiltInCategory.OST_PlumbingFixtures,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_CableTray,
            BuiltInCategory.OST_Conduit
        };

        private static readonly BuiltInCategory[] StructuralCategories =
        {
            BuiltInCategory.OST_StructuralColumns,
            BuiltInCategory.OST_StructuralFraming
        };

        public RevitModelAdapter(Document doc)
        {
            _doc = doc;
        }

        public RevitModelSnapshot BuildSnapshot()
        {
            var snapshot = new RevitModelSnapshot
            {
                ProjectName = _doc.Title,
                ModelPath = _doc.PathName ?? string.Empty,
                RevitVersion = _doc.Application.VersionNumber
            };

            // Collect MEP elements
            foreach (var category in MepCategories)
            {
                var collector = new FilteredElementCollector(_doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType();

                foreach (var element in collector)
                {
                    var mepElement = ConvertElement(element, false);
                    if (mepElement != null)
                        snapshot.Elements.Add(mepElement);
                }
            }

            // Collect structural elements (for clash detection)
            foreach (var category in StructuralCategories)
            {
                var collector = new FilteredElementCollector(_doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType();

                foreach (var element in collector)
                {
                    var mepElement = ConvertElement(element, true);
                    if (mepElement != null)
                        snapshot.Elements.Add(mepElement);
                }
            }

            // Collect rooms
            var roomCollector = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType();

            foreach (var element in roomCollector)
            {
                if (element is Autodesk.Revit.DB.Architecture.Room room)
                {
                    var roomData = ConvertRoom(room);
                    if (roomData != null)
                        snapshot.Rooms.Add(roomData);
                }
            }

            // Collect levels
            var levelCollector = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType();

            foreach (var element in levelCollector)
            {
                if (element is Level level)
                {
                    snapshot.Levels.Add(new LevelData
                    {
                        Id = level.Id.Value,
                        Name = level.Name,
                        Elevation = level.Elevation * FeetToMetres
                    });
                }
            }

            return snapshot;
        }

        private MEPElement? ConvertElement(Element element, bool isStructural)
        {
            var categoryName = GetCategoryName(element);
            if (string.IsNullOrEmpty(categoryName))
                return null;

            var mepElement = new MEPElement
            {
                Id = element.Id.Value,
                Category = categoryName,
                FamilyName = GetFamilyName(element),
                Level = GetLevelName(element),
                IsStructural = isStructural,
                BoundingBox = GetBoundingBox(element),
                Connectors = GetConnectors(element),
                Parameters = GetParameters(element),
                Geometry = GetGeometryData(element)
            };

            return mepElement;
        }

        private static string GetCategoryName(Element element)
        {
            var category = element.Category;
            if (category == null) return string.Empty;

            var bic = (BuiltInCategory)category.Id.Value;
            return "OST_" + bic.ToString().Replace("OST_", "");
        }

        private static string GetFamilyName(Element element)
        {
            try
            {
                var typeId = element.GetTypeId();
                if (typeId != null && typeId != ElementId.InvalidElementId)
                {
                    var type = element.Document.GetElement(typeId);
                    if (type is FamilySymbol fs)
                        return fs.Family?.Name ?? string.Empty;
                }
            }
            catch { }
            return string.Empty;
        }

        private string GetLevelName(Element element)
        {
            try
            {
                var levelId = element.LevelId;
                if (levelId != null && levelId != ElementId.InvalidElementId)
                {
                    var level = _doc.GetElement(levelId) as Level;
                    return level?.Name ?? string.Empty;
                }

                // Try Level parameter
                var levelParam = element.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM)
                    ?? element.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                    ?? element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);

                if (levelParam != null && levelParam.AsElementId() != ElementId.InvalidElementId)
                {
                    var level = _doc.GetElement(levelParam.AsElementId()) as Level;
                    return level?.Name ?? string.Empty;
                }
            }
            catch { }
            return string.Empty;
        }

        private static BoundingBoxData? GetBoundingBox(Element element)
        {
            var bb = element.get_BoundingBox(null);
            if (bb == null) return null;

            return new BoundingBoxData
            {
                MinX = bb.Min.X * FeetToMetres,
                MinY = bb.Min.Y * FeetToMetres,
                MinZ = bb.Min.Z * FeetToMetres,
                MaxX = bb.Max.X * FeetToMetres,
                MaxY = bb.Max.Y * FeetToMetres,
                MaxZ = bb.Max.Z * FeetToMetres
            };
        }

        private static List<ConnectorData> GetConnectors(Element element)
        {
            var result = new List<ConnectorData>();
            try
            {
                ConnectorSet? connectors = null;

                if (element is FamilyInstance fi && fi.MEPModel?.ConnectorManager != null)
                    connectors = fi.MEPModel.ConnectorManager.Connectors;
                else if (element is MEPCurve curve && curve.ConnectorManager != null)
                    connectors = curve.ConnectorManager.Connectors;

                if (connectors == null) return result;

                int index = 1;
                foreach (Connector connector in connectors)
                {
                    result.Add(new ConnectorData
                    {
                        X = connector.Origin.X * FeetToMetres,
                        Y = connector.Origin.Y * FeetToMetres,
                        Z = connector.Origin.Z * FeetToMetres,
                        IsConnected = connector.IsConnected,
                        IsEndCap = false, // Revit doesn't expose end-cap directly
                        Description = $"Connector {index}"
                    });
                    index++;
                }
            }
            catch { }
            return result;
        }

        private static Dictionary<string, string> GetParameters(Element element)
        {
            var result = new Dictionary<string, string>();
            try
            {
                foreach (Parameter param in element.Parameters)
                {
                    if (param.Definition == null) continue;
                    var name = param.Definition.Name;
                    if (string.IsNullOrEmpty(name) || result.ContainsKey(name)) continue;

                    var value = param.AsValueString() ?? param.AsString() ?? string.Empty;
                    result[name] = value;
                }

                // Also get type parameters
                var typeId = element.GetTypeId();
                if (typeId != null && typeId != ElementId.InvalidElementId)
                {
                    var type = element.Document.GetElement(typeId);
                    if (type != null)
                    {
                        foreach (Parameter param in type.Parameters)
                        {
                            if (param.Definition == null) continue;
                            var name = param.Definition.Name;
                            if (string.IsNullOrEmpty(name) || result.ContainsKey(name)) continue;

                            var value = param.AsValueString() ?? param.AsString() ?? string.Empty;
                            result[name] = value;
                        }
                    }
                }
            }
            catch { }
            return result;
        }

        private static GeometryData? GetGeometryData(Element element)
        {
            try
            {
                if (element.Location is LocationCurve locCurve)
                {
                    var curve = locCurve.Curve;
                    var start = curve.GetEndPoint(0);
                    var end = curve.GetEndPoint(1);

                    var geometry = new GeometryData
                    {
                        StartPoint = new PointData(
                            start.X * FeetToMetres,
                            start.Y * FeetToMetres,
                            start.Z * FeetToMetres),
                        EndPoint = new PointData(
                            end.X * FeetToMetres,
                            end.Y * FeetToMetres,
                            end.Z * FeetToMetres)
                    };

                    // Get pipe diameter
                    if (element is Pipe pipe)
                    {
                        var diamParam = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                        if (diamParam != null)
                            geometry.Diameter = diamParam.AsDouble() * FeetToMetres * 1000; // to mm
                    }
                    else if (element is Duct duct)
                    {
                        var diamParam = duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
                        if (diamParam != null)
                            geometry.Diameter = diamParam.AsDouble() * FeetToMetres * 1000;
                    }

                    // Get system name
                    var sysNameParam = element.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
                    if (sysNameParam != null)
                        geometry.SystemName = sysNameParam.AsString() ?? string.Empty;

                    var sysClassParam = element.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
                    if (sysClassParam != null)
                        geometry.SystemClassification = sysClassParam.AsString() ?? string.Empty;

                    return geometry;
                }
            }
            catch { }
            return null;
        }

        private RoomData? ConvertRoom(Autodesk.Revit.DB.Architecture.Room room)
        {
            try
            {
                if (room.Area <= 0) return null;

                var roomData = new RoomData
                {
                    Id = room.Id.Value,
                    Name = room.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString() ?? room.Name ?? string.Empty,
                    Level = room.Level?.Name ?? string.Empty,
                    Area = room.Area * FeetToMetres * FeetToMetres // sq ft to sq m
                };

                // Extract boundary polygon
                var boundaries = room.GetBoundarySegments(new SpatialElementBoundaryOptions());
                if (boundaries != null && boundaries.Count > 0)
                {
                    var outerLoop = boundaries[0]; // First loop is the outer boundary
                    foreach (var segment in outerLoop)
                    {
                        var curve = segment.GetCurve();
                        var pt = curve.GetEndPoint(0);
                        roomData.BoundaryPoints.Add(new PointData(
                            pt.X * FeetToMetres,
                            pt.Y * FeetToMetres,
                            pt.Z * FeetToMetres));
                    }
                }

                return roomData;
            }
            catch { }
            return null;
        }
    }
}
