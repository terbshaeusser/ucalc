﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using UCalc.Models;

namespace UCalc.Controls
{
    public class ErrorsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (((ICollection<string>) value)?.Count ?? 0) == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class ErrorsToInVisibilityConverter : IValueConverter
    {
        private readonly ErrorsToVisibilityConverter _converter = new ErrorsToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = _converter.Convert(value, targetType, parameter, culture) as Visibility?;
            return result == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class ErrorsToCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection<string>) value)?.Count ?? 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class ErrorsToToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var errors = (IReadOnlyList<string>) value;

            return (errors?.Count ?? 0) == 0 ? null : string.Join("\n", errors);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class FlatToRentedConverter : IValueConverter
    {
        public TenantProperty Tenant { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Tenant.RentedFlats.Contains((FlatProperty) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class FlatToAffectedConverter : IValueConverter
    {
        public CostProperty Cost { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Cost.AffectedFlats.Contains((FlatProperty) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class EmptyMultiPropertyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(0) ?? false ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class NameToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string) value;

            if (str == "")
            {
                return "(Unbenannt)";
            }

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class CostToAffectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cost = (CostProperty) value;

            if (cost == null)
            {
                return "";
            }

            if (cost.AffectsAll.Value)
            {
                return "Betrifft: Alle Wohnungen";
            }

            if (cost.AffectedFlats.Count == 0)
            {
                return "Betrifft: Niemanden";
            }

            return $"Betrifft: {string.Join(", ", cost.AffectedFlats.Select(flat => flat.Name.Value))}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool?) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool?) value;
        }
    }
}