using UCalc.Data;

namespace UCalc.Models
{
    public class HouseModel : Model
    {
        private readonly House _data;
        public ModelProperty<string> Street { get; }
        public ModelProperty<string> HouseNumber { get; }
        public ModelProperty<string> City { get; }
        public ModelProperty<string> Postcode { get; }

        // TODO: Flats?

        public HouseModel(House data)
        {
            _data = data;
        }
    }
}