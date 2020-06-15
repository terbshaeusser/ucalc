using System.Windows;
using System.Windows.Controls;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc
{
    public partial class TenantWindow
    {
        public Model Model { get; }
        public TenantProperty Tenant { get; }
        public HouseProperty House { get; }

        public TenantWindow(Model model, TenantProperty tenant, HouseProperty house)
        {
            Model = model;
            Tenant = tenant;
            House = house;
            InitializeComponent();

            ((FlatToRentedConverter) FindResource("FlatToRentedConverter")).Tenant = Tenant;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnFlatChecked(object sender, RoutedEventArgs e)
        {
            var flat = (FlatProperty) ((CheckBox) sender).DataContext;

            Tenant.RentedFlats.Add(flat);
        }

        private void OnFlatUnchecked(object sender, RoutedEventArgs e)
        {
            var flat = (FlatProperty) ((CheckBox) sender).DataContext;

            Tenant.RentedFlats.Remove(flat);
        }
    }
}