using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UCalc.Annotations;
using UCalc.Data;

namespace UCalc.Models
{
    public class Model : INotifyPropertyChanged
    {
        public class Validator : IDisposable
        {
            private readonly Model _model;
            private int _counter;
            private readonly HashSet<Property> _validated;
            private readonly HashSet<Tuple<Property, string>> _notifications;

            public Validator(Model model)
            {
                _model = model;
                _validated = new HashSet<Property>();
                _notifications = new HashSet<Tuple<Property, string>>();
            }

            public void Notify(Property property, string propertyName)
            {
                _notifications.Add(new Tuple<Property, string>(property, propertyName));

                if (property != null)
                {
                    property = property.Parent;

                    while (property != null)
                    {
                        _notifications.Add(new Tuple<Property, string>(property, propertyName));
                        property = property.Parent;
                    }

                    _notifications.Add(new Tuple<Property, string>(null, propertyName));
                }
            }

            public void Validate(Property property)
            {
                if (property == null)
                {
                    return;
                }

                if (_validated.Add(property) && !_model._loading)
                {
                    property.Validate();
                }
            }

            public void ValidateRange(IEnumerable<Property> properties)
            {
                foreach (var property in properties)
                {
                    Validate(property);
                }
            }

            public void IncValidationCounter()
            {
                ++_counter;
            }

            public void Dispose()
            {
                --_counter;

                if (_counter == 0)
                {
                    _validated.Clear();

                    var sortedNotifications = new List<Tuple<Property, string>>(_notifications);
                    sortedNotifications.Sort((x, y) =>
                    {
                        var (prop1, propName1) = x;
                        var (prop2, propName2) = y;

                        if (prop1 == null)
                        {
                            return prop2 == null ? string.CompareOrdinal(propName1, propName2) : 1;
                        }

                        if (prop2 == null)
                        {
                            return -1;
                        }

                        if (ReferenceEquals(prop1, prop2))
                        {
                            return string.CompareOrdinal(propName1, propName2);
                        }

                        if (prop1.IsParentOf(prop2))
                        {
                            return 1;
                        }

                        if (prop2.IsParentOf(prop1))
                        {
                            return -1;
                        }

                        var d1 = prop1.DepthInTree();
                        var d2 = prop2.DepthInTree();
                        if (d1 < d2)
                        {
                            return 1;
                        }

                        if (d1 > d2)
                        {
                            return -1;
                        }

                        return 0;
                    });

                    foreach (var (property, propertyName) in sortedNotifications)
                    {
                        if (property == null)
                        {
                            _model.OnPropertyChanged(propertyName);
                        }
                        else
                        {
                            property.OnPropertyChanged(propertyName);
                        }
                    }

                    _notifications.Clear();
                }
            }
        }

        private readonly Validator _validator;
        private readonly bool _loading;
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public BillingProperty Root { get; }

        public Model(Billing billing)
        {
            _validator = new Validator(this);
            StartDate = billing.StartDate;
            EndDate = billing.EndDate;

            _loading = true;
            Root = new BillingProperty(this, null, billing);
            _loading = false;

            using var validator = BeginValidation();
            validator.Validate(Root);
        }

        public Validator BeginValidation()
        {
            _validator.IncValidationCounter();
            return _validator;
        }

        public Billing Dump()
        {
            var billing = new Billing
            {
                StartDate = StartDate,
                EndDate = EndDate,
                Landlord =
                {
                    Salutation = (Salutation) Root.Landlord.Salutation.Value,
                    Name = Root.Landlord.Name.Value,
                    MailAddress = Root.Landlord.MailAddress.Value,
                    Phone = Root.Landlord.Phone.Value,
                    Address =
                    {
                        Street = Root.Landlord.Address.Street.Value,
                        HouseNumber = Root.Landlord.Address.HouseNumber.Value,
                        City = Root.Landlord.Address.City.Value,
                        Postcode = Root.Landlord.Address.Postcode.Value
                    },
                    BankAccount =
                    {
                        Iban = Root.Landlord.BankAccount.Iban.Value,
                        Bic = Root.Landlord.BankAccount.Bic.Value,
                        BankName = Root.Landlord.BankAccount.BankName.Value
                    }
                },
                House =
                {
                    Address =
                    {
                        Street = Root.House.Address.Street.Value,
                        HouseNumber = Root.House.Address.HouseNumber.Value,
                        City = Root.House.Address.City.Value,
                        Postcode = Root.House.Address.Postcode.Value
                    },
                    Flats = Root.House.Flats.Select(flat => new Flat
                        {Name = flat.Name.Value, Size = int.TryParse(flat.Size.Value, out var n) ? n : 0}).ToList()
                }
            };

            var flatPropertyToFlat = new Dictionary<FlatProperty, Flat>();
            for (var i = 0; i < Root.House.Flats.Count; ++i)
            {
                flatPropertyToFlat.Add(Root.House.Flats[i], billing.House.Flats[i]);
            }

            billing.Tenants = Root.Tenants.Select(tenant => new Tenant
            {
                Salutation = (Salutation) tenant.Salutation.Value,
                Name = tenant.Name.Value,
                PersonCount = int.TryParse(tenant.PersonCount.Value, out var n) ? n : 0,
                BankAccount =
                {
                    Iban = tenant.BankAccount.Iban.Value,
                    Bic = tenant.BankAccount.Bic.Value,
                    BankName = tenant.BankAccount.BankName.Value
                },
                EntryDate = tenant.EntryDate.Value,
                DepartureDate = tenant.DepartureDate.Value,
                RentedFlats =
                    new HashSet<Flat>(tenant.RentedFlats.Select(rentedFlat => flatPropertyToFlat[rentedFlat])),
                PaidRent = decimal.TryParse(tenant.PaidRent.Value, out var d) ? d : 0,
                CustomMessage1 = tenant.CustomMessage1.Value,
                CustomMessage2 = tenant.CustomMessage2.Value
            }).ToList();

            billing.Costs = Root.Costs.Select(cost => new Cost
            {
                Name = cost.Name.Value,
                Division = (CostDivision) cost.Division.Value,
                AffectsAll = cost.AffectsAll.Value,
                IncludeUnrented = cost.IncludeUnrented.Value,
                AffectedFlats =
                    new HashSet<Flat>(cost.AffectedFlats.Select(rentedFlat => flatPropertyToFlat[rentedFlat])),
                // TODO: Entries = {},
                DisplayInBill = cost.DisplayInBill.Value
            }).ToList();

            return billing;
        }

        public void ResetModified()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}