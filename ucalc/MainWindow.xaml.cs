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
        private bool _showRecover;

        public MainWindow()
        {
            InitializeComponent();

            _showRecover = App.Autosaver.CanRecover;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            if (_showRecover)
            {
                _showRecover = false;

                switch (MessageBox.Show(
                    "Die Anwendung wurde das letzte Mal nicht richtig beendet. Es existiert allerdings eine Sicherheitskopie. Möchten Sie diese laden?",
                    "Sicherheitskopie laden?",
                    MessageBoxButton.YesNo, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        OpenBilling(App.Autosaver.AutosavePath, true);
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnNewClick(object sender, RoutedEventArgs e)
        {
            var newWindow = new NewWindow {Owner = this};

            if (newWindow.ShowDialog() == true)
            {
                new BillingWindow(null, newWindow.Billing).Show();
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

        private void OpenBilling(string path, bool isRecovery = false)
        {
            try
            {
                var billing = BillingLoader.Load(path);
                if (!isRecovery)
                {
                    App.RecentlyOpenedList.Add(new RecentlyOpenedItem(path));
                }

                new BillingWindow(isRecovery ? null : path, billing).Show();
                Hide();
            }
            catch (IOException e)
            {
                MessageBox.Show($"Beim Laden der Datei \"{path}\" ist ein Fehler aufgetreten!\n\nDetails: {e.Message}",
                    "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}