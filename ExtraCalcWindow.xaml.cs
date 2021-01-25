using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MietRechner
{
    /// <summary>
    /// Interaktionslogik für ExtraCalcWindow.xaml
    /// </summary>
    public partial class ExtraCalcWindow : Window
    {
        private CostEntry entry;
        private decimal? result;
        private bool resultFromCalc;

        public ExtraCalcWindow(CostEntry entry)
        {
            InitializeComponent();
            this.entry = entry;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (entry.CubicInfo == null)
            {
                CalcTextbox.Text = entry.Price.ToString();
                Tabs.SelectedIndex = 0;

                EvalButton_Click(null, null);
            }
            else
            {
                CubicMeterSumTextbox.Text = entry.CubicInfo.CubicMeterSum.ToString();
                CubicMeterSumPriceTextbox.Text = entry.CubicInfo.CubicMeterSumPrice.ToString("0.00##################");
                Discount1CubicMeterTextbox.Text = entry.CubicInfo.Discount1CubicMeter.ToString("0.00##################");
                Discount2CubicMeterTextbox.Text = entry.CubicInfo.Discount2CubicMeter.ToString("0.00##################");
                Discount3CubicMeterTextbox.Text = entry.CubicInfo.Discount3CubicMeter.ToString("0.00##################");
                Discount4CubicMeterTextbox.Text = entry.CubicInfo.Discount4CubicMeter.ToString("0.00##################");
                Tabs.SelectedIndex = 1;

                Textbox_TextChanged(null, null);
            }
        }

        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                decimal cubicMeters = decimal.Parse(CubicMeterSumTextbox.Text);
                decimal perCubicMeter = decimal.Parse(CubicMeterSumPriceTextbox.Text) / cubicMeters;
                cubicMeters -= decimal.Parse(Discount1CubicMeterTextbox.Text == "" ? "0" : Discount1CubicMeterTextbox.Text);
                cubicMeters -= decimal.Parse(Discount2CubicMeterTextbox.Text == "" ? "0" : Discount2CubicMeterTextbox.Text);
                cubicMeters -= decimal.Parse(Discount3CubicMeterTextbox.Text == "" ? "0" : Discount3CubicMeterTextbox.Text);
                cubicMeters -= decimal.Parse(Discount4CubicMeterTextbox.Text == "" ? "0" : Discount4CubicMeterTextbox.Text);
                decimal sum = cubicMeters * perCubicMeter;

                ResultLabel.Content = "Betrag: " + Billing.CeilToString(sum, Billing.InternalPrecision) + " €";
                ResultLabel.Foreground = MainWindow.highlightColor;
                ApplyButton.IsEnabled = true;
                result = sum;
                resultFromCalc = false;
            }
            catch
            {
                ResultLabel.Content = "Ungültig!";
                ResultLabel.Foreground = Brushes.Red;
                ApplyButton.IsEnabled = false;
                result = null;
            }
        }

        private void CEButton_Click(object sender, RoutedEventArgs e)
        {
            CalcTextbox.Text = "";
        }

        private void CalcButton_Click(object sender, RoutedEventArgs e)
        {
            CalcTextbox.Text += ((Button)sender).Content;
        }

        private void RoundButton_Click(object sender, RoutedEventArgs e)
        {
            CalcTextbox.Text = "ceil(" + CalcTextbox.Text + ")";
        }

        private void CalcTextbox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                EvalButton_Click(null, e);
        }

        private void EvalButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                result = Calc(CalcTextbox.Text);
                ResultLabel.Content = "Betrag: " + Billing.CeilToString(result.Value, Billing.InternalPrecision) + " €";
                ResultLabel.Foreground = MainWindow.highlightColor;
                ApplyButton.IsEnabled = true;
                CalcTextbox.Text = result.Value.ToString();
                resultFromCalc = true;
            }
            catch
            {
                ResultLabel.Content = "Ungültig!";
                ResultLabel.Foreground = Brushes.Red;
                ApplyButton.IsEnabled = false;
            }
        }

        private decimal Calc(string str)
        {
            List<string> tokens = Tokenize(str);
            int pos = 0;

            decimal result = CalcPlus(tokens, ref pos);
            if (pos != tokens.Count)
                throw new Exception();
            return result;
        }

        private decimal CalcPlus(List<string> tokens, ref int pos)
        {
            decimal result = CalcMul(tokens, ref pos);

            while (true)
            {
                // + or - might follow
                if (pos >= tokens.Count)
                    break;

                string op = tokens[pos];
                if (op != "+" && op != "-")
                    break;
                ++pos;

                decimal result2 = CalcMul(tokens, ref pos);
                if (op == "+")
                    result += result2;
                else
                    result -= result2;
            }

            return result;
        }

        private decimal CalcMul(List<string> tokens, ref int pos)
        {
            decimal result = CalcSimple(tokens, ref pos);

            while (true)
            {
                // * or / might follow
                if (pos >= tokens.Count)
                    break;

                string op = tokens[pos];
                if (op != "*" && op != "/")
                    break;
                ++pos;

                decimal result2 = CalcSimple(tokens, ref pos);
                if (op == "*")
                    result *= result2;
                else
                    result /= result2;
            }

            return result;
        }

        private decimal CalcSimple(List<string> tokens, ref int pos)
        {
            string token = tokens[pos];
            ++pos;

            switch (token)
            {
                case "(":
                    {
                        decimal result = CalcPlus(tokens, ref pos);
                        if (tokens[pos] != ")")
                            throw new Exception();
                        ++pos;
                        return result;
                    }
                case "ceil":
                    {
                        decimal result = CalcPlus(tokens, ref pos);
                        return Billing.Ceil(result, 2);
                    }
                default:
                    return decimal.Parse(token);
            }
        }

        private List<string> Tokenize(string str)
        {
            List<string> tokens = new List<string>();
            string token = "";
            int i = 0;

            while (i < str.Length)
            {
                char c = str[i];
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
                        {
                            token += c;
                            break;
                        }
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '(':
                    case ')':
                        {
                            if (token != "")
                                tokens.Add(token);
                            tokens.Add(c.ToString());
                            token = "";
                            break;
                        }
                    default:
                        {
                            // Must be ceil
                            if (token != "")
                                tokens.Add(token);

                            if (str.Substring(i - 1, 4) == "ceil")
                            {
                                tokens.Add("ceil");
                                i += 3;
                            }
                            else
                                throw new Exception();
                            break;
                        }
                }
            }

            if (token != "")
                tokens.Add(token);

            return tokens;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (result == null)
                return;

            if (resultFromCalc)
            {
                if (entry.CubicInfo != null)
                {
                    if (MessageBox.Show("Die Änderungen des Betrags entfernen die Informationen über die m³. Möchten Sie die Änderungen wirklich übernehmen?", "Warnung!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                        return;

                    entry.CubicInfo = null;
                }
            }
            else
            {
                if (entry.CubicInfo == null)
                    entry.CubicInfo = new CubicInfo();

                entry.CubicInfo.CubicMeterSum = decimal.Parse(CubicMeterSumTextbox.Text);
                entry.CubicInfo.CubicMeterSumPrice = decimal.Parse(CubicMeterSumPriceTextbox.Text);
                entry.CubicInfo.Discount1CubicMeter = decimal.Parse(Discount1CubicMeterTextbox.Text == "" ? "0" : Discount1CubicMeterTextbox.Text);
                entry.CubicInfo.Discount2CubicMeter = decimal.Parse(Discount2CubicMeterTextbox.Text == "" ? "0" : Discount2CubicMeterTextbox.Text);
                entry.CubicInfo.Discount3CubicMeter = decimal.Parse(Discount3CubicMeterTextbox.Text == "" ? "0" : Discount3CubicMeterTextbox.Text);
                entry.CubicInfo.Discount4CubicMeter = decimal.Parse(Discount4CubicMeterTextbox.Text == "" ? "0" : Discount4CubicMeterTextbox.Text);
            }

            entry.Price = Billing.Ceil(result.Value, 5);
            Close();
        }
    }
}
