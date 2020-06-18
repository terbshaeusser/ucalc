using System;
using System.Collections;
using System.Collections.Generic;
using UCalc.Controls;
using UCalc.Data;

namespace UCalc.Models
{
    public class EntryDateProperty : ValueProperty<DateTime?>
    {
        public EntryDateProperty(Model model, Property parent, string name, DateTime? value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            using var validator = Model.BeginValidation();

            validator.Validate(((TenantProperty) Parent).DepartureDate);

            return "";
        }
    }

    public class DepatureDateProperty : ValueProperty<DateTime?>
    {
        public DepatureDateProperty(Model model, Property parent, string name, DateTime? value) : base(model, parent,
            name, value)
        {
        }

        protected override string ValidateValue()
        {
            if (Value != null)
            {
                var entryDate = ((TenantProperty) Parent).EntryDate.Value;

                if (entryDate != null && entryDate > Value)
                {
                    return $"{Name}: Das Datum liegt vor dem Einzug.";
                }
            }

            using var validator = Model.BeginValidation();
            validator.Validate(((TenantProperty) Parent).RentedFlats);

            return "";
        }
    }

    public class RentedFlatsProperty : Property, IReadOnlyCollection<FlatProperty>
    {
        private const string NoFlatsError = "Gemietete Wohnungen: Es wurde keine Wohnung zugewiesen.";
        private readonly HashSet<FlatProperty> _flatProperties;
        private readonly List<string> _errors;

        public RentedFlatsProperty(Model model, Property parent, IEnumerable<Flat> data,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent)
        {
            _flatProperties = new HashSet<FlatProperty>();
            _errors = new List<string>();

            foreach (var flat in data)
            {
                _flatProperties.Add(flatToProperty?[flat] ?? throw new InvalidOperationException());
            }

            using var validator = Model.BeginValidation();
            validator.Validate(this);
        }

        public override IReadOnlyList<string> Errors => _errors;

        public void Add(FlatProperty flat)
        {
            if (!_flatProperties.Add(flat))
            {
                return;
            }

            using var validator = Model.BeginValidation();
            validator.Validate(this);
            Modified = true;
        }

        public void Remove(FlatProperty flat)
        {
            if (!_flatProperties.Remove(flat))
            {
                return;
            }

            using var validator = Model.BeginValidation();
            validator.Validate(this);
            Modified = true;
        }

        public bool Contains(FlatProperty flat)
        {
            return _flatProperties.Contains(flat);
        }

        public sealed override void Validate()
        {
            _errors.Clear();

            var tenants = (TenantsProperty) Parent.Parent;

            try
            {
                if (_flatProperties.Count == 0)
                {
                    _errors.Add(NoFlatsError);
                    return;
                }

                var entryDate = ((TenantProperty) Parent).EntryDate.Value;
                var departureDate = ((TenantProperty) Parent).DepartureDate.Value;

                foreach (var flatProperty in _flatProperties)
                {
                    foreach (var tenant in tenants)
                    {
                        if (ReferenceEquals(this, tenant.RentedFlats))
                        {
                            continue;
                        }

                        if (tenant.RentedFlats.IsRented(flatProperty, entryDate, departureDate))
                        {
                            var error = $"Gemietete Wohnungen: Die Wohnung \"{flatProperty.Name.Value}\" ist bereits ";

                            if (entryDate != null)
                            {
                                if (departureDate != null)
                                {
                                    _errors.Add(
                                        $"von {entryDate.Value.ToString(Constants.DateFormat)} - {departureDate.Value.ToString(Constants.DateFormat)} ");
                                }
                                else
                                {
                                    _errors.Add($"seit dem {entryDate.Value.ToString(Constants.DateFormat)} ");
                                }
                            }
                            else if (departureDate != null)
                            {
                                _errors.Add($"bis zum {departureDate.Value.ToString(Constants.DateFormat)} ");
                            }

                            error += "an einen anderen Mieter vermietet.";
                            _errors.Add(error);
                            break;
                        }
                    }
                }
            }
            finally
            {
                using var validator = Model.BeginValidation();
                validator.Notify(this, "Errors");

                validator.ValidateRange(tenants);
            }
        }

        private bool IsRented(FlatProperty flat, DateTime? start, DateTime? end)
        {
            if (!_flatProperties.Contains(flat))
            {
                return false;
            }

            var thisStart = ((TenantProperty) Parent).EntryDate.Value ?? DateTime.MinValue;
            var thisEnd = ((TenantProperty) Parent).DepartureDate.Value ?? DateTime.MaxValue;

            var otherStart = start ?? DateTime.MinValue;
            var otherEnd = end ?? DateTime.MinValue;

            return thisStart.Intersects(thisEnd, otherStart, otherEnd);
        }

        public IEnumerator<FlatProperty> GetEnumerator()
        {
            return _flatProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _flatProperties.Count;
    }

    public class TenantProperty : NestedProperty
    {
        public SalutationProperty Salutation { get; }
        public NotEmptyStringProperty Name { get; }
        public NaturalNumberProperty PersonCount { get; }
        public BankAccountProperty BankAccount { get; }
        public EntryDateProperty EntryDate { get; }
        public DepatureDateProperty DepartureDate { get; }
        public RentedFlatsProperty RentedFlats { get; }
        public PositiveDecimalProperty PaidRent { get; }
        public AlwaysValidProperty<string> CustomMessage1 { get; }
        public AlwaysValidProperty<string> CustomMessage2 { get; }

        public TenantProperty(Model model, Property parent, Tenant data,
            IReadOnlyDictionary<Flat, FlatProperty> flatToProperty = null) : base(model, parent)
        {
            Salutation = Add(new SalutationProperty(model, this, "Anrede", data.Salutation));
            Name = Add(new NotEmptyStringProperty(model, this, "Name", data.Name));
            PersonCount = Add(new NaturalNumberProperty(model, this, "Personenanzahl", data.PersonCount));
            BankAccount = Add(new BankAccountProperty(model, this, data.BankAccount));
            EntryDate = Add(new EntryDateProperty(model, this, "Einzugsdatum", data.EntryDate));
            DepartureDate = Add(new DepatureDateProperty(model, this, "Auszugsdatum", data.DepartureDate));
            RentedFlats = Add(new RentedFlatsProperty(model, this, data.RentedFlats, flatToProperty));
            PaidRent = Add(new PositiveDecimalProperty(model, this, "Bereits gezahlt", data.PaidRent));
            CustomMessage1 = Add(new AlwaysValidProperty<string>(model, this, "Nachricht 1", data.CustomMessage1));
            CustomMessage2 = Add(new AlwaysValidProperty<string>(model, this, "Nachricht 2", data.CustomMessage2));
        }
    }
}