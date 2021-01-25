using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// Interaktionslogik für EditCostWindow.xaml
	/// </summary>
	public partial class EditCostWindow : Window
	{
		/** The current cost */
		private Cost cost;
		public ObservableCollection<CheckedListItem<Renter>> Renters { get; set; }
		/** List with frames of the entries */
		private List<Frame> entries;

		public EditCostWindow(Cost cost)
		{
			this.cost = cost;

			Renters = new ObservableCollection<CheckedListItem<Renter>>();
			foreach (Renter renter in MainWindow.billing.Renters)
			{
				Renters.Add(new CheckedListItem<Renter>(renter));
			}

			entries = new List<Frame>();

			InitializeComponent();
			DataContext = this;
		}

		private void AffectedCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (AffectedListbox != null)
				AffectedListbox.IsEnabled = AffectedCombobox.SelectedIndex == 1;
		}

		private void CountTextbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int count;
			if (!int.TryParse(CountTextbox.Text, out count) || count <= 0)
				return;

			if (count < entries.Count)
			{
				while (count != entries.Count)
				{
					Panel.Children.Remove(entries[count]);
					entries.RemoveAt(count);
				}				
			}
			else if (count > entries.Count)
			{
				for (int i = entries.Count; i < count; ++i)
				{
					Frame frame = new Frame();
					frame.Height = Double.NaN;
					frame.Navigate(new CostTimeSlotPage(new CostEntry(MainWindow.billing.StartDate, MainWindow.billing.EndDate, 0), i));
					Panel.Children.Add(frame);
					entries.Add(frame);
				}
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			NameTextBox.Text = cost.Name;
			if (cost.AffectsAllRenters)
			{
				AffectedCombobox.SelectedIndex = 0;
				AffectedListbox.IsEnabled = false;
			}
			else
			{
				AffectedCombobox.SelectedIndex = 1;
				AffectedListbox.IsEnabled = true;
				foreach (Renter renter in cost.AffectedRenters)
				{
					foreach (CheckedListItem<Renter> item in Renters)
					{
						if (item.Item == renter)
						{
							item.IsChecked = true;
							break;
						}
					}
				}
			}
			DivisionKindCombobox.SelectedIndex = (int) cost.Division;
            DisplayCheckBox.IsChecked = cost.DisplayInBill;
            for (int i = 0; i < cost.Entries.Count; ++i)
			{
				Frame frame = new Frame();
				frame.Height = Double.NaN;
				frame.Navigate(new CostTimeSlotPage(cost.Entries[i], i));
				Panel.Children.Add(frame);
				entries.Add(frame);
			}
			CountTextbox.Text = cost.Entries.Count.ToString();
		}

		private bool AreDateRangesOverlapping(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
		{
			if (start2 >= start1 && start2 <= end1)
				return true;
			if (start1 >= start2 && start1 <= end2)
				return true;

			return false;
		}

		private void ApplyButton_Click(object sender, RoutedEventArgs e)
		{
			// Check the input values
			bool error = false;
			String errorStr = "";

			if (NameTextBox.Text == "")
			{
				errorStr += "- Kein Name vorhanden\n";
				NameLabel.Foreground = Brushes.Red;
				error = true;
			}
			else
				NameLabel.Foreground = MainWindow.highlightColor;
			decimal dummy;
			int dummy2;
			if (!int.TryParse(CountTextbox.Text, out dummy2) || dummy2 <= 0)
			{
				errorStr += "- Die angegebene Zahl an Zeiträumen ist ungültig\n";
				CountLabel.Foreground = Brushes.Red;
				error = true;
			}
			else
				CountLabel.Foreground = MainWindow.highlightColor;

			List<CostTimeSlotPage> pages = new List<CostTimeSlotPage>();
			foreach (Frame frame in entries)
			{
				pages.Add((CostTimeSlotPage) frame.Content);				
			}

			int coveredDays = 0;
			for (int i = 0; i < pages.Count; ++i)
			{
				// Check if dates overlapp
				CostTimeSlotPage page = pages[i];
				if (!decimal.TryParse(page.PriceTextbox.Text, out dummy) || dummy < 0)
				{
					errorStr += "- Die angegebenen Kosten in Zeitraum " + (i + 1).ToString() + " sind gültige Zahl >= 0\n";
					page.PriceLabel.Foreground = Brushes.Red;
					error = true;
				}
				else
					page.PriceLabel.Foreground = MainWindow.highlightColor;
				if (page.StartDatePicker.SelectedDate == null)
				{
					errorStr += "- Für Zeitraum " + (i + 1).ToString() + " wurde kein Startdatum eingegeben\n";
					page.StartDateLabel.Foreground = Brushes.Red;
					error = true;
				}
				else
					page.StartDateLabel.Foreground = MainWindow.highlightColor;
				if (page.EndDatePicker.SelectedDate == null)
				{
					errorStr += "- Für Zeitraum " + (i + 1).ToString() + " wurde kein Enddatum eingegeben\n";
					page.EndDateLabel.Foreground = Brushes.Red;
					error = true;
				}
				else
					page.EndDateLabel.Foreground = MainWindow.highlightColor;
				if (page.StartDatePicker.SelectedDate == null || page.EndDatePicker.SelectedDate == null)
					continue;
				DateTime startDate = (DateTime) page.StartDatePicker.SelectedDate;
				DateTime endDate = (DateTime) page.EndDatePicker.SelectedDate;
				if (startDate > endDate)
				{
					errorStr += "- Das Enddatum ist früher als das Startdatum in Zeitraum " + (i + 1).ToString() + "\n";
					page.EndDateLabel.Foreground = Brushes.Red;
					error = true;
					continue;
				}

				if (startDate <= MainWindow.billing.StartDate && endDate >= MainWindow.billing.StartDate)
				{
					coveredDays += (endDate - MainWindow.billing.StartDate).Days + 1;
				}
				else if (startDate <= MainWindow.billing.EndDate)
				{
					DateTime tmp = MainWindow.billing.EndDate;
					if (tmp > endDate)
						tmp = endDate;

					coveredDays += (tmp - startDate).Days + 1;
				}					

				for (int j = i + 1; j < pages.Count; ++j)
				{
					CostTimeSlotPage page2 = pages[j];
					if (page2.StartDatePicker.SelectedDate == null || page2.EndDatePicker.SelectedDate == null)
						continue;
					DateTime startDate2 = (DateTime) page2.StartDatePicker.SelectedDate;
					DateTime endDate2 = (DateTime) page2.EndDatePicker.SelectedDate;

					if (AreDateRangesOverlapping(startDate, endDate, startDate2, endDate2)) {
						errorStr += "- Die Zeiträume " + (i + 1).ToString() + " und " + (j + 1).ToString() + " überlappen sich\n";
						page.StartDateLabel.Foreground = Brushes.Red;
						page.EndDateLabel.Foreground = Brushes.Red;
						error = true;
					}
				}
			}

			if (coveredDays < (MainWindow.billing.EndDate - MainWindow.billing.StartDate).Days + 1)
			{
				errorStr += "- Die Zeiträume decken nicht alle Tage der Abrechnung ab\n";
				error = true;
			}

			if (error)
			{
				MessageBox.Show("Die eingegebenen Daten sind ungültig:\n" + errorStr, "Ungültige Eingabe!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			// Store input values into cost
			cost.Name = NameTextBox.Text;
			cost.AffectsAllRenters = AffectedCombobox.SelectedIndex == 0;
			cost.AffectedRenters.Clear();
			if (!cost.AffectsAllRenters)
			{
				foreach (CheckedListItem<Renter> item in Renters)
				{
					if (item.IsChecked)
						cost.AffectedRenters.Add(item.Item);
				}
			}
			cost.Division = (CostDivision) DivisionKindCombobox.SelectedIndex;
            cost.DisplayInBill = DisplayCheckBox.IsChecked == true;
            cost.Entries.Clear();
			foreach (Frame frame in entries)
			{
				CostTimeSlotPage page = (CostTimeSlotPage) frame.Content;
                cost.Entries.Add(page.NewEntry);
			}
			MainWindow.billing.ForceModified();

			DialogResult = true;
		}
    }
}
