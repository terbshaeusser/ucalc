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
        public Model Model { get; }

        public BillingWindow(Billing billing)
        {
            Model = new Model(billing);

            InitializeComponent();

            Title =
                $"MietRechner - Abrechnung von {billing.StartDate.ToString(Constants.DateFormat)} - {billing.EndDate.Date.ToString(Constants.DateFormat)}";
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
    }
}