﻿using System.Collections.Generic;
using System.Linq;
using UCalc.Data;

namespace UCalc.Models
{
    public class CostEntriesProperty : MultiProperty<CostEntryProperty>
    {
        public CostEntriesProperty(Model model, Property parent, IEnumerable<CostEntry> data) : base(model, parent,
            "Zeiträume: Geben Sie einen oder mehr Zeiträume an.")
        {
            foreach (var entry in data)
            {
                Add(new CostEntryProperty(model, this, entry));
            }
        }

        public void Add()
        {
            var entry = new CostEntryProperty(Model, this, new CostEntry());
            entry.StartDate.Value = null;
            entry.EndDate.Value = null;
            base.Add(entry);
        }

        public new void Remove(CostEntryProperty entry)
        {
            using var validator = Model.BeginValidation();

            base.Remove(entry);

            validator.ValidateRange(Properties.Select(otherEntry => otherEntry.EndDate));
        }
    }
}