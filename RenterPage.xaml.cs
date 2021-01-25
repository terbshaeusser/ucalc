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
	/// Interaktionslogik für RenterPage.xaml
	/// </summary>
	public partial class RenterPage : Page
	{
		public RenterPage()
		{
			InitializeComponent();
		}

		private void AddPanel_MouseEnter(object sender, MouseEventArgs e)
		{
			AddPanel.Background = MainWindow.highlightColor;
			AddLabel.Foreground = Brushes.White;
			AddImage.Source = new BitmapImage(new Uri("Images/Add_over.png", UriKind.RelativeOrAbsolute));
		}

		private void AddPanel_MouseLeave(object sender, MouseEventArgs e)
		{
			AddPanel.Background = Brushes.White;
			AddLabel.Foreground = MainWindow.highlightColor;
			AddImage.Source = new BitmapImage(new Uri("Images/Add.png", UriKind.RelativeOrAbsolute));
		}

		private void AddPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Renter renter = new Renter();
			EditRenterWindow editRenterWindow = new EditRenterWindow(renter);
			editRenterWindow.Title = "Mieter hinzufügen";
			editRenterWindow.Owner = Application.Current.MainWindow;
			if (editRenterWindow.ShowDialog() == true)
			{
				MainWindow.billing.Renters.Add(renter);
				AddRenterPanel(renter);
			}
		}

		private void AddRenterPanel(Renter renter)
		{
			Frame frame = new Frame();
			frame.Height = 52;
			frame.Navigate(new RenterEntryPage(this, renter));
			RentersPanel.Children.Add(frame);
		}

		public void LoadPanels()
		{
			RentersPanel.Children.Clear();
			if (MainWindow.billing != null)
			{
				foreach (Renter renter in MainWindow.billing.Renters)
				{
					AddRenterPanel(renter);
				}
			}
		}
	}
}
