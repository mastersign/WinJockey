﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Mastersign.WinJockey.Pages;

namespace Mastersign.WinJockey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private static App CurrentApp => (App)System.Windows.Application.Current;

        public MainWindow()
        {
            InitializeComponent();
            CurrentApp.Dialogs.SetDialogHost(dialogPresenter);

            Loaded += (sender, args) =>
            {
                WatchSystemTheme();
                ApplicationThemeManager.ApplySystemTheme();

                DataContext = CurrentApp.Runtime;

                if (CurrentApp.StartMinimized)
                {
                    WindowState = WindowState.Minimized;
                }

                navigationViewMain.Navigate(typeof(HomePage));
            };

            StateChanged += WindowStateChangedHandler;
        }

        private void WatchSystemTheme()
        {
            SystemThemeWatcher.Watch(
                this,                                  // Window class
                WindowBackdropType.Mica,
                updateAccents: true                    // Whether to change accents automatically
            );
        }

        private void WindowStateChangedHandler(object sender, EventArgs e)
        {
            ShowInTaskbar = WindowState != WindowState.Minimized;
        }

        private void Navigation_Navigated(NavigationView sender, NavigatedEventArgs e)
        {
            gridHeader.Visibility = (e.Page as Page).Name == "home"
                ? Visibility.Collapsed
                : Visibility.Visible;
            labelPageTitle.Text = (e.Page as Page)?.Title;
        }

        private void CommandApplicationCloseExecutedHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void CommandApplicationCloseCanExecuteHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandEnableAutostartCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !CurrentApp.Runtime.Config.Autostart;
        }

        private void CommandEnableAutostartExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            AutostartManager.EnableAutostart();
            CurrentApp.Runtime.Config.Autostart = true;
        }

        private void CommandDisableAutostartCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentApp.Runtime.Config.Autostart;
        }

        private void CommandDisableAutostartExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            AutostartManager.DisableAutostart();
            CurrentApp.Runtime.Config.Autostart = false;
        }

        private void CommandOpenConfigurationDirectoryExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentApp?.Runtime?.Config != null;
        }

        private void CommandOpenConfigurationDirectoryExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentApp.Runtime.Config.OpenInExplorer();
        }

        private void CommandEditSetupExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentApp?.Runtime?.Config != null;
        }

        private void CommandEditSetupExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                CurrentApp.Runtime.Config.EditSetup();
            }
            catch (DefaultEditorNotFoundException exc)
            {
                System.Windows.MessageBox.Show(
                    Properties.Resources.Common.EditorNotFound_Message
                    + Environment.NewLine + Environment.NewLine
                    + exc.EditorExecutable,
                    Properties.Resources.CommandsPage.EditCommand_Title,
                    System.Windows.MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CommandSetupForVsCodeExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentApp?.Runtime?.Config != null;
        }

        private async void CommandSetupForVsCodeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            await CurrentApp.Runtime.Config.SetupVisualStudioCodeSettings();
        }
    }
}
