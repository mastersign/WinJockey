using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using UI = Wpf.Ui.Controls;

namespace Mastersign.WinJockey.Pages
{
    /// <summary>
    /// Interaktionslogik für SettingsPage.xaml
    /// </summary>
    public partial class CommandsPage : Page
    {
        private WinJockeyRuntime Runtime => DataContext as WinJockeyRuntime;

        public CommandsPage()
        {
            InitializeComponent();
            TextBoxNewCommandName.Text = string.Empty;
            ValidateNewName(TextBoxNewCommandName.Text);
        }

        private string NewCommandFilename(string name) => System.IO.Path.Combine(Runtime.Config.CommandsDir, name.Trim() + ".yml");

        private void ValidateNewName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName) ||
                System.IO.Path.GetInvalidFileNameChars().Any(newName.Contains))
            {
                ButtonNewCommand.IsEnabled = false;
                return;
            }
            var filename = NewCommandFilename(newName);
            if (File.Exists(filename))
            {
                ButtonNewCommand.IsEnabled = false;
                return;
            }
            ButtonNewCommand.IsEnabled = true;
        }

        private void ButtonNewCommand_Click(object sender, RoutedEventArgs e)
        {
            Runtime.Config.DeployTemplate("Command.yml", NewCommandFilename(TextBoxNewCommandName.Text));
            TextBoxNewCommandName.Text = string.Empty;
            TextBoxNewCommandName.Focus();
        }

        private void TextBoxNewCommandName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateNewName(TextBoxNewCommandName.Text);
        }

        private void ButtonRunCommand_Click(object sender, RoutedEventArgs ea)
        {
            var command = (CommandConfiguration)((FrameworkElement)ea.Source).Tag;
            Runtime.Actions.Trigger(command);
        }

        private void ButtonEditCommand_Click(object sender, RoutedEventArgs ea)
        {
            var command = (CommandConfiguration)((FrameworkElement)ea.Source).Tag;
            try
            {
                Runtime.Config.EditCommandConfiguration(command);
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

        private async void ButtonDeleteCommand_Click(object sender, RoutedEventArgs e)
        {
            var command = (CommandConfiguration)((FrameworkElement)e.Source).Tag;

            var dlg = FindResource("DeleteConfirmationDialog") as UI.ContentDialog;
            var msgTextBlock = dlg.Content as TextBlock;
            msgTextBlock.Text = string.Format(Properties.Resources.CommandsPage.DeleteDialog_Message, command.CommandName);
            var result = await (App.Current as App).Dialogs.ShowAsync(dlg, CancellationToken.None);
            if (result != UI.ContentDialogResult.Primary) return;

            File.Delete(command.Source);
        }

    }
}
