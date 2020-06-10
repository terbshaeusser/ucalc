using UCalc.Data;

namespace UCalc.Models
{
    public class BankAccountModel : Model
    {
        private readonly BankAccount _data;
        public ModelProperty<string> Iban { get; }
        public ModelProperty<string> Bic { get; }
        public ModelProperty<string> BankName { get; }

        public BankAccountModel(BankAccount data)
        {
            _data = data;

            Iban = Add(new ModelProperty<string>("IBAN", _data.Iban, ModelPropertyValidators.IsNotEmpty));
            Bic = Add(new ModelProperty<string>("BIC", _data.Bic, ModelPropertyValidators.IsNotEmpty));
            BankName = Add(new ModelProperty<string>("Name der Bank", _data.BankName,
                ModelPropertyValidators.IsNotEmpty));
        }

        public override void Apply()
        {
            _data.Iban = Iban.Value;
            Iban.ResetModified();

            _data.Bic = Bic.Value;
            Bic.ResetModified();

            _data.BankName = BankName.Value;
            BankName.ResetModified();
        }
    }
}