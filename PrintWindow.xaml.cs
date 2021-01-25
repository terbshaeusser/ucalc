using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheArtOfDev.HtmlRenderer.WPF;

namespace MietRechner
{
	/// <summary>
	/// Interaktionslogik für PrintWindow.xaml
	/// </summary>
	public partial class PrintWindow: Window
	{
		/** Sizes of Din A4 in cm */
		private const float dinA4Width = 21.0f;
		private const float dinA4Height = 29.7f;
		private const float borderWidth = 1.5f;

		private bool ignoreSelectedDateChanged;
		private Billing billing;
		public ObservableCollection<CheckedListItem<Renter>> Renters { get; set; }
		
		public PrintWindow(Billing billing)
		{
			ignoreSelectedDateChanged = false;
			this.billing = billing;
			Renters = new ObservableCollection<CheckedListItem<Renter>>();
			foreach (Renter renter in billing.Renters)
			{
				Renters.Add(new CheckedListItem<Renter>(renter));
			}

			InitializeComponent();
			DataContext = this;
		}

		private void AllRentersCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			if (RentersListbox != null)
				RentersListbox.IsEnabled = false;

			if (CurrentCombobox != null)
			{
				CurrentCombobox.Items.Clear();
				foreach (Renter renter in billing.Renters)
				{
					ComboBoxItem item = new ComboBoxItem();
					item.Content = renter.Name;
					item.Tag = renter;
					CurrentCombobox.Items.Add(item);
				}
				CurrentCombobox.SelectedIndex = 0;
			}
		}

		private void AllRentersCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			if (RentersListbox != null)
				RentersListbox.IsEnabled = true;
			
			CurrentCombobox.Items.Clear();
			foreach (CheckedListItem<Renter> listItem in Renters)
			{
				if (!listItem.IsChecked)
					continue;

				Renter renter = listItem.Item;
				ComboBoxItem item = new ComboBoxItem();
				item.Content = renter.Name;
				item.Tag = renter;
				CurrentCombobox.Items.Add(item);
			}
			if (CurrentCombobox.Items.Count == 0)
				CurrentCombobox.SelectedIndex = -1;
			else
				CurrentCombobox.SelectedIndex = 0;
		}

		private void PrintButton_Click(object sender, RoutedEventArgs e)
		{
			ignoreSelectedDateChanged = true;
			if (CreationDatePicker.SelectedDate == null)
				CreationDatePicker.SelectedDate = DateTime.Now;
			ignoreSelectedDateChanged = false;

			PrintDialog dialog = new PrintDialog();
			if (dialog.ShowDialog() == true)
			{
				float dpi = 0;
				List<Bitmap> pages = new List<Bitmap>();
				if (AllRentersCheckbox.IsChecked == true)
				{
					foreach (Renter renter in billing.Renters)
					{
						List<Bitmap> renterPages = PrintForRenter(renter, dinA4Width - borderWidth * 2, dinA4Height - borderWidth * 2, out dpi);
						if (renterPages == null)
						{
							MessageBox.Show("Die Templatedatei konnte nicht geladen werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}
						pages.AddRange(renterPages);
					}
				}
				else
				{
					foreach (CheckedListItem<Renter> item in Renters)
					{
						if (!item.IsChecked)
							continue;

						List<Bitmap> renterPages = PrintForRenter(item.Item, dinA4Width - borderWidth * 2, dinA4Height - borderWidth * 2, out dpi);
						if (renterPages == null)
						{
							MessageBox.Show("Die Templatedatei konnte nicht geladen werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}
						pages.AddRange(renterPages);
					}
				}

				FixedDocument document = new FixedDocument();
				document.DocumentPaginator.PageSize = new System.Windows.Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
				
				foreach (Bitmap page in pages)
				{
					FixedPage fixedPage = new FixedPage();
					fixedPage.Width = document.DocumentPaginator.PageSize.Width;
					fixedPage.Height = document.DocumentPaginator.PageSize.Height;

					MemoryStream stream = new MemoryStream();
					page.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
					stream.Position = 0;
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.BeginInit();
					bitmapImage.StreamSource = stream;
					bitmapImage.EndInit();
					
					System.Windows.Controls.Image image = new System.Windows.Controls.Image();
					image.Margin = new Thickness(cmToPixels(borderWidth, dpi), cmToPixels(borderWidth, dpi), cmToPixels(borderWidth, dpi), cmToPixels(borderWidth, dpi));
					fixedPage.Children.Add(image);
					image.Source = bitmapImage;

					PageContent pageContent = new PageContent();
					pageContent.Child = fixedPage;
					document.Pages.Add(pageContent);
				}
				
				dialog.PrintDocument(document.DocumentPaginator, "Mietrechner Abrechnung vom " + billing.StartDate.ToShortDateString() + " zum " + billing.EndDate.ToShortDateString());
			}
		}

		private int cmToPixels(float cm, float dpi)
		{
			return Convert.ToInt32(cm / 2.54 * dpi);
		}

		private float GetHtmlDPI()
		{
			PngBitmapEncoder encoder = new PngBitmapEncoder();
			encoder.Frames.Add(HtmlRender.RenderToImage("some test"));
			MemoryStream stream = new MemoryStream();
			encoder.Save(stream);

			Bitmap bitmap = new Bitmap(stream);
			Graphics g = Graphics.FromImage(bitmap);
			return g.DpiX;
		}

		private string LoadTemplateForRenter(Renter renter)
		{
			string result = "";

			// Check if the renter must pay money or gets a refund
			List<KeyValuePair<string, decimal>> costOverview = new List<KeyValuePair<string, decimal>>();
			decimal sum = billing.CalculateForRenter(renter, costOverview, out string details);
			string refundStr;
			if (sum >= 0)
				refundStr = "pay";
			else
				refundStr = "refund";
			string salutationStr = renter.Salutation_.ToString().ToLower();

			string fileName = "Templates/" + renter.Name + "." + refundStr + ".template";
			if (File.Exists(fileName))
			{
				try
				{
					foreach (string line in File.ReadAllLines(fileName))
					{
						if (!line.StartsWith("#"))
							result += line + "\n";
					}
					if (result != "")
						return result;
				}
				catch
				{
					// Do nothing
				}
			}

			fileName = "Templates/" + salutationStr + "." + refundStr + ".template";
			if (File.Exists(fileName))
			{
				try
				{
					foreach (string line in File.ReadAllLines(fileName))
					{
						if (!line.StartsWith("#"))
							result += line + "\n";
					}
					if (result != "")
						return result;
				}
				catch
				{
					// Do nothing
				}
			}

			fileName = "Templates/" + refundStr + ".template";
			if (File.Exists(fileName))
			{
				try
				{
					foreach (string line in File.ReadAllLines(fileName))
					{
						if (!line.StartsWith("#"))
							result += line + "\n";
					}
					if (result != "")
						return result;
				}
				catch
				{
					// Do nothing
				}
			}

			return "";
		}

		private string CreateCostTable(List<KeyValuePair<string, decimal>> costOverview, decimal alreadyPaid, decimal sum)
		{
			string result = "<table class=\"costtable-style\">";

			foreach (KeyValuePair<string, decimal> cost in costOverview)
			{
				result += "<tr><td>" + cost.Key + "</td><td class=\"costtable-price-style\">" + Billing.CeilToString(cost.Value, Billing.PrintPrecision) + " €</td></tr>";
			}
			result += "<tr class=\"costtable-tempsum-style\"><td>Zwischensumme</td><td class=\"costtable-price-style\">" + Billing.CeilToString(sum + alreadyPaid, Billing.PrintPrecision) + " €</td></tr>";
			result += "<tr class=\"costtable-paid-style\"><td>Bereits gezahlt</td><td class=\"costtable-price-style\">" + Billing.CeilToString(alreadyPaid, Billing.PrintPrecision) + " €</td></tr>";
			if (sum < 0)
				result += "<tr class=\"costtable-sum-style\"><td>Einmalige Rückzahlung</td><td class=\"costtable-price-style\">" + Billing.CeilToString(sum, Billing.PrintPrecision) + " €</td></tr>";
			else
				result += "<tr class=\"costtable-sum-style\"><td>Einmalige Nachzahlung</td><td class=\"costtable-price-style\">" + Billing.CeilToString(sum, Billing.PrintPrecision) + " €</td></tr>";

			return result + "</table>";
		}

		private List<Bitmap> PrintForRenter(Renter renter, float widthInCm, float heightInCm, out float dpi)
		{
			string data = LoadTemplateForRenter(renter);
			if (data == "")
			{
				dpi = 0;
				return null;
			}

			List<KeyValuePair<string, decimal>> costOverview = new List<KeyValuePair<string, decimal>>();
			decimal sum = billing.CalculateForRenter(renter, costOverview, out string details);
			decimal absSum = sum;
			if (absSum < 0)
				absSum *= -1;

			string costTableStr = CreateCostTable(costOverview, renter.PaidRent, sum);
			data = data.Replace("{$account-bic}", renter.Account.BIC);
			data = data.Replace("{$account-iban}", renter.Account.IBAN);
			data = data.Replace("{$account-bank}", renter.Account.BankName);
			data = data.Replace("{$address-city}", billing.Address.City);
			data = data.Replace("{$address-number}", billing.Address.HouseNumber);
			data = data.Replace("{$address-plz}", billing.Address.PLZ.ToString());
			data = data.Replace("{$address-street}", billing.Address.Street);
			data = data.Replace("{$costtable}", costTableStr);
			data = data.Replace("{$date}", ((DateTime) CreationDatePicker.SelectedDate).ToShortDateString());
            data = data.Replace("{$details}", details.Replace("\n", "<br>"));
            data = data.Replace("{$enddate}", billing.EndDate.ToShortDateString());
			data = data.Replace("{$message1}", renter.Message1.Replace("\n", "<br>"));
			data = data.Replace("{$message2}", renter.Message2.Replace("\n", "<br>"));
			data = data.Replace("{$name}", renter.Name);
			data = data.Replace("{$owner-account-bic}", billing.Owner.Account.BIC);
			data = data.Replace("{$owner-account-iban}", billing.Owner.Account.IBAN);
			data = data.Replace("{$owner-account-bank}", billing.Owner.Account.BankName);
			data = data.Replace("{$owner-address-city}", billing.Owner.Address.City);
			data = data.Replace("{$owner-address-number}", billing.Owner.Address.HouseNumber);
			data = data.Replace("{$owner-address-plz}", billing.Owner.Address.PLZ.ToString());
			data = data.Replace("{$owner-address-street}", billing.Owner.Address.Street);
            data = data.Replace("{$owner-mail}", billing.Owner.Mail);
            data = data.Replace("{$owner-name}", billing.Owner.Name);
			data = data.Replace("{$owner-phone}", billing.Owner.Phone);
            data = data.Replace("{$owner-salutation}", billing.Owner.Salutation_.AsString());
			data = data.Replace("{$salutation}", renter.Salutation_.AsString());
			data = data.Replace("{$sum}", Billing.CeilToString(absSum, Billing.PrintPrecision));
			data = data.Replace("{$startdate}", billing.StartDate.ToShortDateString());

			// Get the DPI value that is used to print html
			dpi = GetHtmlDPI();
			int width = cmToPixels(widthInCm, dpi);
			int height = cmToPixels(heightInCm, dpi);

			List<Bitmap> list = new List<Bitmap>();
			do
			{
				int index = data.IndexOf("<newpage>");
				string pageData;
				if (index == -1)
				{
					pageData = data;
					data = "";
				}
				else
				{
					pageData = data.Substring(0, index);
					data = data.Substring(index + "<newpage>".Length);
				}

                PngBitmapEncoder encoder = new PngBitmapEncoder();
				encoder.Frames.Add(HtmlRender.RenderToImage(pageData, new System.Windows.Size(width, height)));
				MemoryStream stream = new MemoryStream();
				encoder.Save(stream);
				stream.Position = 0;
				list.Add(new Bitmap(stream));
            } while (data != "");

			return list;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ignoreSelectedDateChanged = true;
			CreationDatePicker.SelectedDate = DateTime.Now;
			ignoreSelectedDateChanged = false;
			AllRentersCheckbox_Checked(null, null);
		}

		private void CurrentCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (CurrentCombobox.SelectedIndex == -1)
			{
				PageCombobox.Items.Clear();
				return;
			}
			ignoreSelectedDateChanged = true;
			if (CreationDatePicker.SelectedDate == null)
				CreationDatePicker.SelectedDate = DateTime.Now;
			ignoreSelectedDateChanged = false;

			Renter renter = (Renter) ((ComboBoxItem) CurrentCombobox.SelectedItem).Tag;
			float dpi;
			List<Bitmap> pages = PrintForRenter(renter, dinA4Width - borderWidth * 2, dinA4Height - borderWidth * 2, out dpi);

			PageCombobox.Items.Clear();
			if (pages == null)
			{
				MessageBox.Show("Die Templatedatei konnte nicht geladen werden!", "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
				PrintButton.IsEnabled = false;
				return;
			}
			else
				PrintButton.IsEnabled = true;

			for (int i = 1; i <= pages.Count; ++i)
			{
				ComboBoxItem item = new ComboBoxItem();
				item.Content = "Seite " + i.ToString();
				PageCombobox.Items.Add(item);
			}
			PageCombobox.SelectedIndex = 0;
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			AllRentersCheckbox_Unchecked(null, null);
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			AllRentersCheckbox_Unchecked(null, null);
		}

		private void PageCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (PageCombobox.SelectedIndex == -1 || CurrentCombobox.SelectedIndex == -1)
			{
				PreviewImage.Source = null;
				return;
			}
			ignoreSelectedDateChanged = true;
			if (CreationDatePicker.SelectedDate == null)
				CreationDatePicker.SelectedDate = DateTime.Now;
			ignoreSelectedDateChanged = false;

			Renter renter = (Renter) ((ComboBoxItem) CurrentCombobox.SelectedItem).Tag;
			float dpi;
			List<Bitmap> pages = PrintForRenter(renter, dinA4Width - borderWidth * 2, dinA4Height - borderWidth * 2, out dpi);
			if (pages == null)
			{
				PreviewImage.Source = null;
				return;
			}

			int width = cmToPixels(dinA4Width, dpi);
			int height = cmToPixels(dinA4Height, dpi);

			Bitmap background = new Bitmap(width, height);
			background.SetResolution(dpi, dpi);
			Graphics g = Graphics.FromImage(background);
			g.FillRectangle(System.Drawing.Brushes.White, new Rectangle(0, 0, background.Width, background.Height));
			g.DrawImageUnscaled(pages[PageCombobox.SelectedIndex], cmToPixels(borderWidth, dpi), cmToPixels(borderWidth, dpi));

			MemoryStream stream = new MemoryStream();
			background.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
			PreviewImage.Source = (ImageSource) new ImageSourceConverter().ConvertFrom(stream);
		}

		private void CreationDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!ignoreSelectedDateChanged)
				CurrentCombobox_SelectionChanged(CurrentCombobox, null);
		}
	}
}
