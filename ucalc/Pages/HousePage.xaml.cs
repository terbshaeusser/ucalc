using System.Windows;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc.Pages
{
    public partial class HousePage
    {
        public Window ParentWindow { get; set; }
        public HouseModel Model { get; set; }

        public HousePage()
        {
            InitializeComponent();
        }

        private void OnAddFlatClick(object sender, RoutedEventArgs e)
        {
            Model.AddFlat();
        }

        private void OnFlatDeleteClick(object sender, RoutedEventArgs e)
        {
            var flatModel = (FlatModel) ((HighlightButton) sender).DataContext;

            if (MessageBox.Show($"Möchten Sie die Wohnung \"{flatModel.Data.Name}\" wirlick löschen?", "Löschen?",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Model.RemoveFlat(flatModel);
            }
        }

        private void OnFlatEditClick(object sender, RoutedEventArgs e)
        {
            var flatModel = (FlatModel) ((HighlightButton) sender).DataContext;

            new FlatWindow(flatModel) {Owner = ParentWindow}.ShowDialog();
        }
    }
}