using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using MEPQCChecker.Core.Models;
using MEPQCChecker.Core.Services;

namespace MEPQCChecker.Revit.Services
{
    public class ColorOverrideService
    {
        private static readonly Color CriticalColor = new Color(220, 50, 50);
        private static readonly Color WarningColor = new Color(230, 160, 0);

        private readonly Document _doc;
        private readonly View _view;

        public ColorOverrideService(Document doc, View view)
        {
            _doc = doc;
            _view = view;
        }

        public void ApplyOverrides(QCReport report)
        {
            ClearOverrides();

            var plan = HighlightPlan.FromReport(report);

            var solidFill = GetSolidFillPatternId();

            // Apply critical (red) overrides
            var criticalSettings = new OverrideGraphicSettings();
            criticalSettings.SetSurfaceForegroundPatternColor(CriticalColor);
            if (solidFill != ElementId.InvalidElementId)
                criticalSettings.SetSurfaceForegroundPatternId(solidFill);

            foreach (var id in plan.CriticalElementIds)
            {
                var elementId = new ElementId(id);
                if (_doc.GetElement(elementId) != null)
                    _view.SetElementOverrides(elementId, criticalSettings);
            }

            // Apply warning (amber) overrides
            var warningSettings = new OverrideGraphicSettings();
            warningSettings.SetSurfaceForegroundPatternColor(WarningColor);
            if (solidFill != ElementId.InvalidElementId)
                warningSettings.SetSurfaceForegroundPatternId(solidFill);

            foreach (var id in plan.WarningElementIds)
            {
                var elementId = new ElementId(id);
                if (_doc.GetElement(elementId) != null)
                    _view.SetElementOverrides(elementId, warningSettings);
            }
        }

        public void ClearOverrides()
        {
            var defaultSettings = new OverrideGraphicSettings();
            var collector = new FilteredElementCollector(_doc, _view.Id)
                .WhereElementIsNotElementType();

            foreach (var element in collector)
            {
                _view.SetElementOverrides(element.Id, defaultSettings);
            }
        }

        private ElementId GetSolidFillPatternId()
        {
            var collector = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement));

            foreach (FillPatternElement fpe in collector)
            {
                var pattern = fpe.GetFillPattern();
                if (pattern != null && pattern.IsSolidFill)
                    return fpe.Id;
            }

            return ElementId.InvalidElementId;
        }
    }
}
