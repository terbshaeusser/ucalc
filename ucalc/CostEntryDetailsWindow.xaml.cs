using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UCalc.Annotations;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc
{
    public partial class CostEntryDetailsWindow : INotifyPropertyChanged
    {
        public CostEntryDetailsProperty Details { get; }
        private decimal? _result;

        public decimal? Result
        {
            get => _result;
            private set
            {
                if (_result == value)
                {
                    return;
                }

                _result = value;
                ApplyButton.IsEnabled = value != null;
                OnPropertyChanged();
                OnPropertyChanged("ResultStr");
            }
        }

        public string ResultStr =>
            _result != null ? $"Betrag: {_result.Value.ToString(Constants.DisplayPrecisionFormat)} €" : "";

        public CostEntryDetailsWindow(CostEntryDetailsProperty details)
        {
            Details = details;
            InitializeComponent();

            Details.TotalAmount.PropertyChanged += DetailsPropertyChanged;
            Details.UnitCount.PropertyChanged += DetailsPropertyChanged;
            Details.DiscountsInUnits.CollectionChanged += DetailsDiscountChanged;

            DetailsPropertyChanged(null, null);

            if (Details.TotalAmount.ConvertedValue != 0 || Details.UnitCount.ConvertedValue != 0 ||
                Details.DiscountsInUnits.Count != 0)
            {
                TabControl.SelectedIndex = 1;
            }
        }

        private void DetailsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var totalAmount = Details.TotalAmount.ConvertedValue;
            var unitCount = Details.UnitCount.ConvertedValue;

            if (totalAmount == null || unitCount == null)
            {
                Result = null;
                return;
            }

            decimal totalDiscount = 0;
            foreach (var discount in Details.DiscountsInUnits)
            {
                var n = discount.ConvertedValue;
                if (n == null)
                {
                    Result = null;
                    return;
                }

                totalDiscount += n.Value;
            }

            Result = unitCount != 0
                ? totalAmount / unitCount * (unitCount - totalDiscount)
                : null;
        }

        private void DetailsDiscountChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var discount in Details.DiscountsInUnits)
            {
                discount.PropertyChanged -= DetailsPropertyChanged;
                discount.PropertyChanged += DetailsPropertyChanged;
            }
        }

        private void OnCEClick(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = "";
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text += (string) ((Button) sender).Content;
        }

        private void OnRoundClick(object sender, RoutedEventArgs e)
        {
            ResultTextBox.Text = "ceil(" + ResultTextBox.Text + ")";
        }

        private void OnEvalClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Result = Calc(ResultTextBox.Text);

                ResultTextBox.Text = Result.Value.ToString(Constants.InternalPrecisionFormat);
            }
            catch
            {
                Result = null;

                ResultTextBox.Text = "Ungültig!";
            }
        }

        private void OnResultTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnEvalClick(sender, null);
            }
        }

        private static decimal Calc(string str)
        {
            var tokens = Tokenize(str);
            var pos = 0;

            var result = CalcPlus(tokens, ref pos);
            if (pos != tokens.Count)
            {
                throw new Exception();
            }

            return result;
        }

        private static decimal CalcPlus(List<string> tokens, ref int pos)
        {
            var result = CalcMul(tokens, ref pos);

            while (true)
            {
                // + or - might follow
                if (pos >= tokens.Count)
                {
                    break;
                }

                var op = tokens[pos];
                if (op != "+" && op != "-")
                {
                    break;
                }

                ++pos;

                var result2 = CalcMul(tokens, ref pos);
                if (op == "+")
                {
                    result += result2;
                }
                else
                {
                    result -= result2;
                }
            }

            return result;
        }

        private static decimal CalcMul(List<string> tokens, ref int pos)
        {
            var result = CalcSimple(tokens, ref pos);

            while (true)
            {
                // * or / might follow
                if (pos >= tokens.Count)
                {
                    break;
                }

                var op = tokens[pos];
                if (op != "*" && op != "/")
                {
                    break;
                }

                ++pos;

                var result2 = CalcSimple(tokens, ref pos);
                if (op == "*")
                {
                    result *= result2;
                }
                else
                {
                    result /= result2;
                }
            }

            return result;
        }

        private static decimal CalcSimple(List<string> tokens, ref int pos)
        {
            var token = tokens[pos];
            ++pos;

            decimal result;

            switch (token)
            {
                case "(":
                    result = CalcPlus(tokens, ref pos);
                    if (tokens[pos] != ")")
                        throw new Exception();
                    ++pos;
                    return result;
                case "ceil":
                    result = CalcPlus(tokens, ref pos);
                    return Math.Round(result, 2);
                default:
                    return decimal.Parse(token);
            }
        }

        private static List<string> Tokenize(string str)
        {
            var tokens = new List<string>();
            var token = "";
            var i = 0;

            while (i < str.Length)
            {
                var c = str[i];
                ++i;

                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case ',':
                        token += c;
                        break;
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '(':
                    case ')':
                        if (token != "")
                        {
                            tokens.Add(token);
                        }

                        tokens.Add(c.ToString());
                        token = "";
                        break;
                    default:
                        // Must be ceil
                        if (token != "")
                        {
                            tokens.Add(token);
                        }

                        if (str.Substring(i - 1, 4) == "ceil")
                        {
                            tokens.Add("ceil");
                            i += 3;
                        }
                        else
                        {
                            throw new Exception();
                        }

                        break;
                }
            }

            if (token != "")
            {
                tokens.Add(token);
            }

            return tokens;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnAddDiscountClick(object sender, RoutedEventArgs e)
        {
            Details.DiscountsInUnits.Add();
        }

        private void OnDiscountDeleteClick(object sender, RoutedEventArgs e)
        {
            var discount = (DiscountProperty) ((HighlightButton) sender).DataContext;

            Details.DiscountsInUnits.Remove(discount);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Details.TotalAmount.PropertyChanged -= DetailsPropertyChanged;
            Details.UnitCount.PropertyChanged -= DetailsPropertyChanged;
            Details.DiscountsInUnits.CollectionChanged -= DetailsDiscountChanged;

            foreach (var discount in Details.DiscountsInUnits)
            {
                discount.PropertyChanged -= DetailsPropertyChanged;
            }
        }
    }
}