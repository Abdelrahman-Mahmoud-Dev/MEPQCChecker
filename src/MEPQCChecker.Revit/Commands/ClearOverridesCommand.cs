using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MEPQCChecker.Revit.Services;

namespace MEPQCChecker.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ClearOverridesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;

                using (var tx = new Transaction(doc, "MEP QC - Clear Overrides"))
                {
                    tx.Start();
                    new ColorOverrideService(doc, uiDoc.ActiveView).ClearOverrides();
                    tx.Commit();
                }

                App.Instance?.UpdateReport(null!);

                TaskDialog.Show("MEP QC Checker", "All QC highlights have been cleared.");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                App.LogError("ClearOverridesCommand", ex);
                TaskDialog.Show("MEP QC Checker - Error",
                    $"An error occurred:\n\n{ex.Message}\n\nSee log file for details.");
                return Result.Failed;
            }
        }
    }
}
