using System;
using System.Windows;
using System.Windows.Data;
using UCalc.Models;

namespace UCalc.Controls
{
    public class ErrorCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((int) value) != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public partial class ErrorCounter
    {
        public Model Model { get; set; }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof(Model), typeof(ErrorCounter), new PropertyMetadata((Model) null));

        public ErrorCounter()
        {
            InitializeComponent();
        }
    }
}