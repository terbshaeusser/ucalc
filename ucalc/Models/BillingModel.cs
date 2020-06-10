using UCalc.Data;

namespace UCalc.Models
{
    public class BillingModel : Model
    {
        public LandlordModel LandlordModel { get; }

        public BillingModel(Billing billing)
        {
            LandlordModel = new LandlordModel(billing.Landlord);

            Properties = LandlordModel.Properties;
        }

        public override void Apply()
        {
            LandlordModel.Apply();
        }
    }
}