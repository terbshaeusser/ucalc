using UCalc.Data;

namespace UCalc
{
    public partial class BillingWindow
    {
        private readonly Billing _savedBilling;
        public Billing Billing { get; }

        public BillingWindow(Billing savedBilling, Billing billing)
        {
            _savedBilling = savedBilling;
            Billing = billing;
            InitializeComponent();
        }
    }
}