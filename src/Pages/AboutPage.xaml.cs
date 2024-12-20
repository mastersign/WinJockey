using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace Mastersign.WinJockey.Pages
{
    /// <summary>
    /// Interaktionslogik für PageAbout.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();

            InitializeProductInfo();
        }

        private void InitializeProductInfo()
        {
            var a = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(a.Location);
            tbProductName.Text = fvi.ProductName;
            tbPublisher.Text = fvi.CompanyName;
            tbVersion.Text = fvi.ProductVersion;
            tbWebsite.Text = "https://www.mastersign.de";
            lnkWebsite.NavigateUri = "https://www.mastersign.de";
        }
    }
}
