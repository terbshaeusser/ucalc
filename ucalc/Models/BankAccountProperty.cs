using UCalc.Data;

namespace UCalc.Models
{
    public class BankAccountProperty : NestedProperty
    {
        public NotEmptyStringProperty Iban { get; }
        public NotEmptyStringProperty Bic { get; }
        public NotEmptyStringProperty BankName { get; }

        public BankAccountProperty(Model model, Property parent, BankAccount data) : base(model, parent)
        {
            Iban = Add(new NotEmptyStringProperty(model, this, "IBAN", data.Iban));
            Bic = Add(new NotEmptyStringProperty(model, this, "BIC", data.Bic));
            BankName = Add(new NotEmptyStringProperty(model, this, "Name der Bank", data.BankName));
        }
    }
}