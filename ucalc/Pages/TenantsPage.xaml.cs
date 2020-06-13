using System.Windows;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc.Pages
{
    public partial class TenantsPage
    {
        public Window ParentWindow { get; set; }
        public TenantsProperty Tenants { get; set; }
        public HouseProperty House { get; set; }

        public TenantsPage()
        {
            InitializeComponent();
        }

        private void OnAddTenantClick(object sender, RoutedEventArgs e)
        {
            Tenants.Add();
        }

        private void OnTenantDeleteClick(object sender, RoutedEventArgs e)
        {
            var tenant = (TenantProperty) ((HighlightButton) sender).DataContext;

            if (MessageBox.Show($"Möchten Sie den Mieter \"{tenant.Name.Value}\" wirlick löschen?", "Löschen?",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Tenants.Remove(tenant);
            }
        }

        private void OnTenantEditClick(object sender, RoutedEventArgs e)
        {
            var tenant = (TenantProperty) ((HighlightButton) sender).DataContext;

            new TenantWindow(tenant, House) {Owner = ParentWindow}.ShowDialog();
        }
    }
}