using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UCalc.Controls
{
    public static class Helpers
    {
        public static IEnumerable<T> FindChildren<T>(this Panel parent) where T : FrameworkElement
        {
            foreach (var child in parent.Children)
            {
                if (child is T t)
                {
                    yield return t;
                }
            }
        }

        public static void ChangeColor(this Viewbox viewbox, SolidColorBrush brush)
        {
            foreach (var path in ((Canvas) viewbox.Child).FindChildren<Path>())
            {
                path.Fill = brush;
            }
        }

        private static bool IsBetween(this DateTime dt, DateTime start, DateTime end)
        {
            return dt >= start && dt <= end;
        }

        public static bool Intersects(this DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1.IsBetween(start2, end2) || start2.IsBetween(start1, end1);
        }

        public static string PrecisionToFormat(int precision, int optional = 0)
        {
            return $"0.{new string('0', precision - optional)}{new string('#', optional)}";
        }
    }
}