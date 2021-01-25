using UCalc.Data;

namespace UCalc.Models
{
    public class AddressProperty : NestedProperty
    {
        public NotEmptyStringProperty Street { get; }
        public NotEmptyStringProperty HouseNumber { get; }
        public NotEmptyStringProperty City { get; }
        public NotEmptyStringProperty Postcode { get; }

        public AddressProperty(Model model, Property parent, Address data) : base(model, parent)
        {
            Street = Add(new NotEmptyStringProperty(model, this, "Straße", data.Street));
            HouseNumber = Add(new NotEmptyStringProperty(model, this, "Hausnummer", data.HouseNumber));
            City = Add(new NotEmptyStringProperty(model, this, "Stadt", data.City));
            Postcode = Add(new NotEmptyStringProperty(model, this, "PLZ", data.Postcode));
        }
    }
}