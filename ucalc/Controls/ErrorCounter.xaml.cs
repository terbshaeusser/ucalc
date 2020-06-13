using System.Windows;
using UCalc.Models;

namespace UCalc.Controls
{
    public partial class ErrorCounter
    {
        public Property Property { get; set; }

        public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(
            "Property", typeof(Property), typeof(ErrorCounter), new PropertyMetadata((Property) null));

        public ErrorCounter()
        {
            InitializeComponent();
        }
    }
}