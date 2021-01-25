using System.Collections.ObjectModel;
using System.Windows.Controls;
using UCalc.Data;
using UCalc.Models;

namespace UCalc.Pages
{
    public class DetailsItem
    {
        public Tenant Tenant { get; }
        public string Name => Tenant != null ? Tenant.Name : "Kostenübersicht";

        public DetailsItem(Tenant tenant)
        {
            Tenant = tenant;
        }
    }

    public partial class DetailsPage
    {
        public Model Model { get; set; }
        private Billing _billing;
        public ObservableCollection<DetailsItem> Items { get; }

        public DetailsPage()
        {
            Items = new ObservableCollection<DetailsItem>();
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
            Items.Clear();
            Items.Add(new DetailsItem(null));
            foreach (var tenant in _billing.Tenants)
            {
                Items.Add(new DetailsItem(tenant));
            }

            TenantComboBox.IsEnabled = true;
            TenantComboBox.SelectedIndex = -1;
            OnSelectedTenantChanged(TenantComboBox, null);
        }

        private void OnSelectedTenantChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (DetailsItem) ((ComboBox) sender).SelectedItem;

            if (item == null)
            {
                CalculationTextBox.Text = "Bitte wählen Sie einen Mieter aus, um die Berechnungen anzuzeigen.";
                return;
            }

            if (item.Tenant == null)
            {
                CalculationTextBox.Text = BillingCalculator.CalculateCostOverview(_billing);
                return;
            }

            var result = BillingCalculator.CalculateForTenant(_billing, item.Tenant);

            CalculationTextBox.Text = result.DetailsForLandlord;
        }
    }
}