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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MietRechner
{
	/// <summary>
	/// Interaktionslogik für OwnerPage.xaml
	/// </summary>
	public partial class OwnerPage : Page
	{
		public OwnerPage()
		{
			InitializeComponent();
        }

        private void SalutationCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SalutationCombobox.SelectedIndex != -1 && MainWindow.billing != null)
                MainWindow.billing.Owner.Salutation_ = (Salutation)SalutationCombobox.SelectedIndex;
        }

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Name = NameTextBox.Text;
		}

		private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Phone = PhoneTextBox.Text;
        }

        private void MailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.billing.Owner.Mail = MailTextBox.Text;
        }

        private void StreetTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Address.Street = StreetTextBox.Text;
		}

		private void HouseNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Address.HouseNumber = HouseNumberTextBox.Text;
		}

		private void CityTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Address.City = CityTextBox.Text;
		}

		private void PLZTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int plz;
			if (!int.TryParse(PLZTextBox.Text, out plz))
				plz = 0;

			MainWindow.billing.Owner.Address.PLZ = plz;
		}

		private void IBANTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Account.IBAN = IBANTextBox.Text;
		}

		private void BICTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Account.BIC = BICTextBox.Text;
		}

		private void BankNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Owner.Account.BankName = BankNameTextBox.Text;
		}

		public void Page_Loaded(object sender, RoutedEventArgs e)
		{
			SalutationCombobox.SelectedIndex = (int) MainWindow.billing.Owner.Salutation_;
			NameTextBox.Text = MainWindow.billing.Owner.Name;
			PhoneTextBox.Text = MainWindow.billing.Owner.Phone;
            MailTextBox.Text = MainWindow.billing.Owner.Mail;
            StreetTextBox.Text = MainWindow.billing.Owner.Address.Street;
			HouseNumberTextBox.Text = MainWindow.billing.Owner.Address.HouseNumber;
			CityTextBox.Text = MainWindow.billing.Owner.Address.City;
			PLZTextBox.Text = MainWindow.billing.Owner.Address.PLZ.ToString();
			IBANTextBox.Text = MainWindow.billing.Owner.Account.IBAN;
			BICTextBox.Text = MainWindow.billing.Owner.Account.BIC;
			BankNameTextBox.Text = MainWindow.billing.Owner.Account.BankName;
		}
    }
}
