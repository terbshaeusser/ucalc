using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ucalc.Annotations;

namespace UCalc.Models
{
    public class ModelProperty : INotifyPropertyChanged
    {
        private readonly Model _model;
        private object _value;
        private string _error;
        private readonly Func<object, string> _validate;

        public object Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value))
                {
                    return;
                }

                var oldError = Error;
                Error = _validate?.Invoke(value);
                if (oldError == null && Error != null || oldError != null && Error == null)
                {
                    _model.OnPropertyChanged("ErrorCount");
                }

                Modified = true;
                _value = value;
            }
        }

        public string Error
        {
            get => _error;
            private set
            {
                if (_error == value)
                {
                    return;
                }

                _error = value;
                OnPropertyChanged();
            }
        }

        public bool Modified { get; set; }

        public ModelProperty(Model model, object value, Func<object, string> validate)
        {
            _model = model;
            _value = value;
            _error = validate?.Invoke(value);
            _validate = validate;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ModelProperty<T> : ModelProperty
    {
        public new T Value
        {
            get => (T) base.Value;
            set => base.Value = value;
        }

        public ModelProperty(Model model, T value, Func<T, string> validate) : base(model, value,
            validate == null ? (Func<object, string>) null : o => validate((T) o))
        {
        }
    }

    public static class ModelPropertyValidators
    {
        public static string IsNotEmpty(string data)
        {
            return string.IsNullOrEmpty(data) ? "Geben Sie einen Wert ein" : null;
        }
    }

    public abstract class Model : INotifyPropertyChanged
    {
        public ModelProperty[] Properties { get; protected set; }

        public bool Modified => Properties.Select(property => property.Modified).Any();

        public int ErrorCount => Properties.Select(property => string.IsNullOrEmpty(property.Error) ? 0 : 1).Sum();

        public virtual void Apply()
        {
            if (ErrorCount > 0)
            {
                throw new InvalidOperationException();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}