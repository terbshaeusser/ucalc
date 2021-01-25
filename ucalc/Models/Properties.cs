using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UCalc.Annotations;
using UCalc.Controls;

namespace UCalc.Models
{
    public abstract class Property : INotifyPropertyChanged
    {
        protected static readonly IReadOnlyList<string> EmptyList = new List<string>();

        protected readonly Model Model;
        public Property Parent { get; }

        public abstract IReadOnlyList<string> Errors { get; }

        private bool _modified;

        public bool Modified
        {
            get => _modified;
            protected set
            {
                if (_modified == value)
                {
                    return;
                }

                _modified = value;

                using var validator = Model.BeginValidation();
                validator.Notify(this, "Modified");
            }
        }

        protected Property(Model model, Property parent)
        {
            Model = model;
            Parent = parent;
        }

        public abstract void Validate();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsParentOf(Property property)
        {
            while (property != null)
            {
                if (ReferenceEquals(this, property))
                {
                    return true;
                }

                property = property.Parent;
            }

            return false;
        }

        public int DepthInTree()
        {
            var i = 0;
            var property = Parent;

            while (property != null)
            {
                ++i;
                property = property.Parent;
            }

            return i;
        }

        public virtual void ResetModified()
        {
            Modified = false;
        }
    }

    public abstract class ValueProperty<T> : Property
    {
        protected readonly string Name;
        private T _value;
        private readonly List<string> _errors;

        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value))
                {
                    return;
                }

                _value = value;
                OnPropertyChanged();

                using var validator = Model.BeginValidation();
                Modified = true;
                validator.Validate(this);
            }
        }

        protected ValueProperty(Model model, Property parent, string name, T value) : base(model, parent)
        {
            Name = name;
            _value = value;
            _errors = new List<string> {""};

            using var validator = Model.BeginValidation();
            validator.Validate(this);
        }

        public override IReadOnlyList<string> Errors => _errors[0] != "" ? _errors : EmptyList;

        public sealed override void Validate()
        {
            var oldError = _errors[0];
            _errors[0] = ValidateValue();

            if (oldError != _errors[0])
            {
                using var validator = Model.BeginValidation();
                validator.Notify(this, "Errors");
            }
        }

        protected abstract string ValidateValue();
    }

    public class AlwaysValidProperty<T> : ValueProperty<T>
    {
        public AlwaysValidProperty(Model model, Property parent, string name, T value) : base(model, parent,
            name, value)
        {
        }

        protected override string ValidateValue()
        {
            return "";
        }
    }

    public class NotEmptyStringProperty : ValueProperty<string>
    {
        public NotEmptyStringProperty(Model model, Property parent, string name, string value) : base(model, parent,
            name, value)
        {
        }

        protected override string ValidateValue()
        {
            return string.IsNullOrEmpty(Value) ? $"{Name}: Geben Sie einen Wert ein." : "";
        }
    }

    public class NaturalNumberProperty : ValueProperty<string>
    {
        public NaturalNumberProperty(Model model, Property parent, string name, int value) : base(model, parent, name,
            value.ToString())
        {
        }

        protected override string ValidateValue()
        {
            return !int.TryParse(Value, out var n) || n <= 0
                ? $"{Name}: Der eingegebene Wert ist keine gültige Zahl > 0."
                : "";
        }
    }

    public class PositiveMoreExactDecimalProperty : ValueProperty<string>
    {
        private readonly int _decimals;
        public decimal? ConvertedValue { get; private set; }

        public PositiveMoreExactDecimalProperty(Model model, Property parent, string name, decimal value, int decimals)
            : base(model, parent, name, value.ToString(Helpers.PrecisionToFormat(decimals, decimals - 2)))
        {
            _decimals = decimals;
        }

        protected override string ValidateValue()
        {
            ConvertedValue = null;

            if (!decimal.TryParse(Value, out var n))
            {
                return $"{Name}: Der eingegebene Wert ist ungültig.";
            }

            if (n < 0)
            {
                return $"{Name}: Der eingegebene Wert darf nicht negativ sein.";
            }

            if (Math.Round(n, _decimals) != n)
            {
                return $"{Name}: Der eingegebene Wert besitzt mehr als {_decimals} Nachkommastellen.";
            }

            ConvertedValue = n;
            return "";
        }
    }

    public class PositiveDecimalProperty : PositiveMoreExactDecimalProperty
    {
        public PositiveDecimalProperty(Model model, Property parent, string name, decimal value) : base(model, parent,
            name, value, Constants.DisplayPrecision)
        {
        }
    }

    public abstract class NestedProperty : Property
    {
        private readonly List<Property> _properties;
        private readonly List<string> _errors;
        public override IReadOnlyList<string> Errors => _errors;

        protected NestedProperty(Model model, Property parent) : base(model, parent)
        {
            _properties = new List<Property>();
            _errors = new List<string>();
        }

        protected T Add<T>(T property) where T : Property
        {
            _properties.Add(property);
            return property;
        }

        public override void Validate()
        {
            using var validator = Model.BeginValidation();
            validator.ValidateRange(_properties);
        }

        public override void OnPropertyChanged(string propertyName = null)
        {
            switch (propertyName)
            {
                case "Errors":
                    _errors.Clear();

                    foreach (var property in _properties)
                    {
                        _errors.AddRange(property.Errors);
                    }

                    break;
                case "Modified":
                    Modified = _properties.Any(property => property.Modified);
                    break;
            }

            base.OnPropertyChanged(propertyName);
        }

        public override void ResetModified()
        {
            foreach (var property in _properties)
            {
                property.ResetModified();
            }
        }
    }

    public abstract class MultiProperty<T> : Property, IReadOnlyList<T>, INotifyCollectionChanged where T : Property
    {
        private readonly string _errorOnEmpty;
        private readonly List<T> _properties;
        protected IReadOnlyList<T> Properties => _properties;
        private readonly List<string> _errors;
        public override IReadOnlyList<string> Errors => _errors;

        protected MultiProperty(Model model, Property parent, string errorOnEmpty = null) : base(model, parent)
        {
            _errorOnEmpty = errorOnEmpty;
            _properties = new List<T>();
            _errors = new List<string>();

            OnPropertyChanged("Errors");
        }

        protected void Add(T property)
        {
            _properties.Add(property);

            using var validator = Model.BeginValidation();
            validator.Notify(this, "Errors");
            Modified = true;

            OnPropertyChanged("Count");

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, property,
                    _properties.Count - 1));
        }

        protected void Remove(T property)
        {
            var index = _properties.IndexOf(property);
            _properties.RemoveAt(index);

            using var validator = Model.BeginValidation();
            validator.Notify(this, "Errors");
            Modified = true;

            OnPropertyChanged("Count");

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, property, index));
        }

        public override void Validate()
        {
            using var validator = Model.BeginValidation();
            validator.ValidateRange(_properties);
        }

        public void NotifyChanged(T property)
        {
            var index = _properties.IndexOf(property);
            if (index == -1)
            {
                return;
            }

            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public sealed override void OnPropertyChanged(string propertyName = null)
        {
            switch (propertyName)
            {
                case "Errors":
                    _errors.Clear();

                    if (_properties.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(_errorOnEmpty))
                        {
                            _errors.Add(_errorOnEmpty);
                        }
                    }
                    else
                    {
                        foreach (var property in _properties)
                        {
                            _errors.AddRange(property.Errors);
                        }
                    }

                    break;
                case "Modified":
                    Modified = Modified || _properties.Any(property => property.Modified);
                    break;
            }

            base.OnPropertyChanged(propertyName);
        }

        public override void ResetModified()
        {
            foreach (var property in _properties)
            {
                property.ResetModified();
            }

            base.ResetModified();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _properties.Count;

        public T this[int index] => _properties[index];

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}