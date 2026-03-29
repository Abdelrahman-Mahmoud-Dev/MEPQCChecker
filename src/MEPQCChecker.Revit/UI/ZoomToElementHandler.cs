using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace MEPQCChecker.Revit.UI
{
    public class ZoomToElementHandler : IExternalEventHandler
    {
        public long TargetElementId { get; set; }

        public void Execute(UIApplication app)
        {
            try
            {
                var uiDoc = app.ActiveUIDocument;
                if (uiDoc == null) return;

                var elementId = new ElementId(TargetElementId);
                var element = uiDoc.Document.GetElement(elementId);
                if (element == null) return;

                uiDoc.ShowElements(elementId);
                uiDoc.Selection.SetElementIds(new List<ElementId> { elementId });
            }
            catch (Exception ex)
            {
                App.LogError("ZoomToElement", ex);
            }
        }

        public string GetName() => "MEPQCChecker.ZoomToElement";
    }
}
