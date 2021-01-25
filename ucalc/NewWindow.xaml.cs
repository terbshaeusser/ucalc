using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using UCalc.Data;

namespace UCalc
{
    public partial class NewWindow
    {
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

        private void OnCreateClick(object sender, RoutedEventArgs args)
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

                var path = ((RecentlyOpenedItem) ReuseDataComboBox.SelectedItem).Path;
                try
                {
                    var details = new StringBuilder();
                    Billing = BillingImporter.Import(path, StartCalendar.SelectedDate.Value,
                        EndCalendar.SelectedDate.Value, details);

                    if (details.Length > 0)
                    {
                        MessageBox.Show(
                            $"Die folgenden Änderungen wurden beim Übernehmen vorgenommen:\n{details}",
                            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (IOException e)
                {
                    MessageBox.Show(
                        $"Beim Laden der Datei \"{path}\" ist ein Fehler aufgetreten!\n\nDetails: {e.Message}",
                        "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
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