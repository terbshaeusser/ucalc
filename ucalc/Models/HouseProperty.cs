using UCalc.Data;

namespace UCalc.Models
{
    public class HouseProperty : NestedProperty
    {
        public AddressProperty Address { get; }
        public FlatsProperty Flats { get; }

        public HouseProperty(Model model, Property parent, House data) : base(model, parent)
        {
            Address = Add(new AddressProperty(model, this, data.Address));
            Flats = Add(new FlatsProperty(model, this, data.Flats));
        }
    }
}