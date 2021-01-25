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
	/// Interaktionslogik für CostsPage.xaml
	/// </summary>
	public partial class CostsPage : Page
	{
		public CostsPage()
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
			Cost cost = new Cost(MainWindow.billing.StartDate, MainWindow.billing.EndDate);
			EditCostWindow editCostWindow = new EditCostWindow(cost);
			editCostWindow.Title = "Kostenpunkt hinzufügen";
			editCostWindow.Owner = Application.Current.MainWindow;
			if (editCostWindow.ShowDialog() == true)
			{
				MainWindow.billing.Costs.Add(cost);
				AddCostPanel(cost);
			}
		}

		private void AddCostPanel(Cost cost)
		{
			Frame frame = new Frame();
			frame.Height = 52;
			frame.Navigate(new CostEntryPage(this, cost));
			CostsPanel.Children.Add(frame);
		}

		public void LoadPanels()
		{
			CostsPanel.Children.Clear();
			if (MainWindow.billing != null)
			{
				foreach (Cost cost in MainWindow.billing.Costs)
				{
					AddCostPanel(cost);
				}
			}
		}
	}
}
