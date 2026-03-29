using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Autodesk.Revit.UI;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Revit.UI
{
    public partial class QCDashboardPanel : Page, IDockablePaneProvider
    {
        private QCReport? _currentReport;
        private List<QCIssue> _allIssues = new List<QCIssue>();

        // External events for Revit thread-safe operations
        public static ExternalEvent? RunCheckEvent { get; set; }
        public static ExternalEvent? ZoomToElementEvent { get; set; }
        public static ZoomToElementHandler? ZoomHandler { get; set; }

        public QCDashboardPanel()
        {
            InitializeComponent();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right
            };
        }

        public void Bind(QCReport? report)
        {
            _currentReport = report;

            if (report == null)
            {
                CriticalCount.Text = "0";
                WarningCount.Text = "0";
                InfoCount.Text = "0";
                _allIssues.Clear();
                IssueList.ItemsSource = null;
                StatusText.Text = "Ready — click Run QC Check to start";
                return;
            }

            CriticalCount.Text = report.CriticalCount.ToString();
            WarningCount.Text = report.WarningCount.ToString();
            InfoCount.Text = report.InfoCount.ToString();

            _allIssues = report.Issues.ToList();
            ApplyFilters();

            StatusText.Text = $"Last run: {report.RunAt:HH:mm:ss} · {report.TotalCount} issues · Model: {report.ProjectName}";
        }

        private void BtnRunCheck_Click(object sender, RoutedEventArgs e)
        {
            RunCheckEvent?.Raise();
        }

        private void BtnClearHighlights_Click(object sender, RoutedEventArgs e)
        {
            // Clear handled via external event pattern
            Bind(null);
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allIssues == null) return;

            var filtered = _allIssues.AsEnumerable();

            var disciplineItem = DisciplineFilter?.SelectedItem as ComboBoxItem;
            var discipline = disciplineItem?.Content?.ToString();
            if (!string.IsNullOrEmpty(discipline) && discipline != "All")
            {
                filtered = filtered.Where(i => i.Discipline == discipline);
            }

            var severityItem = SeverityFilter?.SelectedItem as ComboBoxItem;
            var severity = severityItem?.Content?.ToString();
            if (!string.IsNullOrEmpty(severity) && severity != "All")
            {
                var severityEnum = severity switch
                {
                    "Critical" => QCSeverity.Critical,
                    "Warning" => QCSeverity.Warning,
                    "Info" => QCSeverity.Info,
                    _ => (QCSeverity?)null
                };
                if (severityEnum.HasValue)
                    filtered = filtered.Where(i => i.Severity == severityEnum.Value);
            }

            IssueList.ItemsSource = filtered.ToList();
        }

        private void IssueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IssueList.SelectedItem is QCIssue issue && ZoomHandler != null)
            {
                ZoomHandler.TargetElementId = issue.ElementId;
                ZoomToElementEvent?.Raise();
            }
        }
    }
}
