using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UCalc.Data;

namespace UCalc
{
    public partial class NewWindow
    {
        public Billing SavedBilling { get; private set; }
        public Billing Billing { get; private set; }

        public NewWindow()
        {
            InitializeComponent();
        }

        private void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog {Filter = "MietRechner Datei (*.mr) | *.mr"};

            if (dialog.ShowDialog() == true)
            {
                App.RecentlyOpenedList.Add(new RecentlyOpenedItem(dialog.FileName));
                ReuseDataComboBox.SelectedIndex = 0;
            }
        }

        private void OnCreateClick(object sender, RoutedEventArgs e)
        {
            if (StartCalendar.SelectedDate == null)
            {
                MessageBox.Show("Wählen Sie ein Startdatum für die Abrechnung aus!", "Fehlende Eingabe",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (EndCalendar.SelectedDate == null)
            {
                MessageBox.Show("Wählen Sie ein Enddatum für die Abrechnung aus!", "Fehlende Eingabe",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (StartCalendar.SelectedDate >= EndCalendar.SelectedDate)
            {
                MessageBox.Show("Das Enddatum ist ungültig!", "Ungültige Eingabe", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            if (ReuseDataCheckBox.IsChecked == true)
            {
                if (ReuseDataComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Wählen Sie die Abrechnung, von der Daten übernommen werden sollen!",
                        "Ungültige Eingabe", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                throw new NotImplementedException();
                /*billing = new Billing();
                string fileName = (string) ((ComboBoxItem) LoadCombobox.SelectedItem).Tag;
                if (!billing.LoadFromFile(fileName))
                {
                    MessageBox.Show("Die Datei \"" + fileName + "\" konnte nicht geladen werden!", "Fehler!",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    billing = null;
                    return;
                }

                // Remove depatured renters and costs that were only paid once
                string summary = "";
                billing.StartDate = (DateTime) StartCalendar.SelectedDate;
                billing.EndDate = (DateTime) EndCalendar.SelectedDate;

                for (int i = 0; i < billing.Renters.Count;)
                {
                    Renter renter = billing.Renters[i];
                    if (renter.DepartureDate != null && renter.DepartureDate < billing.StartDate)
                    {
                        summary += "- Mieter \"" + renter.Name + "\" wurde entfernt, da er ausgezogen ist.\n";
                        foreach (Cost cost in billing.Costs)
                        {
                            cost.AffectedRenters.Remove(renter);
                        }

                        billing.Renters.Remove(renter);
                        continue;
                    }
                    else if (renter.EntryDate != null && renter.EntryDate <= billing.StartDate)
                    {
                        summary += "- Das Einzugsdatum von Mieter \"" + renter.Name + "\" wurde entfernt.\n";
                        renter.EntryDate = null;
                    }

                    ++i;
                }

                if (summary != "")
                    MessageBox.Show("Die folgenden Änderungen wurden beim Übernehmen vorgenommen:\n" + summary,
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);*/
            }
            else
            {
                SavedBilling = new Billing();
                Billing = new Billing
                {
                    StartDate = StartCalendar.SelectedDate.Value,
                    EndDate = EndCalendar.SelectedDate.Value
                };
            }

            DialogResult = true;
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (Mouse.Captured is Calendar || Mouse.Captured is System.Windows.Controls.Primitives.CalendarItem)
            {
                Mouse.Capture(null);
            }
        }
    }
}