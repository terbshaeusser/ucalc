using UCalc.Data;

namespace UCalc.Models
{
    public class BillingModel : Model
    {
        public LandlordModel LandlordModel { get; }
        public HouseModel HouseModel { get; }

        public BillingModel(Billing billing)
        {
            LandlordModel = new LandlordModel(billing.Landlord);
            HouseModel = new HouseModel(billing.House);
        }

        public override void Apply()
        {
            LandlordModel.Apply();
            HouseModel.Apply();
        }
    }
}