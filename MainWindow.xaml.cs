using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;

namespace MietRechner
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		/** Name of the recently opened files file */
		private const string recentlyOpenedFileName = "recently.txt";
		/** Color for highlighting */
		public static readonly SolidColorBrush highlightColor = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
		/** Index of the owner page */
		private const int ownerPageIndex = 0;
		/** Index of the house page */
		private const int housePageIndex = 1;
		/** Index of the renter page */
		private const int renterPageIndex = 2;
		/** Index of the costs page */
		private const int costsPageIndex = 3;
		/** Index of the math page */
		private const int mathPageIndex = 4;
		/** The opened billing or null */
		public static Billing billing;
		/** 
		 * The file name that should be used for saving the billing or empty string
		 */
		private string billingFileName;
		/** A timer to update the error and warning messages */
		private DispatcherTimer timer;
		/** List with recently opened files */
		private List<string> recentlyOpenedFiles;

		public MainWindow()
        {
            InitializeComponent();
			billing = null;
			billingFileName = "";
			SaveMenuItem.IsEnabled = false;
			SaveAsMenuItem.IsEnabled = false;
			PrintMenuItem.IsEnabled = false;
			CloseMenuItem.IsEnabled = false;
			LoadRencentlyOpenedFiles();

			timer = new DispatcherTimer();
			timer.Tick += new EventHandler(timer_Tick);
			timer.Interval = new TimeSpan(0, 0, 2);
			timer.Start();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			UpdateWarningsErrors();
		}

		private void LoadRencentlyOpenedFiles()
		{
			recentlyOpenedFiles = new List<string>();
			try
			{
				recentlyOpenedFiles.AddRange(File.ReadAllLines(recentlyOpenedFileName));
				if (recentlyOpenedFiles.Count > 10)
					recentlyOpenedFiles.RemoveRange(10, recentlyOpenedFiles.Count - 10);

				if (recentlyOpenedFiles.Count == 0)
					OpenRecentMenuItem.IsEnabled = false;
				else {
					foreach (string file in recentlyOpenedFiles)
					{
						MenuItem menuItem = new MenuItem();
						menuItem.Header = System.IO.Path.GetFileNameWithoutExtension(file);
						menuItem.Tag = file;
						menuItem.Click += OpenRecentMenuItems_Click;
						OpenRecentMenuItem.Items.Add(menuItem);
					}
				}
			}
			catch
			{
				OpenRecentMenuItem.IsEnabled = false;
			}
		}

		private void AddRecentlyOpenedFile(string fileName)
		{
			int index = recentlyOpenedFiles.IndexOf(fileName);
			if (index == -1)
			{
				recentlyOpenedFiles.Insert(0, fileName);
				if (recentlyOpenedFiles.Count > 10)
					recentlyOpenedFiles.RemoveRange(10, recentlyOpenedFiles.Count - 10);
			}
			else if (index != 0)
			{
				recentlyOpenedFiles.RemoveAt(index);
				recentlyOpenedFiles.Insert(0, fileName);
			}
			
			try
			{
				File.WriteAllLines(recentlyOpenedFileName, recentlyOpenedFiles);
			}
			catch
			{
				// Do nothing
			}

			OpenRecentMenuItem.Items.Clear();
			foreach (string file in recentlyOpenedFiles)
			{
				MenuItem menuItem = new MenuItem();
				menuItem.Header = System.IO.Path.GetFileNameWithoutExtension(file);
				menuItem.Tag = file;
				menuItem.Click += OpenRecentMenuItems_Click;
				OpenRecentMenuItem.Items.Add(menuItem);
			}
			OpenRecentMenuItem.IsEnabled = true;
		}

		private int GetPageIndexFromLeftPanel(DockPanel panel)
		{
			if (panel == OwnerPanel)
				return ownerPageIndex;
			if (panel == HousePanel)
				return housePageIndex;
			if (panel == RenterPanel)
				return renterPageIndex;
			if (panel == CostsPanel)
				return costsPageIndex;
			if (panel == MathPanel)
				return mathPageIndex;
			return -1;
		}

		private DockPanel GetLeftPanelFromPageIndex(int index)
		{
			switch (index)
			{
				case ownerPageIndex:
					return OwnerPanel;
				case housePageIndex:
					return HousePanel;
				case renterPageIndex:
					return RenterPanel;
				case costsPageIndex:
					return CostsPanel;
				case mathPageIndex:
					return MathPanel;
				default:
					return null;
			}
		}

		private string GetImageFromPageIndex(int index, bool over)
		{
			switch (index)
			{
				case ownerPageIndex:
					{
						if (over)
							return "Images/Owner_over.png";
						return "Images/Owner.png";
					}
				case housePageIndex:
					{
						if (over)
							return "Images/House_over.png";
						return "Images/House.png";
					}
				case renterPageIndex:
					{
						if (over)
							return "Images/User_over.png";
						return "Images/User.png";
					}
				case costsPageIndex:
					{
						if (over)
							return "Images/Costs_over.png";
						return "Images/Costs.png";
					}
				case mathPageIndex:
					{
						if (over)
							return "Images/Math_over.png";
						return "Images/Math.png";
					}
				default:
					return "";
			}
		}

		private void SelectPage(int index)
		{
			// Reset style of old selected page
			int oldIndex = Tabs.SelectedIndex;
			DockPanel panel = GetLeftPanelFromPageIndex(oldIndex);
			if (panel != null)
			{
				panel.Background = highlightColor;
				panel.Children.OfType<Label>().FirstOrDefault().Foreground = Brushes.White;

				Image image = panel.Children.OfType<Image>().FirstOrDefault();
				image.Source = new BitmapImage(new Uri(GetImageFromPageIndex(oldIndex, false), UriKind.RelativeOrAbsolute));
			}

			// Set the style of the new selected page
			panel = GetLeftPanelFromPageIndex(index);
			if (panel != null)
			{
				panel.Background = Brushes.White;
				panel.Children.OfType<Label>().FirstOrDefault().Foreground = highlightColor;

				Image image = panel.Children.OfType<Image>().FirstOrDefault();
				image.Source = new BitmapImage(new Uri(GetImageFromPageIndex(index, true), UriKind.RelativeOrAbsolute));

				Tabs.SelectedIndex = index;
			}
		}

		private void OpenRecentMenuItems_Click(object sender, RoutedEventArgs e)
		{
			// Close the old one
			if (billing != null && !CloseBilling())
				return;

            OpenBilling((string) ((MenuItem) sender).Tag);
            ((OwnerPage)((Frame)((TabItem)Tabs.Items[ownerPageIndex]).Content).Content).Page_Loaded(null, null);
            ((HousePage)((Frame)((TabItem)Tabs.Items[housePageIndex]).Content).Content).Page_Loaded(null, null);
            ((MathPage)((Frame)((TabItem)Tabs.Items[mathPageIndex]).Content).Content).Page_Loaded(null, null);
        }

		private void OwnerPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SelectPage(ownerPageIndex);
		}

		private void HousePanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SelectPage(housePageIndex);
		}

		private void RenterPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
			SelectPage(renterPageIndex);
		}

		private void CostsPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SelectPage(costsPageIndex);
		}

		private void MathPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			SelectPage(mathPageIndex);
		}

		private void LeftPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            DockPanel panel = sender as DockPanel;
			int index = GetPageIndexFromLeftPanel(panel);
			if (index == -1 || Tabs.SelectedIndex == index)
				return;
			
			panel.Background = Brushes.White;
            panel.Children.OfType<Label>().FirstOrDefault().Foreground = highlightColor;

            Image image = panel.Children.OfType<Image>().FirstOrDefault();
			image.Source = new BitmapImage(new Uri(GetImageFromPageIndex(index, true), UriKind.RelativeOrAbsolute));
		}

        private void LeftPanel_MouseLeave(object sender, MouseEventArgs e)
        {
			DockPanel panel = sender as DockPanel;
			int index = GetPageIndexFromLeftPanel(panel);
			if (index == -1 || Tabs.SelectedIndex == index)
				return;

			panel.Background = highlightColor;
			panel.Children.OfType<Label>().FirstOrDefault().Foreground = Brushes.White;

			Image image = panel.Children.OfType<Image>().FirstOrDefault();
			image.Source = new BitmapImage(new Uri(GetImageFromPageIndex(index, false), UriKind.RelativeOrAbsolute));
		}

		private void MainForm_Loaded(object sender, RoutedEventArgs e)
		{
			SelectPage(ownerPageIndex);
		}

		private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			AboutWindow aboutWindow = new AboutWindow();
			aboutWindow.Owner = this;
			aboutWindow.ShowDialog();
		}

		/**
		 * Updates the warning and error messages
		 */
		private void UpdateWarningsErrors()
		{
			WarningErrorPanel.Children.Clear();
			Frame frame = null;

			Billing localBilling = billing;
			if (billing == null)
				return;

			foreach (string msg in localBilling.Analyze())
			{
				frame = new Frame();
				frame.Height = Double.NaN;

				if (msg[0] == 'W')
					frame.Navigate(new WarningPage(msg.Substring(1)));
				else if (msg[0] == 'E')
					frame.Navigate(new ErrorPage(msg.Substring(1)));

				WarningErrorPanel.Children.Add(frame);
			}

			if (frame != null)
				frame.Margin = new Thickness(frame.Margin.Left, frame.Margin.Top, frame.Margin.Right, 10);
		}

		private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
		{
			CloseBilling();
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (billing != null && !CloseBilling())
				e.Cancel = true;
		}

		/**
		 * Creates a new billing and shows the owner page
		 * @param billing The new billing
		 */
		private void CreateNewBilling(Billing billing)
		{
			MainWindow.billing = billing;
			Title = "Miet Rechner: Abrechnung " + billing.StartDate.ToShortDateString() + " - " + billing.EndDate.ToShortDateString();
			OuterTabs.SelectedIndex = 0;
			SelectPage(ownerPageIndex);
			((RenterPage) ((Frame) ((TabItem) Tabs.Items[renterPageIndex]).Content).Content).LoadPanels();
			((CostsPage) ((Frame) ((TabItem) Tabs.Items[costsPageIndex]).Content).Content).LoadPanels();

			SaveMenuItem.IsEnabled = true;
			SaveAsMenuItem.IsEnabled = true;
			PrintMenuItem.IsEnabled = true;
			CloseMenuItem.IsEnabled = true;
		}

		/**
		 * Opens a billing and shows the owner page
		 * @param fileName Name of the billing file
		 * @return false if an error occurred
		 */
		private bool OpenBilling(string fileName)
		{
			billing = new Billing();
			if (!billing.LoadFromFile(fileName))
			{
				MessageBox.Show("Die Datei \"" + fileName + "\" konnte nicht geladen werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
				billing = null;
				return false;
			}

			billingFileName = fileName;
			Title = "Miet Rechner: Abrechnung " + billing.StartDate.ToShortDateString() + " - " + billing.EndDate.ToShortDateString();
			OuterTabs.SelectedIndex = 0;
			SelectPage(ownerPageIndex);
			((RenterPage) ((Frame) ((TabItem) Tabs.Items[renterPageIndex]).Content).Content).LoadPanels();
			((CostsPage) ((Frame) ((TabItem) Tabs.Items[costsPageIndex]).Content).Content).LoadPanels();

			SaveMenuItem.IsEnabled = true;
			SaveAsMenuItem.IsEnabled = true;
			PrintMenuItem.IsEnabled = true;
			CloseMenuItem.IsEnabled = true;
			return true;
		}

		/**
		 * Tries to close an open billing
		 * @return true if the billing was closed otherwise false
		 */
		private bool CloseBilling()
		{
			if (billing.Modified)
			{
				// Ask the user if the changes should be stored
				switch (MessageBox.Show("Möchten Sie die letzten Änderungen speichern?", "Speichern?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
				{
					case MessageBoxResult.Yes:
						{
							if (billingFileName == "")
							{
								SaveFileDialog dialog = new SaveFileDialog();
								dialog.Filter = "MietRechner Datei (*.mr) | *.mr";
								if (dialog.ShowDialog() != true)
									return false;

								billingFileName = dialog.FileName;
								if (!billingFileName.EndsWith(".mr"))
									billingFileName += ".mr";
							}

							if (!billing.SaveToFile(billingFileName))
							{
								MessageBox.Show("Die Datei \"" + billingFileName + "\" konnte nicht geschrieben werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
								return false;
							}

							break;
						}
					case MessageBoxResult.No:
						break;
					case MessageBoxResult.Cancel:
						return false;
				}
			}

			billing = null;
			billingFileName = "";
			Title = "Miet Rechner";
			OuterTabs.SelectedIndex = 1; 
			((RenterPage) ((Frame) ((TabItem) Tabs.Items[renterPageIndex]).Content).Content).LoadPanels();
			((CostsPage) ((Frame) ((TabItem) Tabs.Items[costsPageIndex]).Content).Content).LoadPanels();

			SaveMenuItem.IsEnabled = false;
			SaveAsMenuItem.IsEnabled = false;
			PrintMenuItem.IsEnabled = false;
			CloseMenuItem.IsEnabled = false;

			return true;
		}

		private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			// Close the old one
			if (billing != null && !CloseBilling())
				return;

			NewWindow newWindow = new NewWindow(recentlyOpenedFiles);
			newWindow.Owner = this;
			if (newWindow.ShowDialog() == true)
			{
				CreateNewBilling(newWindow.billing);
			}
		}

		private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			// Close the old one
			if (billing != null && !CloseBilling())
				return;

			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "MietRechner Datei (*.mr) | *.mr";
			if (dialog.ShowDialog() == true)
			{
				if (OpenBilling(dialog.FileName))
					AddRecentlyOpenedFile(dialog.FileName);
			}
		}

		private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!SaveMenuItem.IsEnabled)
				return;

			if (billingFileName == "")
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "MietRechner Datei (*.mr) | *.mr";
				if (dialog.ShowDialog() != true)
					return;

				billingFileName = dialog.FileName;
				if (!billingFileName.EndsWith(".mr"))
					billingFileName += ".mr";
			}

			if (!billing.SaveToFile(billingFileName))
			{
				MessageBox.Show("Die Datei \"" + billingFileName + "\" konnte nicht geschrieben werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			AddRecentlyOpenedFile(billingFileName);
		}

		private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!SaveAsMenuItem.IsEnabled)
				return;

			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "MietRechner Datei (*.mr) | *.mr";
			dialog.FileName = billingFileName;
			if (dialog.ShowDialog() != true)
				return;

			billingFileName = dialog.FileName;
			if (!billingFileName.EndsWith(".mr"))
				billingFileName += ".mr";

			if (!billing.SaveToFile(billingFileName))
			{
				MessageBox.Show("Die Datei \"" + billingFileName + "\" konnte nicht geschrieben werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			AddRecentlyOpenedFile(billingFileName);
		}

		private void PrintCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!PrintMenuItem.IsEnabled)
				return;

			// Check if there are errors
			foreach (string msg in billing.Analyze())
			{
				if (msg[0] == 'E')
				{
					MessageBox.Show("Die Abrechnung kann nicht gedruckt werden, da Fehler gefunden wurden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
			}

			PrintWindow printWindow = new PrintWindow(billing);
			printWindow.Owner = this;
			printWindow.ShowDialog();
		}
	}
}
