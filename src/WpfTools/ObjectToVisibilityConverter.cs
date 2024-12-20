using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mastersign.WpfTools
{
    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class ObjectToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public Visibility Invisible { get; set; } = Visibility.Collapsed;

        private bool Transform(bool value) => Invert ? !value : value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Transform(value != null)
                ? Visibility.Visible
                : Invisible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
