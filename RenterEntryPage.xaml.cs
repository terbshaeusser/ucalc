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
	/// Interaktionslogik für RenterEntryPage.xaml
	/// </summary>
	public partial class RenterEntryPage : Page
	{
		/** The parent page */
		private RenterPage page;
		/** The connected renter */
		private Renter renter;

		public RenterEntryPage(RenterPage page, Renter renter)
		{
			InitializeComponent();
			this.page = page;
			this.renter = renter;
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			EntryLabel.Content = renter.Name;
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
			if (MessageBox.Show("Möchten Sie den Mieter \"" + renter.Name + "\" wirklich entfernen?", "Mieter entfernen?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				MainWindow.billing.Renters.Remove(renter);
				foreach (Cost cost in MainWindow.billing.Costs)
				{
					cost.AffectedRenters.Remove(renter);
				}

				foreach (Frame frame in page.RentersPanel.Children.OfType<Frame>())
				{
					if (frame.Content == this)
					{
						page.RentersPanel.Children.Remove(frame);
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
			EntryImage.Source = new BitmapImage(new Uri("Images/User.png", UriKind.RelativeOrAbsolute));

			DeletePanel.Background = Brushes.DarkRed;
			DeleteImage.Source = new BitmapImage(new Uri("Images/Delete_over.png", UriKind.RelativeOrAbsolute));
		}

		private void EntryPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			EntryPanel.Background = Brushes.Transparent;
			EntryLabel.Foreground = MainWindow.highlightColor;
			EntryImage.Source = new BitmapImage(new Uri("Images/User_over.png", UriKind.RelativeOrAbsolute));

			DeletePanel.Background = Brushes.Transparent;
			DeleteImage.Source = new BitmapImage(new Uri("Images/Delete.png", UriKind.RelativeOrAbsolute));
		}

		private void EntryPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			EditRenterWindow editRenterWindow = new EditRenterWindow(renter);
			editRenterWindow.Title = "Mieter bearbeiten";
			editRenterWindow.Owner = Application.Current.MainWindow;
			if (editRenterWindow.ShowDialog() == true)
			{
				EntryLabel.Content = renter.Name;
			}
		}
	}
}
