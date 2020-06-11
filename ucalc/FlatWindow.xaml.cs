using System.Windows;
using UCalc.Models;

namespace UCalc
{
    public partial class FlatWindow
    {
        public FlatModel Model { get; }

        public FlatWindow(FlatModel model)
        {
            Model = model;
            InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}