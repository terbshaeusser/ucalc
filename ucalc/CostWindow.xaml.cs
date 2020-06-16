using System.Windows;
using System.Windows.Controls;
using UCalc.Controls;
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

            ((FlatToAffectedConverter) FindResource("FlatToAffectedConverter")).Cost = Cost;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnFlatChecked(object sender, RoutedEventArgs e)
        {
            var flat = (FlatProperty) ((CheckBox) sender).DataContext;

            Cost.AffectedFlats.Add(flat);
        }

        private void OnFlatUnchecked(object sender, RoutedEventArgs e)
        {
            var flat = (FlatProperty) ((CheckBox) sender).DataContext;

            Cost.AffectedFlats.Remove(flat);
        }

        private void OnCostEntryDeleteClick(object sender, RoutedEventArgs e)
        {
            var entry = (CostEntryProperty) ((HighlightButton) sender).DataContext;

            Cost.Entries.Remove(entry);
        }

        private void OnAddCostEntryClick(object sender, RoutedEventArgs e)
        {
            Cost.Entries.Add();
        }
    }
}