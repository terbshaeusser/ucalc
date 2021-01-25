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
	/// Interaktionslogik für CostEntryPage.xaml
	/// </summary>
	public partial class CostEntryPage : Page
	{
		/** The parent page */
		private CostsPage page;
		/** The connected cost */
		private Cost cost;

		public CostEntryPage(CostsPage page, Cost cost)
		{
			InitializeComponent();
			this.page = page;
			this.cost = cost;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			EntryLabel.Content = cost.Name + "\nBetroffene: " + GetAffectedStr();
		}

		private void DeletePanel_MouseEnter(object sender, MouseEventArgs e)
		{
			DeletePanel.Background = Brushes.DarkRed;
			DeleteImage.Source = new BitmapImage(new Uri("Images/Delete_over.png", UriKind.RelativeOrAbsolute));
		}

		private void DeletePanel_MouseLeave(object sender, MouseEventArgs e)
		{
			DeletePanel.Background = Brushes.Transparent;
			DeleteImage.Source = new BitmapImage(new Uri("Images/Delete.png", UriKind.RelativeOrAbsolute));
		}

		private void DeletePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (MessageBox.Show("Möchten Sie den Kostenpunkt \"" + cost.Name + "\" wirklich entfernen?", "Kostenpunkt entfernen?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				MainWindow.billing.Costs.Remove(cost);
				foreach (Frame frame in page.CostsPanel.Children.OfType<Frame>())
				{
					if (frame.Content == this)
					{
						page.CostsPanel.Children.Remove(frame);
						MainWindow.billing.ForceModified();
						break;
					}
				}
			}
		}

		private void EntryPanel_MouseEnter(object sender, MouseEventArgs e)
		{
			EntryPanel.Background = MainWindow.highlightColor;
			EntryLabel.Foreground = Brushes.White;
			EntryImage.Source = new BitmapImage(new Uri("Images/Costs.png", UriKind.RelativeOrAbsolute));

			DeletePanel.Background = Brushes.DarkRed;
			DeleteImage.Source = new BitmapImage(new Uri("Images/Delete_over.png", UriKind.RelativeOrAbsolute));
		}

		private void EntryPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			EntryPanel.Background = Brushes.Transparent;
			EntryLabel.Foreground = MainWindow.highlightColor;
			EntryImage.Source = new BitmapImage(new Uri("Images/Costs_over.png", UriKind.RelativeOrAbsolute));

			DeletePanel.Background = Brushes.Transparent;
			DeleteImage.Source = new BitmapImage(new Uri("Images/Delete.png", UriKind.RelativeOrAbsolute));
		}

		private void EntryPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			EditCostWindow editCostWindow = new EditCostWindow(cost);
			editCostWindow.Title = "Kostenpunkt bearbeiten";
			editCostWindow.Owner = Application.Current.MainWindow;
			if (editCostWindow.ShowDialog() == true)
			{
				EntryLabel.Content = cost.Name + "\nBetroffene: " + GetAffectedStr();
			}
		}

		private string GetAffectedStr()
		{
			if (cost.AffectsAllRenters)
				return "Alle Mieter";

			string result = "";
			foreach (Renter renter in cost.AffectedRenters)
			{
				if (result != "")
					result += ", ";
				result += renter.Name;
			}

			if (result == "")
				result = "Keine";
			return result;
		}
	}
}
