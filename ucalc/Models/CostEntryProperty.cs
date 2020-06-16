using System;
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

    public class CostEntryProperty : NestedProperty
    {
        public DateProperty StartDate { get; }
        public EndDateProperty EndDate { get; }
        public PositiveDecimalProperty Amount { get; }

        // TODO: Details

        public CostEntryProperty(Model model, Property parent, CostEntry data) : base(model, parent)
        {
            StartDate = Add(new DateProperty(model, this, "Startdatum", data.StartDate));
            EndDate = Add(new EndDateProperty(model, this, "Enddatum", data.EndDate));
            Amount = Add(new PositiveDecimalProperty(model, this, "Betrag", data.Amount));
        }
    }
}