using UCalc.Data;

namespace UCalc.Models
{
    public class HouseModel : Model
    {
        private readonly House _data;
        public NestedModelProperty<AddressModel> Address { get; }
        public MultiModelProperty<FlatModel> Flats { get; }

        public HouseModel(House data)
        {
            _data = data;

            Address = Add(new NestedModelProperty<AddressModel>(new AddressModel(_data.Address)));
            Flats = Add(new MultiModelProperty<FlatModel>());

            foreach (var flat in data.Flats)
            {
                Flats.Add(new FlatModel(flat, _data.Flats));
            }
        }

        public override void Apply()
        {
            Address.Model.Apply();

            foreach (var model in Flats.Models)
            {
                model.Apply();
            }
        }

        public FlatModel AddFlat()
        {
            var flat = new Flat {Name = $"Wohnung {_data.Flats.Count + 1}"};
            _data.Flats.Add(flat);

            var model = new FlatModel(flat, _data.Flats);
            Flats.Add(model);

            return model;
        }

        public void RemoveFlat(FlatModel model)
        {
            _data.Flats.Remove(model.Data);

            Flats.Remove(model);

            if (model.Errors.Count > 0)
            {
                OnPropertyChanged("Errors");
            }
        }
    }
}