using System.Collections.Generic;
using UCalc.Data;

namespace UCalc.Models
{
    public class BillingProperty : NestedProperty
    {
        public LandlordProperty Landlord { get; }
        public HouseProperty House { get; }
        public TenantsProperty Tenants { get; }


        // TODO: Costs

        public BillingProperty(Model model, Property parent, Billing data) : base(model, parent)
        {
            Landlord = Add(new LandlordProperty(model, this, data.Landlord));
            House = Add(new HouseProperty(model, this, data.House));

            var flatToProperty = new Dictionary<Flat, FlatProperty>();
            for (var i = 0; i < data.House.Flats.Count; ++i)
            {
                flatToProperty.Add(data.House.Flats[i], House.Flats[i]);
            }

            Tenants = Add(new TenantsProperty(model, this, data.Tenants, flatToProperty));
        }
    }
}