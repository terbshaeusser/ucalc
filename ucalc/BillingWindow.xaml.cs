using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;
using UCalc.Data;
using UCalc.Models;
using UCalc.Pages;

namespace UCalc
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _execute;

        public DelegateCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

    public partial class BillingWindow
    {
        public string FilePath { get; set; }
        public Model Model { get; }

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand PrintCommand { get; }

        public BillingWindow(string filePath, Billing billing)
        {
            FilePath = filePath;
            Model = new Model(billing);
            SaveCommand = new DelegateCommand(parameter => { Save(); });
            SaveAsCommand = new DelegateCommand(parameter => { Save(true); });
            PrintCommand = new DelegateCommand(parameter => { Print(); });

            InitializeComponent();

            Title =
                $"MietRechner - Abrechnung von {billing.StartDate.ToString(Constants.DateFormat)} - {billing.EndDate.Date.ToString(Constants.DateFormat)}";
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!Model.Root.Modified)
            {
                return;
            }

            while (true)
            {
                switch (MessageBox.Show("Möchten Sie die letzten Änderungen speichern?", "Speichern?",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        if (Save())
                        {
                            return;
                        }

                        break;
                    case MessageBoxResult.No:
                        return;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Application.Current.MainWindow?.Show();
        }

        private void OnSideBarFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var sideBar = (SideBar) ((Frame) sender).Content;
            sideBar.ParentWindow = this;
            sideBar.Model = Model;
            sideBar.LandlordButton.Selected = true;
        }

        private void OnLandlordFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (LandlordPage) ((Frame) sender).Content;
            page.Landlord = Model.Root.Landlord;
        }

        private void OnHouseFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (HousePage) ((Frame) sender).Content;
            page.House = Model.Root.House;
            page.ParentWindow = this;
        }

        private void OnTenantsFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (TenantsPage) ((Frame) sender).Content;
            page.Model = Model;
            page.Tenants = Model.Root.Tenants;
            page.House = Model.Root.House;
            page.ParentWindow = this;
        }

        private void OnCostsFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (CostsPage) ((Frame) sender).Content;
            page.Model = Model;
            page.Costs = Model.Root.Costs;
            page.House = Model.Root.House;
            page.ParentWindow = this;
        }

        private void OnDetailsFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (DetailsPage) ((Frame) sender).Content;
            page.Model = Model;
        }

        public bool Save(bool rename = false)
        {
            if (Model.Root.Errors.Count > 0)
            {
                MessageBox.Show(
                    "Fehlerhafte Einträge (wie z.B. ungültige Beträge) gehen beim Speichern und anschließenden Laden verloren.",
                    "Warnung!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            while (true)
            {
                if (string.IsNullOrEmpty(FilePath) || rename)
                {
                    var dialog = new SaveFileDialog
                        {Filter = "MietRechner Datei (*.mr) | *.mr", FileName = FilePath ?? ""};
                    if (dialog.ShowDialog() != true)
                    {
                        return false;
                    }

                    FilePath = dialog.FileName;
                    if (!FilePath.EndsWith(".mr"))
                    {
                        FilePath += ".mr";
                    }
                }

                var billing = Model.Dump();
                try
                {
                    BillingLoader.Store(FilePath, billing);
                    Model.ResetModified();
                    return true;
                }
                catch (IOException)
                {
                    if (MessageBox.Show(
                        $"Die Daten konnten nicht in \"{FilePath}\" gespeichert werden.\nMöchten Sie die Daten an einem anderen Ort speichern?",
                        "Fehler!", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No)
                    {
                        return false;
                    }

                    rename = true;
                }
            }
        }

        public bool Print()
        {
            if (Model.Root.Errors.Count > 0)
            {
                MessageBox.Show(
                    "Bitte beheben Sie zuerst die angezeigten Fehler, bevor Sie das Dokument drucken können.",
                    "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            throw new NotImplementedException();
        }

        private void OnDetailsTabSelected(object sender, RoutedEventArgs e)
        {
            ((DetailsPage) DetailsFrame.Content).Compute();
        }
    }
}