using sinkien.IBAN4Net;
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
	/// Interaktionslogik für EditRenterWindow.xaml
	/// </summary>
	public partial class EditRenterWindow : Window
	{
		/** The current renter */
		private Renter renter;

		public EditRenterWindow(Renter renter)
		{
			InitializeComponent();
			this.renter = renter;
		}

		private void EntryCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			EntryDatePicker.IsEnabled = true;
		}

		private void EntryCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			EntryDatePicker.IsEnabled = false;
		}

		private void DepatureCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			DepatureDatePicker.IsEnabled = true;
		}

		private void DepatureCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			DepatureDatePicker.IsEnabled = false;
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
			int dummy;
			if (!int.TryParse(PersonTextBox.Text, out dummy) || dummy < 1)
			{
				errorStr += "- Personenanzahl ist keine gültige Zahl > 0\n";
				PersonLabel.Foreground = Brushes.Red;
				error = true;
			}
			else
				PersonLabel.Foreground = MainWindow.highlightColor;
			if (IBANTextBox.Text != "" || BankNameTextBox.Text != "")
			{
				try
				{
					IbanUtils.Validate(IBANTextBox.Text);
					IBANLabel.Foreground = MainWindow.highlightColor;
				}
				catch (IbanFormatException)
				{
					errorStr += "- IBAN ist ungültig\n";
					IBANLabel.Foreground = Brushes.Red;
					error = true;
				}
				try
				{
                    if (BICTextBox.Text != "")
                    {
                        BicUtils.ValidateBIC(BICTextBox.Text);
                        BICLabel.Foreground = MainWindow.highlightColor;
                    }
				}
				catch (BicFormatException)
				{
					errorStr += "- BIC ist ungültig\n";
					BICLabel.Foreground = Brushes.Red;
					error = true;
				}
				if (BankNameTextBox.Text == "")
				{
					errorStr += "- Kein Bankname vorhanden\n";
					BankNameLabel.Foreground = Brushes.Red;
					error = true;
				}
				else
					BankNameLabel.Foreground = MainWindow.highlightColor;
			}
			else
			{
				IBANLabel.Foreground = MainWindow.highlightColor;
				BICLabel.Foreground = MainWindow.highlightColor;
				BankNameLabel.Foreground = MainWindow.highlightColor;
			}
			if (EntryCheckbox.IsChecked == true)
			{
				if (EntryDatePicker.SelectedDate < MainWindow.billing.StartDate || EntryDatePicker.SelectedDate > MainWindow.billing.EndDate)
				{
					errorStr += "- Einzugsdatum befindet sich nicht im Bereich der Abrechnung\n";
					EntryCheckbox.Foreground = Brushes.Red;
					error = true;
				}
			}
			else
				EntryCheckbox.Foreground = MainWindow.highlightColor;
			if (DepatureCheckbox.IsChecked == true)
			{
				if (DepatureDatePicker.SelectedDate < MainWindow.billing.StartDate || DepatureDatePicker.SelectedDate > MainWindow.billing.EndDate)
				{
					errorStr += "- Auszugsdatum befindet sich nicht im Bereich der Abrechnung\n";
					DepatureCheckbox.Foreground = Brushes.Red;
					error = true;
				}
				else if (EntryCheckbox.IsChecked == true && EntryDatePicker.SelectedDate >= DepatureDatePicker.SelectedDate)
				{
					errorStr += "- Das Auszugsdatum kann nicht vor dem Einzugsdatum liegen\n";
					DepatureCheckbox.Foreground = Brushes.Red;
					error = true;
				}
			}
			else
				DepatureCheckbox.Foreground = MainWindow.highlightColor;
			decimal dummy2;
			if (!decimal.TryParse(PaidRentTextBox.Text, out dummy2) || dummy2 < 0)
			{
				errorStr += "- Bezahlte Miete ist keine gültige Zahl >= 0\n";
				PaidRentLabel.Foreground = Brushes.Red;
				error = true;
			}
			else
				PaidRentLabel.Foreground = MainWindow.highlightColor;

			if (error)
			{
				MessageBox.Show("Die eingegebenen Daten sind ungültig:\n" + errorStr, "Ungültige Eingabe!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				return;
			}

			// Store input values into renter
			renter.Salutation_ = (Salutation) SalutationCombobox.SelectedIndex;
			renter.Name = NameTextBox.Text;
			renter.PersonCount = int.Parse(PersonTextBox.Text);
			renter.Account.IBAN = IBANTextBox.Text;
			renter.Account.BIC = BICTextBox.Text;
			renter.Account.BankName = BankNameTextBox.Text;
			if (EntryCheckbox.IsChecked == true)
				renter.EntryDate = EntryDatePicker.SelectedDate;
			else
				renter.EntryDate = null;
			if (DepatureCheckbox.IsChecked == true)
				renter.DepartureDate = DepatureDatePicker.SelectedDate;
			else
				renter.DepartureDate = null;
			renter.PaidRent = decimal.Parse(PaidRentTextBox.Text);
			renter.Message1 = Message1TextBox.Text;
			renter.Message2 = Message2TextBox.Text;

			DialogResult = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SalutationCombobox.SelectedIndex = (int) renter.Salutation_;
			NameTextBox.Text = renter.Name;
			PersonTextBox.Text = renter.PersonCount.ToString();
			IBANTextBox.Text = renter.Account.IBAN;
			BICTextBox.Text = renter.Account.BIC;
			BankNameTextBox.Text = renter.Account.BankName;
			if (renter.EntryDate == null)
				EntryCheckbox.IsChecked = false;
			else {
				EntryCheckbox.IsChecked = true;
				EntryDatePicker.SelectedDate = renter.EntryDate;
			}
			if (renter.DepartureDate == null)
				DepatureCheckbox.IsChecked = false;
			else {
				DepatureCheckbox.IsChecked = true;
				DepatureDatePicker.SelectedDate = renter.DepartureDate;
			}
			PaidRentTextBox.Text = renter.PaidRent.ToString();
			Message1TextBox.Text = renter.Message1;
			Message2TextBox.Text = renter.Message2;
		}
	}
}
