using System;
using System.Windows;

namespace Mastersign.WinJockey
{
    internal static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                App.Main();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "WinJockey", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
            return 0;
        }
    }
}
