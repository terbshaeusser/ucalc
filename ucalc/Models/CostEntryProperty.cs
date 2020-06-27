using System;
using System.Collections.Generic;
using System.Linq;
using UCalc.Controls;
using UCalc.Data;

namespace UCalc.Models
{
    public class DateProperty : ValueProperty<DateTime?>
    {
        public DateProperty(Model model, Property parent, string name, DateTime value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            var error = Value == null ? $"{Name}: Geben Sie einen Wert ein." : "";

            using var validator = Model.BeginValidation();
            validator.Validate(((CostEntryProperty) Parent).EndDate);

            return error;
        }
    }

    public class EndDateProperty : ValueProperty<DateTime?>
    {
        public EndDateProperty(Model model, Property parent, string name, DateTime value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            try
            {
                if (Value == null)
                {
                    return $"{Name}: Geben Sie einen Wert ein.";
                }

                var startDate = ((CostEntryProperty) Parent).StartDate.Value;
                if (startDate != null)
                {
                    if (startDate > Value)
                    {
                        return $"{Name}: Das Enddatum liegt vor dem Startdatum.";
                    }

                    foreach (var entry in ((CostProperty) Parent.Parent.Parent).Entries)
                    {
                        if (!ReferenceEquals(entry.EndDate, this) && entry.StartDate.Value != null &&
                            entry.EndDate.Value != null &&
                            startDate.Value.Intersects(Value.Value, entry.StartDate.Value.Value,
                                entry.EndDate.Value.Value))
                        {
                            return $"{Name}: Dieser Zeitraum überschneidet sich mit einem anderen.";
                        }
                    }

                    var billing = (BillingProperty) Parent.Parent.Parent.Parent.Parent;
                    if (!Value.Value.IsBetween(billing.StartDate, billing.EndDate) &&
                        !startDate.Value.IsBetween(billing.StartDate, billing.EndDate))
                    {
                        return $"{Name}: Dieser Zeitraum befindet sich außerhalb des Abrechnungszeitraums.";
                    }
                }

                return "";
            }
            finally
            {
                using var validator = Model.BeginValidation();
                validator.ValidateRange(((CostProperty) Parent.Parent.Parent).Entries.Select(entry => entry.EndDate));
            }
        }
    }

    public class TotalAmountProperty : PositiveDecimalProperty
    {
        public TotalAmountProperty(Model model, Property parent, string name, decimal value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            var entry = (CostEntryDetailsProperty) Parent;

            try
            {
                var error = base.ValidateValue();
                if (error != "")
                {
                    return error;
                }

                if (((entry.UnitCount.ConvertedValue ?? 1) != 0 || entry.DiscountsInUnits.Count > 0) &&
                    ConvertedValue == 0)
                {
                    return $"{Name}: Der Wert muss größer als 0 sein.";
                }

                return "";
            }
            finally
            {
                using var validator = Model.BeginValidation();
                validator.Validate(entry.UnitCount);
            }
        }
    }

    public class UnitCountProperty : PositiveMoreExactDecimalProperty
    {
        public UnitCountProperty(Model model, Property parent, string name, decimal value) : base(model, parent, name,
            value, Constants.InternalPrecision)
        {
        }

        protected override string ValidateValue()
        {
            var entry = (CostEntryDetailsProperty) Parent;

            try
            {
                var error = base.ValidateValue();
                if (error != "")
                {
                    return error;
                }

                if (((entry.TotalAmount.ConvertedValue ?? 1) != 0 || entry.DiscountsInUnits.Count > 0) &&
                    ConvertedValue == 0)
                {
                    return $"{Name}: Der Wert muss größer als 0 sein.";
                }

                return "";
            }
            finally
            {
                using var validator = Model.BeginValidation();
                validator.Validate(entry.TotalAmount);
            }
        }
    }

    public class DiscountProperty : PositiveDecimalProperty
    {
        public DiscountProperty(Model model, Property parent, string name, decimal value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            var error = base.ValidateValue();


            return error;
        }
    }

    public class DiscountsProperty : MultiProperty<DiscountProperty>
    {
        public DiscountsProperty(Model model, Property parent, IEnumerable<decimal> data) : base(model, parent)
        {
            using var validator = Model.BeginValidation();

            foreach (var discount in data)
            {
                Add(new DiscountProperty(Model, this, "Abzug", discount));
            }

            Modified = false;
        }

        public void Add()
        {
            using var validator = Model.BeginValidation();

            Add(new DiscountProperty(Model, this, "Abzug", 0));

            validator.Validate(((CostEntryDetailsProperty) Parent).UnitCount);
        }

        public new void Remove(DiscountProperty discount)
        {
            using var validator = Model.BeginValidation();

            base.Remove(discount);

            validator.Validate(((CostEntryDetailsProperty) Parent).UnitCount);
        }
    }

    public class CostEntryDetailsProperty : NestedProperty
    {
        public TotalAmountProperty TotalAmount { get; }
        public UnitCountProperty UnitCount { get; }
        public DiscountsProperty DiscountsInUnits { get; }

        public CostEntryDetailsProperty(Model model, Property parent, CostEntryDetails data) : base(model, parent)
        {
            TotalAmount = Add(new TotalAmountProperty(model, this, "Gesamtbetrag", data.TotalAmount));
            UnitCount = Add(new UnitCountProperty(model, this, "Gesamtverbrauch", data.UnitCount));
            DiscountsInUnits = Add(new DiscountsProperty(model, this, data.DiscountsInUnits));
        }
    }

    public class CostEntryProperty : NestedProperty
    {
        public DateProperty StartDate { get; }
        public EndDateProperty EndDate { get; }
        public PositiveDecimalProperty Amount { get; }
        public CostEntryDetailsProperty Details { get; }

        public CostEntryProperty(Model model, Property parent, CostEntry data) : base(model, parent)
        {
            StartDate = Add(new DateProperty(model, this, "Startdatum", data.StartDate));
            EndDate = Add(new EndDateProperty(model, this, "Enddatum", data.EndDate));
            Amount = Add(new PositiveDecimalProperty(model, this, "Betrag", data.Amount));
            Details = Add(new CostEntryDetailsProperty(model, this, data.Details));
        }
    }
}