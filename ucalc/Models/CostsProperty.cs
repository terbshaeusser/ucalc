using System.Collections.Generic;
using UCalc.Data;

namespace UCalc.Models
{
    public class CostsProperty : MultiProperty<CostProperty>
    {
        public CostsProperty(Model model, Property parent, IEnumerable<Cost> data,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent,
            "Kosten: Geben Sie einen oder mehr Kostenpunkte an.")
        {
            using var validator = Model.BeginValidation();

            foreach (var cost in data)
            {
                base.Add(new CostProperty(Model, this, cost, flatToProperty));
            }

            Modified = false;
        }

        public CostProperty Add()
        {
            using var validator = Model.BeginValidation();

            var cost = new CostProperty(Model, this, new Cost());
            base.Add(cost);
            return cost;
        }

        public new void Remove(CostProperty cost)
        {
            using var validator = Model.BeginValidation();

            base.Remove(cost);
        }
    }
}