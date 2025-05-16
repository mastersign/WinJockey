using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UI = Wpf.Ui.Controls;

namespace Mastersign.WinJockey;

public partial class ConfigEditorWindow : UI.FluentWindow
{
    private static bool IsWebView2Available()
    {
        try
        {
            Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private const string WEBVIEW2_DOWNLOAD_URL = "https://developer.microsoft.com/de-de/microsoft-edge/webview2";

    private static void ShowWebView2NotFoundMessage()
    {
        var msgContent = new StackPanel();
        msgContent.Children.Add(new TextBlock
        {
            Text = Properties.Resources.Common.WebView2_NotFound,
        });
        msgContent.Children.Add(new UI.HyperlinkButton
        {
            Margin = new Thickness(0, 8, 0, 0),
            NavigateUri = WEBVIEW2_DOWNLOAD_URL,
            Content = WEBVIEW2_DOWNLOAD_URL,
        });
        UserInteraction.ShowMessage(
            title: Properties.Resources.Common.WebView2_NotFound_Title,
            message: msgContent,
            symbol: InteractionSymbol.Warning,
            showInTaskbar: true,
            owner: Application.Current.MainWindow);
    }

    public static void ShowAsDialog(string title, string filename, string schemaName)
    {
        if (!IsWebView2Available())
        {
            ShowWebView2NotFoundMessage();
            return;
        }
        var window = new ConfigEditorWindow
        {
            Title = title,
            filename = filename,
            schemaName = schemaName,
        };
        window.ShowDialog();
    }

    private string schemaName;
    private string filename;

    public ConfigEditorWindow()
    {
        InitializeComponent();
        editor.Visibility = Visibility.Hidden;
    }

    private static string GetTextResource(string path)
    {
        var ns = typeof(ConfigEditorWindow).Namespace;
        var resPath = ns + "." + path.Replace('/', '.');
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resPath);
        using var r = new StreamReader(s, Encoding.UTF8);
        return r.ReadToEnd();
    }

    private async void EditorReadyHandler(object sender, EventArgs e)
    {
        await editor.LoadJsonSchemaAsync(
            GetTextResource($"resources/{schemaName}.schema.json"),
            "https://winjockey.mastersign.de/command.schema.json");

        await editor.LoadTextAsync(
            File.ReadAllText(filename, Encoding.UTF8),
            "yaml",
            Path.GetFileName(filename));

        editor.Visibility = Visibility.Visible;
        editor.InvalidateVisual();
    }

    private async void WindowClosingHandler(object sender, CancelEventArgs e)
    {
        var text = await editor.GetTextAsync();
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        File.WriteAllText(filename, text, encoding);
    }
}
