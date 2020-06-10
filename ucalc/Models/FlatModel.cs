using System.Collections.Generic;
using System.Linq;
using UCalc.Data;

namespace UCalc.Models
{
    public class FlatModel : Model
    {
        public Flat Data { get; }
        public ModelProperty<string> Name { get; }
        public ModelProperty<string> Size { get; }

        public FlatModel(Flat data, IReadOnlyList<Flat> flats)
        {
            Data = data;

            Name = Add(new ModelProperty<string>("Name", Data.Name, (name, value) =>
            {
                var error = ModelPropertyValidators.IsNotEmpty(name, value);

                if (error == "" && flats.Any(flat => !ReferenceEquals(flat, Data) && flat.Name == value))
                {
                    error = $"{name}: Der Wert \"{value}\" ist nicht eindeutig.";
                }

                return error;
            }));
            Size = Add(new ModelProperty<string>("Größe", Data.Size.ToString(), ModelPropertyValidators.IsNaturalInt));
        }

        public override void Apply()
        {
            Data.Name = Name.Value;
            Name.ResetModified();

            Data.Size = int.TryParse(Size.Value, out var n) && n > 0 ? n : 0;
            Size.ResetModified();
        }
    }
}