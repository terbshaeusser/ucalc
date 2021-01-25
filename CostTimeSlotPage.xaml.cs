using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MietRechner
{
    /// <summary>
    /// Interaktionslogik für CostTimeSlotPage.xaml
    /// </summary>
    public partial class CostTimeSlotPage : Page
    {
        private CostEntry entry;
        private int index;

        public CostTimeSlotPage(CostEntry entry, int index)
        {
            InitializeComponent();
            this.entry = entry.Clone();
            this.index = index;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TitleLabel.Content = "Zeitraum " + (index + 1).ToString();
            StartDatePicker.SelectedDate = entry.Start;
            EndDatePicker.SelectedDate = entry.End;
            PriceTextbox.Text = Billing.CeilToString(entry.Price, Billing.InternalPrecision);
        }

        private void ExtraCalc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ExtraCalcWindow calcWindow = new ExtraCalcWindow(entry);
            calcWindow.ShowDialog();
            PriceTextbox.Text = Billing.CeilToString(entry.Price, Billing.InternalPrecision);
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDatePicker.SelectedDate != null)
                entry.Start = StartDatePicker.SelectedDate.Value;
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndDatePicker.SelectedDate != null)
                entry.End = EndDatePicker.SelectedDate.Value;
        }

        private void PriceTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(PriceTextbox.Text, out decimal price))
                entry.Price = price;
        }

        /**
         * Returns the new cloned entry
         */
        public CostEntry NewEntry
        {
            get
            {
                return entry;
            }
        }
    }
}
