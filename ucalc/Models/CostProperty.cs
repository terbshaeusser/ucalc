using System.Collections.Generic;
using UCalc.Data;

namespace UCalc.Models
{
    public class CostProperty : NestedProperty
    {
        public NotEmptyStringProperty Name { get; }
        public AlwaysValidProperty<int> Division { get; }
        public AlwaysValidProperty<bool> AffectsAll { get; }

        public AlwaysValidProperty<bool> IncludeUnrented { get; }

        // TODO: public HashSet<Flat> AffectedFlats { get;  }
        // TODO: public List<CostEntry> Entries { get;  }
        public AlwaysValidProperty<bool> DisplayInBill { get; }

        public CostProperty(Model model, Property parent, Cost cost,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent)
        {
            Name = Add(new NotEmptyStringProperty(model, this, "Name", cost.Name));
            Division = Add(new AlwaysValidProperty<int>(model, this, "Aufteilung", (int) cost.Division));
            AffectsAll = Add(new AlwaysValidProperty<bool>(model, this, "Betrifft alle", cost.AffectsAll));
            IncludeUnrented =
                Add(new AlwaysValidProperty<bool>(model, this, "Unvermietete einbeziehen", cost.IncludeUnrented));
            DisplayInBill = Add(new AlwaysValidProperty<bool>(model, this, "In Rechnung anzeigen", cost.DisplayInBill));
        }
    }
}