using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using UCalc.Annotations;

namespace UCalc.Controls
{
    public class ErrorsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (((ICollection<string>) value)?.Count ?? 0) == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class ErrorsToInVisibilityConverter : IValueConverter
    {
        private readonly ErrorsToVisibilityConverter _converter = new ErrorsToVisibilityConverter();

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

    public partial class ErrorIcon : INotifyPropertyChanged
    {
        public ICollection<string> Errors { get; set; }

        public string ErrorsToolTip { get; private set; }

        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(
            "Errors", typeof(ICollection<string>), typeof(ErrorIcon),
            new PropertyMetadata(null, OnErrorsChanged));

        public ErrorIcon()
        {
            InitializeComponent();
        }

        private static void OnErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ErrorIcon) d;
            var newErrors = (ICollection<string>) e.NewValue;
            self.ErrorsToolTip = newErrors.Count > 0 ? string.Join("\n", newErrors) : null;
            self.OnPropertyChanged("ErrorsToolTip");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}