using UCalc.Data;

namespace UCalc.Models
{
    public class SalutationProperty : AlwaysValidProperty<int>
    {
        public SalutationProperty(Model model, Property parent, string name, Salutation value) : base(model, parent,
            name, (int) value)
        {
        }
    }
}