using System;
using System.Collections;
using System.Collections.Generic;
using UCalc.Data;

namespace UCalc.Models
{
    public class AffectsAllProperty : AlwaysValidProperty<bool>
    {
        public AffectsAllProperty(Model model, Property parent, string name, bool value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            var affectedFlats = ((CostProperty) Parent).AffectedFlats;

            using var validator = Model.BeginValidation();
            validator.Validate(affectedFlats);

            return "";
        }
    }

    public class AffectedFlatsProperty : Property, IReadOnlyCollection<FlatProperty>
    {
        private const string NoFlatsError = "Betroffene Wohnungen: Es wurde keine Wohnung zugewiesen.";
        private readonly HashSet<FlatProperty> _flats;
        private readonly List<string> _errors;

        public AffectedFlatsProperty(Model model, Property parent, IEnumerable<Flat> data,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent)
        {
            _flats = new HashSet<FlatProperty>();
            _errors = new List<string>();

            foreach (var flat in data)
            {
                _flats.Add(flatToProperty?[flat] ?? throw new InvalidOperationException());
            }

            Validate();
        }

        public override IReadOnlyList<string> Errors => _errors;

        public void Add(FlatProperty flat)
        {
            if (!_flats.Add(flat))
            {
                return;
            }

            using var validator = Model.BeginValidation();
            validator.Validate(this);
            Modified = true;
        }

        public void Remove(FlatProperty flat)
        {
            if (!_flats.Remove(flat))
            {
                return;
            }

            using var validator = Model.BeginValidation();
            validator.Validate(this);
            Modified = true;
        }

        public bool Contains(FlatProperty flat)
        {
            return _flats.Contains(flat);
        }

        public sealed override void Validate()
        {
            _errors.Clear();

            if (_flats.Count == 0 && !((CostProperty) Parent).AffectsAll.Value)
            {
                _errors.Add(NoFlatsError);
            }

            using var validator = Model.BeginValidation();
            validator.Notify(this, "Errors");

            ((CostsProperty) Parent.Parent).NotifyChanged((CostProperty) Parent);
        }

        public IEnumerator<FlatProperty> GetEnumerator()
        {
            return _flats.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _flats.Count;
    }

    public class CostProperty : NestedProperty
    {
        public NotEmptyStringProperty Name { get; }
        public AlwaysValidProperty<int> Division { get; }
        public AffectsAllProperty AffectsAll { get; }
        public AlwaysValidProperty<bool> ShiftUnrented { get; }
        public AffectedFlatsProperty AffectedFlats { get; }
        public CostEntriesProperty Entries { get; }
        public AlwaysValidProperty<bool> DisplayInBill { get; }

        public CostProperty(Model model, Property parent, Cost cost,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent)
        {
            Name = Add(new NotEmptyStringProperty(model, this, "Name", cost.Name));
            Division = Add(new AlwaysValidProperty<int>(model, this, "Aufteilung", (int) cost.Division));
            AffectsAll = Add(new AffectsAllProperty(model, this, "Betrifft alle", cost.AffectsAll));
            AffectedFlats = Add(new AffectedFlatsProperty(model, this, cost.AffectedFlats, flatToProperty));
            ShiftUnrented =
                Add(new AlwaysValidProperty<bool>(model, this, "Unvermietete einbeziehen", cost.ShiftUnrented));
            Entries = Add(new CostEntriesProperty(model, this, cost.Entries));
            DisplayInBill = Add(new AlwaysValidProperty<bool>(model, this, "In Rechnung anzeigen", cost.DisplayInBill));
        }
    }
}