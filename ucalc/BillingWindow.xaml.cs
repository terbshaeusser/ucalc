using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UCalc.Data;
using UCalc.Models;
using UCalc.Pages;

namespace UCalc
{
    public partial class BillingWindow
    {
        public Billing Billing { get; }
        public BillingModel Model { get; }

        public BillingWindow(Billing savedBilling, Billing billing)
        {
            Billing = billing;
            InitializeComponent();

            Model = new BillingModel(Billing);

            Title =
                $"MietRechner - Abrechnung von {billing.StartDate.ToShortDateString()} - {billing.EndDate.Date.ToShortDateString()}";
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Application.Current.MainWindow?.Show();
        }

        private void OnSideBarFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var sideBar = (SideBar) ((Frame) sender).Content;
            sideBar.TabControl = TabControl;
            sideBar.LandlordButton.Selected = true;

            sideBar.Model = Model;
        }

        private void OnLandlordFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (LandlordPage) ((Frame) sender).Content;
            page.Model = Model.LandlordModel;
        }

        private void OnHouseFrameLoadCompleted(object sender, NavigationEventArgs e)
        {
            var page = (HousePage) ((Frame) sender).Content;
            page.Model = Model.HouseModel;
        }
    }
}