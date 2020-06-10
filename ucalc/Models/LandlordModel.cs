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
        public NestedModelProperty<AddressModel> Address { get; }
        public NestedModelProperty<BankAccountModel> BankAccount { get; }

        public LandlordModel(Landlord data)
        {
            _data = data;

            Salutation = Add(new ModelProperty<int>("Anrede", (int) _data.Salutation, null));
            Name = Add(new ModelProperty<string>("Name", _data.Name, ModelPropertyValidators.IsNotEmpty));
            MailAddress =
                Add(new ModelProperty<string>("Email Adresse", _data.MailAddress, ModelPropertyValidators.IsNotEmpty));
            Phone = Add(new ModelProperty<string>("Telefonnummer", _data.Phone, ModelPropertyValidators.IsNotEmpty));
            Address = Add(new NestedModelProperty<AddressModel>(new AddressModel(_data.Address)));
            BankAccount = Add(new NestedModelProperty<BankAccountModel>(new BankAccountModel(_data.BankAccount)));
        }

        public override void Apply()
        {
            _data.Salutation = (Salutation) Salutation.Value;
            Salutation.ResetModified();

            _data.Name = Name.Value;
            Name.ResetModified();

            _data.MailAddress = MailAddress.Value;
            MailAddress.ResetModified();

            _data.Phone = Phone.Value;
            Phone.ResetModified();

            Address.Model.Apply();

            BankAccount.Model.Apply();
        }
    }
}