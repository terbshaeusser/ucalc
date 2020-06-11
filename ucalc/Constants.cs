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

        public static readonly ImmutableList<string> SalutationStrs =
            ((Salutation[]) Enum.GetValues(typeof(Salutation))).Select(value => value.AsString()).ToImmutableList();
    }
}