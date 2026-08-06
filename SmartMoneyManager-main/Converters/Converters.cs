using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartMoneyManager.Converters
{
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is bool b && b ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    public class InvBoolToVisConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    // Shows Visible when string non-empty, Collapsed when null/empty
    public class StrToVisConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => !string.IsNullOrEmpty(v as string) ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    public class PercentToColorConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
        {
            double d = v is double d2 ? d2 : 0;
            if (d >= 90) return new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
            if (d >= 70) return new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
            return new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4));
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    public class SeverityToColorConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => (v as string) switch
            {
                "Alert"   => new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50)),
                "Warning" => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
                _         => new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xD4)),
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    public class EditLabelConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is bool b && b ? "Update" : "Save";
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    public class AmountColorConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
        {
            double d = v is double d2 ? d2 : (v is decimal dec ? (double)dec : 0);
            return d >= 0
                ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))
                : new SolidColorBrush(Color.FromRgb(0xEF, 0x53, 0x50));
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }
}
