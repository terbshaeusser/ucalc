using UCalc.Data;

namespace UCalc.Models
{
    public class BankAccountProperty : NestedProperty
    {
        public ValueProperty<string> Iban { get; }
        public ValueProperty<string> Bic { get; }
        public ValueProperty<string> BankName { get; }

        public BankAccountProperty(Model model, Property parent, BankAccount data, bool belongsToTenant) : base(model,
            parent)
        {
            if (belongsToTenant)
            {
                Iban = Add(new AlwaysValidProperty<string>(model, this, "IBAN", data.Iban));
                Bic = Add(new AlwaysValidProperty<string>(model, this, "BIC", data.Bic));
                BankName = Add(new AlwaysValidProperty<string>(model, this, "Name der Bank", data.BankName));
            }
            else
            {
                Iban = Add(new NotEmptyStringProperty(model, this, "IBAN", data.Iban));
                Bic = Add(new NotEmptyStringProperty(model, this, "BIC", data.Bic));
                BankName = Add(new NotEmptyStringProperty(model, this, "Name der Bank", data.BankName));
            }
        }
    }
}