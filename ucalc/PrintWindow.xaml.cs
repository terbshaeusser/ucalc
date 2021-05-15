using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UCalc.Annotations;
using UCalc.Controls;
using UCalc.Data;
using UCalc.Models;

namespace UCalc
{
    public class TenantMenuItem : INotifyPropertyChanged
    {
        public Tenant Tenant { get; }
        public bool None { get; }
        private bool _selected;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value == _selected)
                {
                    return;
                }

                _selected = value;
                OnPropertyChanged();
            }
        }

        public TenantMenuItem(Tenant tenant, bool none)
        {
            Tenant = tenant;
            None = none;
            Selected = tenant != null;
        }

        public string Name => Tenant != null ? Tenant.Name : None ? "Kein Mieter" : "Alle Mieter";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class PrintWindow
    {
        public Billing Billing { get; }
        public IReadOnlyList<TenantMenuItem> TenantMenuItems { get; }
        private PrintableDocument _document;


        public PrintWindow(Model model)
        {
            Billing = model.Dump();
            TenantMenuItems = new[] {new TenantMenuItem(null, false), new TenantMenuItem(null, true)}.Concat(
                Billing.Tenants.Select(tenant => new TenantMenuItem(tenant, false))).ToList();

            foreach (var item in TenantMenuItems)
            {
                item.PropertyChanged += OnSelectedTenantChanged;
            }

            InitializeComponent();

            Preview();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _document?.Dispose();
        }

        private void Preview()
        {
            _document?.Dispose();

            _document = new PrintableDocument(
                Billing,
                TenantMenuItems.Where(item => item.Selected).Select(item => item.Tenant)
            );
            _document.PreviewIn(Viewer);
        }

        private void OnPrintClick(object sender, RoutedEventArgs e)
        {
            _document.Print(
                $"MietRechner Abrechnung {Billing.StartDate.ToString(Constants.DateFormat)} - {Billing.EndDate.ToString(Constants.DateFormat)}"
            );

            /*PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintQueue.AddJob("test", "C:\\test.xps", false);
            }*/
        }

        private void OnSelectedTenantChanged(object sender, PropertyChangedEventArgs e)
        {
            Preview();
        }

        private void OnTenantSelectorClick(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var contextMenu = button.ContextMenu;
            // ReSharper disable once PossibleNullReferenceException
            contextMenu.PlacementTarget = button;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;

            e.Handled = true;
        }

        private void OnTenantMenuItemClick(object sender, RoutedEventArgs e)
        {
            var item = (TenantMenuItem) ((MenuItem) sender).DataContext;

            if (item.Tenant == null)
            {
                foreach (var item2 in TenantMenuItems)
                {
                    if (item2.Tenant != null)
                    {
                        item2.Selected = !item.None;
                    }
                }
            }
            else
            {
                item.Selected = !item.Selected;
            }
        }
    }
}