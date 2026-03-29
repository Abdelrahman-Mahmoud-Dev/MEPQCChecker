using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace MEPQCChecker.Revit.Ribbon
{
    public static class RibbonSetup
    {
        public static void CreateRibbon(UIControlledApplication app)
        {
            var tabName = "MEP Tools";
            try
            {
                app.CreateRibbonTab(tabName);
            }
            catch
            {
                // Tab may already exist
            }

            var panel = app.CreateRibbonPanel(tabName, "QC Checker");
            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Run QC Check button
            var runData = new PushButtonData(
                "RunQCCheck",
                "Run QC\nCheck",
                assemblyPath,
                "MEPQCChecker.Revit.Commands.RunQCCheckCommand");
            var runButton = panel.AddItem(runData) as PushButton;
            if (runButton != null)
            {
                runButton.ToolTip = "Scan the active model for MEP quality issues and clashes";
                SetButtonIcon(runButton, "icon_run_32.png");
            }

            // Clear Highlights button
            var clearData = new PushButtonData(
                "ClearHighlights",
                "Clear\nHighlights",
                assemblyPath,
                "MEPQCChecker.Revit.Commands.ClearOverridesCommand");
            var clearButton = panel.AddItem(clearData) as PushButton;
            if (clearButton != null)
            {
                clearButton.ToolTip = "Remove all QC color overrides from the active view";
                SetButtonIcon(clearButton, "icon_clear_32.png");
            }
        }

        private static void SetButtonIcon(RibbonButton button, string iconFileName)
        {
            try
            {
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (assemblyDir == null) return;

                var iconPath = Path.Combine(assemblyDir, "Icons", iconFileName);
                if (File.Exists(iconPath))
                {
                    var uri = new Uri(iconPath);
                    button.LargeImage = new BitmapImage(uri);
                }
            }
            catch
            {
                // Icon missing or invalid — fall back to text-only button (no crash)
            }
        }
    }
}
