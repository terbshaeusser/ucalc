using System.Windows;
using UCalc.Models;

namespace UCalc.Controls
{
    public partial class ErrorCounter
    {
        public Model Model { get; set; }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof(Model), typeof(ErrorCounter), new PropertyMetadata((Model) null));

        public ErrorCounter()
        {
            InitializeComponent();
        }
    }
}