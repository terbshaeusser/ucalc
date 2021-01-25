using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MietRechner
{
    /// <summary>
    /// Interaktionslogik für NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window
    {
		public Billing billing;

        public NewWindow(List<string> recentlyOpenedFiles)
        {
			InitializeComponent();

			billing = null;
			foreach (string file in recentlyOpenedFiles)
			{
				ComboBoxItem item = new ComboBoxItem();
				item.Content = System.IO.Path.GetFileNameWithoutExtension(file);
				item.Tag = file;
				LoadCombobox.Items.Add(item);
			}
		}

		private void LoadCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			LoadCombobox.IsEnabled = true;
			BrowseButton.IsEnabled = true;
		}

		private void LoadCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			LoadCombobox.IsEnabled = false;
			BrowseButton.IsEnabled = false;
		}

		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "MietRechner Datei (*.mr) | *.mr";
			if (dialog.ShowDialog() == true)
			{
				ComboBoxItem item = new ComboBoxItem();
				item.Content = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
				item.Tag = dialog.FileName;
				LoadCombobox.Items.Insert(0, item);
				LoadCombobox.SelectedItem = item;
			}
		}

		private void NextButton_Click(object sender, RoutedEventArgs e)
		{
			if (StartCalendar.SelectedDate == null)
			{
				MessageBox.Show("Wählen Sie ein Startdatum für die Abrechnung aus!", "Fehlende Eingabe", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			if (EndCalendar.SelectedDate == null)
			{
				MessageBox.Show("Wählen Sie ein Enddatum für die Abrechnung aus!", "Fehlende Eingabe", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}
			if (StartCalendar.SelectedDate >= EndCalendar.SelectedDate)
			{
				MessageBox.Show("Das Enddatum ist ungültig!", "Ungültige Eingabe", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			if (LoadCheckbox.IsChecked == true)
			{
				if (LoadCombobox.SelectedItem == null)
				{
					MessageBox.Show("Wählen Sie die Abrechnung, von der Daten übernommen werden sollen!", "Ungültige Eingabe", MessageBoxButton.OK, MessageBoxImage.Exclamation);
					return;
				}

				billing = new Billing();
				string fileName = (string) ((ComboBoxItem) LoadCombobox.SelectedItem).Tag;
				if (!billing.LoadFromFile(fileName))
				{
					MessageBox.Show("Die Datei \"" + fileName + "\" konnte nicht geladen werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
					billing = null;
					return;
				}

				// Remove depatured renters and costs that were only paid once
				string summary = "";
				billing.StartDate = (DateTime) StartCalendar.SelectedDate;
				billing.EndDate = (DateTime) EndCalendar.SelectedDate;

				for (int i = 0; i < billing.Renters.Count; )
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
					MessageBox.Show("Die folgenden Änderungen wurden beim Übernehmen vorgenommen:\n" + summary, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
				billing = new Billing((DateTime) StartCalendar.SelectedDate, (DateTime) EndCalendar.SelectedDate);

			DialogResult = true;
		}

		private void NewForm_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			base.OnPreviewMouseUp(e);
			if (Mouse.Captured is Calendar || Mouse.Captured is System.Windows.Controls.Primitives.CalendarItem)
			{
				Mouse.Capture(null);
			}
		}
	}
}
