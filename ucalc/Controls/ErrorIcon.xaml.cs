using System.Windows;
using UCalc.Models;

namespace UCalc.Controls
{
    public partial class ErrorIcon
    {
        public Property Property { get; set; }

        public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(
            "Property", typeof(Property), typeof(ErrorIcon), new PropertyMetadata((Property) null));

        public ErrorIcon()
        {
            InitializeComponent();
        }
    }
}