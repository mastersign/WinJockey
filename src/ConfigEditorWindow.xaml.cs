using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using UI = Wpf.Ui.Controls;

namespace Mastersign.WinJockey;

public partial class ConfigEditorWindow : UI.FluentWindow
{
    public static void ShowAsDialog(string title, string filename, string schemaName)
    {
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
    }

    private async void WindowClosingHandler(object sender, CancelEventArgs e)
    {
        var text = await editor.GetTextAsync();
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        File.WriteAllText(filename, text, encoding);
    }
}
