using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Win32;
using UCalc.Data;

namespace UCalc
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnNewClick(object sender, RoutedEventArgs e)
        {
            var newWindow = new NewWindow {Owner = this};

            if (newWindow.ShowDialog() == true)
            {
                new BillingWindow(newWindow.SavedBilling, newWindow.Billing).Show();
                Hide();
            }
        }

        private void OnOpenClick(object sender, MouseButtonEventArgs e)
        {
            if (App.RecentlyOpenedList.Count == 0)
            {
                OnOpenFromDiskClick(null, null);
            }
            else
            {
                var button = (Button) sender;
                var contextMenu = button.ContextMenu;
                // ReSharper disable once PossibleNullReferenceException
                contextMenu.PlacementTarget = button;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            }

            e.Handled = true;
        }

        private void OnOpenFromDiskClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog {Filter = "MietRechner Datei (*.mr) | *.mr"};

            if (dialog.ShowDialog() == true)
            {
                OpenBilling(dialog.FileName);
            }

            if (e != null)
            {
                e.Handled = true;
            }
        }

        private void OnOpenRecentClick(object sender, RoutedEventArgs e)
        {
            var recentlyOpenedItem = (RecentlyOpenedItem) ((MenuItem) sender).DataContext;

            OpenBilling(recentlyOpenedItem.Path);
        }

        private void OpenBilling(string path)
        {
            try
            {
                var billing = new BillingLoader().Load(path);
                App.RecentlyOpenedList.Add(new RecentlyOpenedItem(path));

                new BillingWindow(billing, billing.Clone()).Show();
                Hide();
            }
            catch (IOException)
            {
                throw new NotImplementedException();
            }
        }
    }
}