using UCalc.Data;

namespace UCalc.Models
{
    public class LandlordProperty : NestedProperty
    {
        public SalutationProperty Salutation { get; }
        public NotEmptyStringProperty Name { get; }
        public AlwaysValidProperty<string> MailAddress { get; }
        public NotEmptyStringProperty Phone { get; }
        public AddressProperty Address { get; }
        public BankAccountProperty BankAccount { get; }

        public LandlordProperty(Model model, Property parent, Landlord data) : base(model, parent)
        {
            Salutation = Add(new SalutationProperty(model, this, "Anrede", data.Salutation));
            Name = Add(new NotEmptyStringProperty(model, this, "Name", data.Name));
            MailAddress = Add(new AlwaysValidProperty<string>(model, this, "Email Adresse", data.MailAddress));
            Phone = Add(new NotEmptyStringProperty(model, this, "Telefonnummer", data.Phone));
            Address = Add(new AddressProperty(model, this, data.Address));
            BankAccount = Add(new BankAccountProperty(model, this, data.BankAccount, false));
        }
    }
}