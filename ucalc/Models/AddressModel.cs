using UCalc.Data;

namespace UCalc.Models
{
    public class AddressModel : Model
    {
        private readonly Address _data;
        public ModelProperty<string> Street { get; }
        public ModelProperty<string> HouseNumber { get; }
        public ModelProperty<string> City { get; }
        public ModelProperty<string> Postcode { get; }

        public AddressModel(Address data)
        {
            _data = data;

            Street = Add(new ModelProperty<string>("Straße", _data.Street, ModelPropertyValidators.IsNotEmpty));
            HouseNumber =
                Add(new ModelProperty<string>("Hausnummer", _data.HouseNumber, ModelPropertyValidators.IsNotEmpty));
            City = Add(new ModelProperty<string>("Stadt", _data.City, ModelPropertyValidators.IsNotEmpty));
            Postcode = Add(new ModelProperty<string>("PLZ", _data.Postcode, ModelPropertyValidators.IsNotEmpty));
        }

        public override void Apply()
        {
            _data.Street = Street.Value;
            Street.ResetModified();

            _data.HouseNumber = HouseNumber.Value;
            HouseNumber.ResetModified();

            _data.City = City.Value;
            City.ResetModified();

            _data.Postcode = Postcode.Value;
            Postcode.ResetModified();
        }
    }
}