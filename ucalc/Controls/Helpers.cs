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
    }
}