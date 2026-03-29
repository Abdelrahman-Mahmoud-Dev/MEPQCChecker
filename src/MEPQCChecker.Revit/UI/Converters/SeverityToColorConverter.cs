using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MEPQCChecker.Core.Models;

namespace MEPQCChecker.Revit.UI.Converters
{
    public class SeverityToColorConverter : IValueConverter
    {
        private static readonly SolidColorBrush CriticalBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 50, 50));
        private static readonly SolidColorBrush WarningBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 160, 0));
        private static readonly SolidColorBrush InfoBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 150));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QCSeverity severity)
            {
                return severity switch
                {
                    QCSeverity.Critical => CriticalBrush,
                    QCSeverity.Warning => WarningBrush,
                    _ => InfoBrush
                };
            }
            return InfoBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
