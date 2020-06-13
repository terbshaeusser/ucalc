using System.Linq;
using UCalc.Data;

namespace UCalc.Models
{
    public class FlatNameProperty : ValueProperty<string>
    {
        public FlatNameProperty(Model model, Property parent, string name, string value) : base(model, parent, name,
            value)
        {
        }

        protected override string ValidateValue()
        {
            var error = "";
            using var validator = Model.BeginValidation();

            if (Model.Root.House.Flats.Any(flat => !ReferenceEquals(this, flat.Name) && flat.Name.Value == Value))
            {
                error = $"{Name}: Der Name ist nicht eindeutig.";
            }

            validator.ValidateRange(Model.Root.House.Flats.Select(flat => flat.Name));
            return error;
        }
    }

    public class FlatProperty : NestedProperty
    {
        public FlatNameProperty Name { get; }
        public NaturalNumberProperty Size { get; }

        public FlatProperty(Model model, Property parent, Flat data) : base(model, parent)
        {
            Name = Add(new FlatNameProperty(model, this, "Name", data.Name));
            Size = Add(new NaturalNumberProperty(model, this, "Größe", data.Size));
        }
    }
}