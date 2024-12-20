using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using Wpf.Ui.Controls;

namespace Mastersign.WinJockey.Pages
{
    /// <summary>
    /// Interaktionslogik für SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private WinJockeyRuntime Runtime => DataContext as WinJockeyRuntime;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void CheckBoxAutostartClickedHandler(object sender, RoutedEventArgs e)
        {
            var runtime = Runtime;
            if (runtime == null) return;
            if (runtime.Config.Autostart)
            {
                if (Commands.DisableAutostart.CanExecute(null, CheckBoxAutostart))
                    Commands.DisableAutostart.Execute(this, CheckBoxAutostart);
            }
            else
            {
                if (Commands.EnableAutostart.CanExecute(null, CheckBoxAutostart))
                    Commands.EnableAutostart.Execute(this, CheckBoxAutostart);
            }
        }
    }
}
