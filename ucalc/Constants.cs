using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Media;
using UCalc.Data;

namespace UCalc
{
    public static class Constants
    {
        public static readonly SolidColorBrush MainColor = Brushes.White;
        public static readonly SolidColorBrush SubMainColor = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));

        public const string DecimalFormat = "0.00";
        public const string DateFormat = "dd.MM.yyyy";

        public static readonly ImmutableList<string> SalutationStrs =
            ((Salutation[]) Enum.GetValues(typeof(Salutation))).Select(value => value.AsString()).ToImmutableList();

        public static readonly ImmutableList<string> CostDivisionStrs =
            ((CostDivision[]) Enum.GetValues(typeof(CostDivision))).Select(value => value.AsString()).ToImmutableList();
    }
}