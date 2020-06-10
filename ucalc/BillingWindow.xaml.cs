using System;
using System.Windows;
using System.Windows.Navigation;
using UCalc.Data;
using UCalc.Pages;

namespace UCalc
{
    public partial class BillingWindow
    {
        private readonly Billing _savedBilling;
        public Billing Billing { get; }

        public BillingWindow(Billing savedBilling, Billing billing)
        {
            _savedBilling = savedBilling;
            Billing = billing;
            InitializeComponent();

            Title =
                $"MietRechner - Abrechnung von {billing.StartDate.ToShortDateString()} - {billing.EndDate.Date.ToShortDateString()}";
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Application.Current.MainWindow?.Show();
        }

        private void OnSideBarFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var sideBar = (SideBar) SideBarFrame.Content;
            sideBar.TabControl = TabControl;
            sideBar.LandlordButton.Selected = true;
        }
    }
}