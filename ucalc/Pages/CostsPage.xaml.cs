using System.Windows;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc.Pages
{
    public partial class CostsPage
    {
        public Window ParentWindow { get; set; }
        public Model Model { get; set; }
        public CostsProperty Costs { get; set; }
        public HouseProperty House { get; set; }

        public CostsPage()
        {
            InitializeComponent();
        }

        private void OnAddCostClick(object sender, RoutedEventArgs e)
        {
            var cost = Costs.Add();
            new CostWindow(Model, cost, House) {Owner = ParentWindow}.ShowDialog();
        }

        private void OnCostDeleteClick(object sender, RoutedEventArgs e)
        {
            var cost = (CostProperty) ((HighlightButton) sender).DataContext;

            if (MessageBox.Show($"Möchten Sie den Kostenpunkt \"{cost.Name.Value}\" wirlick löschen?", "Löschen?",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Costs.Remove(cost);
            }
        }

        private void OnCostEditClick(object sender, RoutedEventArgs e)
        {
            var cost = (CostProperty) ((HighlightButton) sender).DataContext;

            new CostWindow(Model, cost, House) {Owner = ParentWindow}.ShowDialog();
        }
    }
}