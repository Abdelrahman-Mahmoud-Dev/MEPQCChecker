using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MEPQCChecker.Core.Services;
using MEPQCChecker.Revit.Adapters;
using MEPQCChecker.Revit.Services;

namespace MEPQCChecker.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class RunQCCheckCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiDoc = commandData.Application.ActiveUIDocument;
                var doc = uiDoc.Document;

                // 1. Build snapshot (reads model, no transaction needed)
                var snapshot = new RevitModelAdapter(doc).BuildSnapshot();

                // 2. Run all checks (pure C#, no Revit API)
                var config = ConfigService.Load();
                var report = new CheckRunner(config).RunAll(snapshot);

                // 3. Apply color overrides (needs transaction)
                using (var tx = new Transaction(doc, "MEP QC - Apply Color Overrides"))
                {
                    tx.Start();
                    new ColorOverrideService(doc, uiDoc.ActiveView).ApplyOverrides(report);
                    tx.Commit();
                }

                // 4. Push results to dashboard
                App.Instance?.UpdateReport(report);

                TaskDialog.Show("MEP QC Checker",
                    $"QC Check Complete\n\n" +
                    $"Critical: {report.CriticalCount}\n" +
                    $"Warning: {report.WarningCount}\n" +
                    $"Info: {report.InfoCount}\n" +
                    $"Total: {report.TotalCount} issues found");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                App.LogError("RunQCCheckCommand", ex);
                TaskDialog.Show("MEP QC Checker - Error",
                    $"An error occurred during the QC check:\n\n{ex.Message}\n\nSee log file for details.");
                return Result.Failed;
            }
        }
    }
}
