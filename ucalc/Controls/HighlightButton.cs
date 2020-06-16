using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace UCalc.Controls
{
    public class HighlightButton : DockPanel
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler),
                typeof(HighlightButton));

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public SolidColorBrush HighlightForeground { get; set; }
        public SolidColorBrush HighlightBackground { get; set; }
        public bool Selectable { get; set; }
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected && !value)
                {
                    Unhighlight();
                }
                else if (value)
                {
                    if (!_selected)
                    {
                        Highlight();
                    }

                    RaiseEvent(new RoutedEventArgs(ClickEvent));
                }

                _selected = value;
            }
        }

        public HighlightButton()
        {
            HighlightForeground = Brushes.White;
            HighlightBackground = Brushes.Black;
        }

        private void SetColors(SolidColorBrush foreground, SolidColorBrush background)
        {
            Background = background;

            foreach (var child in Children)
            {
                switch (child)
                {
                    case Label label:
                        label.Foreground = foreground;
                        break;
                    case TextBlock block:
                        block.Foreground = foreground;
                        break;
                    case Path path:
                        path.Fill = foreground;
                        break;
                    case Viewbox viewbox:
                        viewbox.ChangeColor(foreground);
                        break;
                }
            }
        }

        private void Highlight()
        {
            SetColors(HighlightBackground, HighlightForeground);
        }

        private void Unhighlight()
        {
            SetColors(HighlightForeground, HighlightBackground);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Selected)
            {
                Highlight();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (!Selected)
            {
                Unhighlight();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (Selectable)
            {
                Selected = true;
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(ClickEvent));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!Selected)
            {
                Highlight();
                Unhighlight();
            }
        }
    }
}