using System.Windows;
using UCalc.Models;

namespace UCalc
{
    public partial class CostWindow
    {
        public Model Model { get; }
        public CostProperty Cost { get; }
        public HouseProperty House { get; }

        public CostWindow(Model model, CostProperty cost, HouseProperty house)
        {
            Model = model;
            Cost = cost;
            House = house;

            InitializeComponent();
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}