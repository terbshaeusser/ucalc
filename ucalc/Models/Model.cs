using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UCalc.Annotations;

namespace UCalc.Models
{
    public abstract class ModelBaseProperty : INotifyPropertyChanged
    {
        protected static readonly List<string> EmptyList = new List<string>();
        public abstract IReadOnlyCollection<string> Errors { get; }
        public abstract bool Modified { get; }

        protected void OnChildPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Errors" || args.PropertyName == "Modified")
            {
                OnPropertyChanged(args.PropertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NestedModelProperty<T> : ModelBaseProperty where T : Model
    {
        public T Model { get; }

        public NestedModelProperty(T model)
        {
            Model = model;

            Model.PropertyChanged += OnChildPropertyChanged;
        }

        public override IReadOnlyCollection<string> Errors => Model.Errors;

        public override bool Modified => Model.Modified;
    }

    public class MultiModelProperty<T> : ModelBaseProperty where T : Model
    {
        private readonly ObservableCollection<T> _models;

        public MultiModelProperty()
        {
            _models = new ObservableCollection<T>();
        }

        public void Add(T model)
        {
            _models.Add(model);

            model.PropertyChanged += OnChildPropertyChanged;

            if (model.Errors.Count > 0)
            {
                OnPropertyChanged("Errors");
            }
        }

        public void Remove(T model)
        {
            model.PropertyChanged -= OnChildPropertyChanged;

            _models.Remove(model);

            if (model.Errors.Count > 0)
            {
                OnPropertyChanged("Errors");
            }
        }

        public IReadOnlyList<T> Models => _models;

        public override IReadOnlyCollection<string> Errors
        {
            get
            {
                var errors = new List<string>();

                foreach (var model in _models)
                {
                    errors.AddRange(model.Errors);
                }

                return errors;
            }
        }

        public override bool Modified => _models.Select(model => model.Modified).Any();
    }

    public class ModelProperty<T> : ModelBaseProperty
    {
        public string Name { get; }
        private T _value;
        private readonly List<string> _errors;
        private readonly Func<string, T, string> _validate;
        private bool _modified;

        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value))
                {
                    return;
                }

                var oldError = _errors[0];
                _errors[0] = _validate?.Invoke(Name, value) ?? "";

                if (oldError != _errors[0])
                {
                    OnPropertyChanged("Errors");
                }

                if (!_modified)
                {
                    OnPropertyChanged("Modified");
                }

                _modified = true;
                _value = value;

                OnPropertyChanged("Value");
            }
        }

        public ModelProperty(string name, T value, Func<string, T, string> validate)
        {
            Name = name;
            _value = value;
            _errors = new List<string> {validate?.Invoke(Name, _value) ?? ""};
            _validate = validate;
            _modified = false;
        }

        public override IReadOnlyCollection<string> Errors => _errors[0] != "" ? _errors : EmptyList;

        public override bool Modified => _modified;

        public void ResetModified()
        {
            _modified = false;
        }
    }

    public static class ModelPropertyValidators
    {
        public static string IsNotEmpty(string name, string data)
        {
            return string.IsNullOrEmpty(data) ? $"{name}: Geben Sie einen Wert ein." : "";
        }

        public static string IsNaturalInt(string name, string data)
        {
            return !int.TryParse(data, out var n) || n <= 0
                ? $"{name}: Der eingegebene Wert ist keine gültige positive Zahl > 0."
                : "";
        }
    }

    public abstract class Model : INotifyPropertyChanged
    {
        private readonly List<ModelBaseProperty> _properties;

        protected Model()
        {
            _properties = new List<ModelBaseProperty>();

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Errors")
                {
                    OnPropertyChanged("ErrorCount");
                }
            };
        }

        protected T Add<T>(T property) where T : ModelBaseProperty
        {
            _properties.Add(property);

            property.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Errors" || args.PropertyName == "Modified")
                {
                    OnPropertyChanged(args.PropertyName);
                }
            };

            return property;
        }

        public bool Modified => _properties.Select(property => property.Modified).Any();

        public IReadOnlyCollection<string> Errors
        {
            get
            {
                var errors = new List<string>();

                foreach (var property in _properties)
                {
                    errors.AddRange(property.Errors);
                }

                return errors;
            }
        }

        public int ErrorCount => Errors.Count;

        public abstract void Apply();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}