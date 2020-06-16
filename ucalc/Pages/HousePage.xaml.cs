using System.Windows;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc.Pages
{
    public partial class HousePage
    {
        public Window ParentWindow { get; set; }
        public HouseProperty House { get; set; }

        public HousePage()
        {
            InitializeComponent();
        }

        private void OnAddFlatClick(object sender, RoutedEventArgs e)
        {
            var flat = House.Flats.Add();

            new FlatWindow(flat) {Owner = ParentWindow}.ShowDialog();
        }

        private void OnFlatDeleteClick(object sender, RoutedEventArgs e)
        {
            var flat = (FlatProperty) ((HighlightButton) sender).DataContext;

            if (MessageBox.Show($"Möchten Sie die Wohnung \"{flat.Name.Value}\" wirlick löschen?", "Löschen?",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                House.Flats.Remove(flat);
            }
        }

        private void OnFlatEditClick(object sender, RoutedEventArgs e)
        {
            var flat = (FlatProperty) ((HighlightButton) sender).DataContext;

            new FlatWindow(flat) {Owner = ParentWindow}.ShowDialog();
        }
    }
}