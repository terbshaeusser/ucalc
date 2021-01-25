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
	/// Interaktionslogik für MathPage.xaml
	/// </summary>
	public partial class MathPage : Page
	{
		public MathPage()
		{
			InitializeComponent();
		}

		public void Page_Loaded(object sender, RoutedEventArgs e)
		{
			RenterCombobox.Items.Clear();
			RenterCombobox.SelectedIndex = -1;
			foreach (Renter renter in MainWindow.billing.Renters)
			{
				RenterCombobox.Items.Add(renter.Name);
			}

			CalcTextbox.Text = "";
		}

		private void RenterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (RenterCombobox.SelectedIndex == -1)
				return;

			CalcTextbox.Text = MainWindow.billing.CalculateForRenterEx(MainWindow.billing.Renters[RenterCombobox.SelectedIndex]);
		}
	}
}
