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
	/// Interaktionslogik für HousePage.xaml
	/// </summary>
	public partial class HousePage : Page
	{
		public HousePage()
		{
			InitializeComponent();
		}

		private void FlatCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int flatCount;
			if (!int.TryParse(FlatCountTextBox.Text, out flatCount))
				flatCount = 0;

			MainWindow.billing.FlatCount = flatCount;
		}

		private void StreetTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Address.Street = StreetTextBox.Text;
		}

		private void HouseNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Address.HouseNumber = HouseNumberTextBox.Text;
		}

		private void CityTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			MainWindow.billing.Address.City = CityTextBox.Text;
		}

		private void PLZTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int plz;
			if (!int.TryParse(PLZTextBox.Text, out plz))
				plz = 0;

			MainWindow.billing.Address.PLZ = plz;
		}

		public void Page_Loaded(object sender, RoutedEventArgs e)
		{
			FlatCountTextBox.Text = MainWindow.billing.FlatCount.ToString();
			StreetTextBox.Text = MainWindow.billing.Address.Street;
			HouseNumberTextBox.Text = MainWindow.billing.Address.HouseNumber;
			CityTextBox.Text = MainWindow.billing.Address.City;
			PLZTextBox.Text = MainWindow.billing.Address.PLZ.ToString();
		}
	}
}
