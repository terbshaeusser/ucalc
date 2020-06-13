using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UCalc.Controls;
using UCalc.Models;

namespace UCalc.Pages
{
    public partial class SideBar
    {
        public TabControl TabControl { get; set; }
        public Model Model { get; set; }

        public SideBar()
        {
            InitializeComponent();
        }

        private void OnTabButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var button in ((Panel) ((HighlightButton) sender).Parent).FindChildren<HighlightButton>())
            {
                if (!ReferenceEquals(sender, button))
                {
                    button.Selected = false;
                }
            }

            var buttons = new List<HighlightButton>
                {LandlordButton, HouseButton, TenantsButton, CostsButton, DetailsButton};
            for (var i = 0; i < buttons.Count; ++i)
            {
                if (ReferenceEquals(sender, buttons[i]))
                {
                    TabControl.SelectedIndex = i;
                    break;
                }
            }
        }
    }
}