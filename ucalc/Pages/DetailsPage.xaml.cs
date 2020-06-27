using System.Collections.ObjectModel;
using System.Windows.Controls;
using UCalc.Data;
using UCalc.Models;

namespace UCalc.Pages
{
    public partial class DetailsPage
    {
        public Model Model { get; set; }
        private Billing _billing;
        public ObservableCollection<Tenant> Tenants { get; }

        public DetailsPage()
        {
            Tenants = new ObservableCollection<Tenant>();
            InitializeComponent();
        }

        public void Compute()
        {
            if (Model.Root.Errors.Count > 0)
            {
                TenantComboBox.IsEnabled = false;
                CalculationTextBox.Text =
                    "Bitte beheben Sie zunächst alle Fehler bevor eine Berechnung vorgenommen werden kann.";
                return;
            }

            _billing = Model.Dump();
            Tenants.Clear();
            foreach (var tenant in _billing.Tenants)
            {
                Tenants.Add(tenant);
            }

            TenantComboBox.IsEnabled = true;
            TenantComboBox.SelectedIndex = -1;
            OnSelectedTenantChanged(TenantComboBox, null);
        }

        private void OnSelectedTenantChanged(object sender, SelectionChangedEventArgs e)
        {
            var tenant = (Tenant) ((ComboBox) sender).SelectedItem;

            if (tenant == null)
            {
                CalculationTextBox.Text = "Bitte wählen Sie einen Mieter aus, um die Berechnungen anzuzeigen.";
                return;
            }

            var result = BillingCalculator.CalculateForTenant(_billing, tenant);

            CalculationTextBox.Text = result.DetailsForLandlord;
        }
    }
}