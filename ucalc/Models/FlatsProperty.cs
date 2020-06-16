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
            FlatProperty flat;
            {
                using var validator = Model.BeginValidation();

                flat = new FlatProperty(Model, this, new Flat {Name = $"Wohnung {Properties.Count + 1}"});
                base.Add(flat);
            }

            {
                using var validator = Model.BeginValidation();
                validator.Validate(flat);
            }

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

            // TODO: Revalidate all flats + revalidate tenant rent lists + costs flat lists
        }
    }
}