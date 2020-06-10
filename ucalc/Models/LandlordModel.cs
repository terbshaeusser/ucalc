using UCalc.Data;

namespace UCalc.Models
{
    public class LandlordModel : Model
    {
        private readonly Landlord _data;
        public ModelProperty<int> Salutation { get; }
        public ModelProperty<string> Name { get; }
        public ModelProperty<string> MailAddress { get; }
        public ModelProperty<string> Phone { get; }
        public ModelProperty<string> Street { get; }
        public ModelProperty<string> HouseNumber { get; }
        public ModelProperty<string> City { get; }
        public ModelProperty<string> Postcode { get; }
        public ModelProperty<string> Iban { get; }
        public ModelProperty<string> Bic { get; }
        public ModelProperty<string> BankName { get; }

        public LandlordModel(Landlord data)
        {
            _data = data;

            Salutation = new ModelProperty<int>(this, (int) _data.Salutation, null);
            Name = new ModelProperty<string>(this, _data.Name, ModelPropertyValidators.IsNotEmpty);
            MailAddress = new ModelProperty<string>(this, _data.MailAddress, ModelPropertyValidators.IsNotEmpty);
            Phone = new ModelProperty<string>(this, _data.Phone, ModelPropertyValidators.IsNotEmpty);
            Street = new ModelProperty<string>(this, _data.Address.Street, ModelPropertyValidators.IsNotEmpty);
            HouseNumber =
                new ModelProperty<string>(this, _data.Address.HouseNumber, ModelPropertyValidators.IsNotEmpty);
            City = new ModelProperty<string>(this, _data.Address.City, ModelPropertyValidators.IsNotEmpty);
            Postcode = new ModelProperty<string>(this, _data.Address.Postcode, ModelPropertyValidators.IsNotEmpty);
            Iban = new ModelProperty<string>(this, _data.BankAccount.Iban, ModelPropertyValidators.IsNotEmpty);
            Bic = new ModelProperty<string>(this, _data.BankAccount.Bic, ModelPropertyValidators.IsNotEmpty);
            BankName = new ModelProperty<string>(this, _data.BankAccount.BankName, ModelPropertyValidators.IsNotEmpty);

            Properties = new ModelProperty[]
                {Salutation, Name, MailAddress, Phone, Street, HouseNumber, City, Postcode, Iban, Bic, BankName};
        }

        public override void Apply()
        {
            base.Apply();

            _data.Salutation = (Salutation) Salutation.Value;
            Salutation.Modified = false;

            _data.Name = Name.Value;
            Name.Modified = false;

            _data.MailAddress = MailAddress.Value;
            MailAddress.Modified = false;

            _data.Phone = Phone.Value;
            Phone.Modified = false;

            _data.Address.Street = Street.Value;
            Street.Modified = false;

            _data.Address.HouseNumber = HouseNumber.Value;
            HouseNumber.Modified = false;

            _data.Address.City = City.Value;
            City.Modified = false;

            _data.Address.Postcode = Postcode.Value;
            Postcode.Modified = false;

            _data.BankAccount.Iban = Iban.Value;
            Iban.Modified = false;

            _data.BankAccount.Bic = Bic.Value;
            Bic.Modified = false;

            _data.BankAccount.BankName = BankName.Value;
            BankName.Modified = false;
        }
    }
}