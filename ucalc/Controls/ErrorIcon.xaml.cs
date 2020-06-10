using System;
using System.Windows;
using System.Windows.Data;

namespace UCalc.Controls
{
    public class ErrorToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || "".Equals(value))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class ErrorToInVisibilityConverter : IValueConverter
    {
        private readonly ErrorToVisibilityConverter _converter = new ErrorToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
            return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public partial class ErrorIcon
    {
        public string Error { get; set; }

        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(string), typeof(ErrorIcon), new PropertyMetadata((string) null));

        public ErrorIcon()
        {
            InitializeComponent();
        }
    }
}