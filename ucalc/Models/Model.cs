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
            private int _postPoneCounter;
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

                if (_validated.Add(property) && _postPoneCounter == 0)
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

            public void IncValidationCounter(bool postPone)
            {
                ++_counter;

                if (postPone || _postPoneCounter != 0)
                {
                    ++_postPoneCounter;
                }
            }

            public void Dispose()
            {
                --_counter;

                if (_postPoneCounter > 0)
                {
                    --_postPoneCounter;

                    if (_postPoneCounter == 0)
                    {
                        var queue = new List<Property>(_validated);
                        _validated.Clear();

                        ValidateRange(queue);
                    }
                }

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
        public BillingProperty Root { get; }

        public Model(Billing billing, bool modified)
        {
            _validator = new Validator(this);

            using var validator = BeginValidation(true);

            Root = new BillingProperty(billing.StartDate, billing.EndDate, this, null, billing);
            if (modified)
            {
                var oldName = Root.Landlord.Name.Value;
                Root.Landlord.Name.Value = $"{oldName}_";
                Root.Landlord.Name.Value = oldName;
            }
        }

        public Validator BeginValidation(bool postPone = false)
        {
            _validator.IncValidationCounter(postPone);
            return _validator;
        }

        public Billing Dump()
        {
            var billing = new Billing
            {
                StartDate = Root.StartDate,
                EndDate = Root.EndDate,
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
                PaidRent = tenant.PaidRent.ConvertedValue ?? 0,
                CustomMessage1 = tenant.CustomMessage1.Value,
                CustomMessage2 = tenant.CustomMessage2.Value
            }).ToList();

            billing.Costs = Root.Costs.Select(cost => new Cost
            {
                Name = cost.Name.Value,
                Division = (CostDivision) cost.Division.Value,
                AffectsAll = cost.AffectsAll.Value,
                ShiftUnrented = cost.ShiftUnrented.Value,
                AffectedFlats =
                    new HashSet<Flat>(cost.AffectedFlats.Select(rentedFlat => flatPropertyToFlat[rentedFlat])),
                Entries = cost.Entries.Select(entry => new CostEntry
                {
                    StartDate = entry.StartDate.Value ?? Root.StartDate,
                    EndDate = entry.EndDate.Value ?? Root.EndDate,
                    Amount = entry.Amount.ConvertedValue ?? 0,
                    Details = new CostEntryDetails
                    {
                        TotalAmount = entry.Details.TotalAmount.ConvertedValue ?? 0,
                        UnitCount = entry.Details.UnitCount.ConvertedValue ?? 0,
                        DiscountsInUnits = entry.Details.DiscountsInUnits
                            .Select(discount => discount.ConvertedValue ?? 0).ToList()
                    }
                }).ToList(),
                DisplayInBill = cost.DisplayInBill.Value
            }).ToList();

            return billing;
        }

        public void ResetModified()
        {
            using var validator = BeginValidation();

            Root.ResetModified();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}