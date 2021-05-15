using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using UCalc.Controls;
using UCalc.Data;

namespace UCalc
{
    public static class Constants
    {
        public static readonly SolidColorBrush MainColor = Brushes.White;
        public static readonly SolidColorBrush SubMainColor = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));

        public const int InternalPrecision = 6;
        public const int DisplayPrecision = 2;
        public const string DateFormat = "dd.MM.yyyy";

        public static readonly string InternalPrecisionFormat =
            Helpers.PrecisionToFormat(InternalPrecision, InternalPrecision - 2);

        public static readonly string DisplayPrecisionFormat = Helpers.PrecisionToFormat(DisplayPrecision);

        public static readonly ImmutableList<string> SalutationStrs =
            ((Salutation[]) Enum.GetValues(typeof(Salutation))).Select(value => value.AsString()).ToImmutableList();

        public static readonly ImmutableList<string> CostDivisionStrs =
            ((CostDivision[]) Enum.GetValues(typeof(CostDivision))).Select(value => value.AsString()).ToImmutableList();

        public static readonly double DinA4Width;
        public static readonly double DinA4Height;
        public static readonly Thickness DinA4Padding;

        public static double DinA4ContentWidth => DinA4Width - DinA4Padding.Left - DinA4Padding.Right;

        public static readonly double PrintDefaultFontSize;
        public static readonly double PrintSubjectFontSize;
        public static readonly double PrintNewlineFontSize;

        static Constants()
        {
            var converter = new LengthConverter();
            // ReSharper disable PossibleNullReferenceException
            DinA4Width = (double) converter.ConvertFromInvariantString("29.7cm");
            DinA4Height = (double) converter.ConvertFromInvariantString("42cm");
            var dinA4MarginLeftRight = (double) converter.ConvertFromInvariantString("3.18cm");
            var dinA4MarginTopBottom = (double) converter.ConvertFromInvariantString("2.54cm");

            PrintDefaultFontSize = (double) converter.ConvertFromInvariantString("11pt");
            PrintSubjectFontSize = (double) converter.ConvertFromInvariantString("12pt");
            PrintNewlineFontSize = (double) converter.ConvertFromInvariantString("11pt");
            // ReSharper restore PossibleNullReferenceException

            DinA4Padding = new Thickness(dinA4MarginLeftRight, dinA4MarginTopBottom, dinA4MarginLeftRight,
                dinA4MarginTopBottom);
        }
    }
}