using System;
using System.IO;
using Autodesk.Revit.UI;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Revit
{
    public class App : IExternalApplication
    {
        public static App? Instance { get; private set; }

        public static readonly Guid DockablePaneGuid = new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

        public QCReport? LastReport { get; private set; }

        private static readonly string LogPath = Path.Combine(
            Path.GetDirectoryName(typeof(App).Assembly.Location) ?? "",
            "MEPQCChecker.log");

        public Result OnStartup(UIControlledApplication app)
        {
            try
            {
                Instance = this;
                Ribbon.RibbonSetup.CreateRibbon(app);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                LogError("OnStartup failed", ex);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            Instance = null;
            return Result.Succeeded;
        }

        public void UpdateReport(QCReport report)
        {
            LastReport = report;
        }

        public static void LogError(string context, Exception ex)
        {
            try
            {
                var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(LogPath, message);
            }
            catch
            {
                // If logging fails, don't crash the plugin
            }
        }
    }
}
