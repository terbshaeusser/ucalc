using System.Collections.Generic;
using System.Linq;
using UCalc.Data;

namespace UCalc.Models
{
    public class TenantsProperty : MultiProperty<TenantProperty>
    {
        public TenantsProperty(Model model, Property parent, IEnumerable<Tenant> data,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent,
            "Mieter: Geben Sie einen oder mehr Mieter an.")
        {
            using var validator = Model.BeginValidation();

            foreach (var tenant in data)
            {
                base.Add(new TenantProperty(Model, this, tenant, flatToProperty));
            }

            Modified = false;
        }

        public TenantProperty Add()
        {
            using var validator = Model.BeginValidation();

            var tenant = new TenantProperty(Model, this, new Tenant());
            base.Add(tenant);
            return tenant;
        }

        public new void Remove(TenantProperty tenant)
        {
            using var validator = Model.BeginValidation();

            base.Remove(tenant);

            validator.ValidateRange(Properties.Select(otherTenant => otherTenant.RentedFlats));
        }
    }
}