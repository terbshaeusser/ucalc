using System.Collections.Generic;
using System.Linq;
using UCalc.Data;

namespace UCalc.Models
{
    public class FlatsProperty : MultiProperty<FlatProperty>
    {
        public FlatsProperty(Model model, Property parent, IEnumerable<Flat> data) : base(model, parent,
            "Wohnungen: Geben Sie eine oder mehr Wohnungen an.")
        {
            using var validator = Model.BeginValidation();

            foreach (var flat in data)
            {
                base.Add(new FlatProperty(Model, this, flat));
            }

            Modified = false;
        }

        public FlatProperty Add()
        {
            using var validator = Model.BeginValidation(true);

            var flat = new FlatProperty(Model, this, new Flat {Name = $"Wohnung {Properties.Count + 1}"});
            base.Add(flat);

            validator.Validate(flat);

            return flat;
        }

        public new void Remove(FlatProperty flat)
        {
            using var validator = Model.BeginValidation();

            base.Remove(flat);

            // Revalidate flat names and flat lists in tenants and costs
            validator.ValidateRange(Properties.Select(otherFlat => otherFlat.Name));

            foreach (var tenant in Model.Root.Tenants)
            {
                tenant.RentedFlats.Remove(flat);
            }

            foreach (var cost in Model.Root.Costs)
            {
                cost.AffectedFlats.Remove(flat);
            }
        }
    }
}