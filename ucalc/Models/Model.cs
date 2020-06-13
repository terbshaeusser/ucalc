using System;
using System.Collections.Generic;
using System.ComponentModel;
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

                if (_validated.Add(property))
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

        public Model(Billing billing)
        {
            _validator = new Validator(this);

            using var validator = BeginValidation();
            Root = new BillingProperty(this, null, billing);
        }

        public Validator BeginValidation()
        {
            _validator.IncValidationCounter();
            return _validator;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}