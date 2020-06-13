using System.Windows;
using UCalc.Models;

namespace UCalc
{
    public partial class FlatWindow
    {
        public FlatProperty Flat { get; }

        public FlatWindow(FlatProperty flat)
        {
            Flat = flat;
            InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}