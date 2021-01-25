using System;
using System.Collections.Generic;
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

namespace MietRechner
{
	/// <summary>
	/// Interaktionslogik für WarningPage.xaml
	/// </summary>
	public partial class WarningPage: Page
	{
		public WarningPage(string message)
		{
			InitializeComponent();
			MessageLabel.Text = message;
		}
	}
}
