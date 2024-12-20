using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.IO.Path;
using static Mastersign.WinJockey.FileHelper;

namespace Mastersign.WinJockey;

partial class WinJockeyConfiguration
{
    private const string REDIRECT_FILENAME = "redirect.txt";
    private const string SETUP_FILENAME = "setup.yml";
    private const string COMMANDS_DIRNAME = "commands";

    private FileSystemWatcher watcher;

    public event EventHandler ConfigurationChanged;

    private string RedirectFile => Join(Path, REDIRECT_FILENAME);

    public string RealPath { get; private set; }

    private string SetupFile => Join(RealPath, SETUP_FILENAME);

    internal string CommandsDir => Join(RealPath, COMMANDS_DIRNAME);

    private void Initialize()
    {
        PathChanged += PathChangedhandler;
        Path = Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".winjockey");
        Autostart = AutostartManager.IsAutostartEnabled();
    }

    private void PathChangedhandler(object sender, EventArgs e)
    {
        watcher?.Dispose();
        watcher = null;
        DiscoverRealPath();
        if (RealPath != null && Directory.Exists(RealPath))
        {
            Debug.WriteLine("Creating Watcher for: " + RealPath);
            watcher = new FileSystemWatcher(RealPath);
            watcher.Changed += FileSystemChanged;
            watcher.Created += FileSystemChanged;
            watcher.Deleted += FileSystemChanged;
            watcher.Renamed += FileSystemChanged;
            watcher.NotifyFilter = NotifyFilters.CreationTime
                | NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName
                | NotifyFilters.Size;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
        }
        Synchronize();
    }

    private void FileSystemChanged(object sender, FileSystemEventArgs e)
    {
        var vscodePath = Join(RealPath, ".vscode");
        if (e.FullPath.StartsWith(vscodePath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        Debug.WriteLine($"{e.ChangeType} {e.FullPath}");
        Synchronize(e.FullPath);
    }

    public void Synchronize(string path = null)
    {
        Debug.WriteLine("Synchronize!");
        if (RealPath == null || !Directory.Exists(RealPath))
        {
            Setup = new();
            ResetCommands();
            OnChange();
        }
        else
        {
            InitializeSetup();
            ReadSetup();
            ReadCommands();
            OnChange();
        }
    }

    private void DiscoverRealPath()
    {
        if (Path == null)
        {
            RealPath = null;
            return;
        }
        var redirectFile = RedirectFile;
        if (File.Exists(redirectFile))
        {
            try
            {
                var potentialPath = RobustReadAllText(redirectFile, Encoding.UTF8).Trim();
                if (!string.IsNullOrWhiteSpace(potentialPath)
                    && !potentialPath.Contains('\n')
                    && !potentialPath.Contains('\r')
                    && Directory.Exists(potentialPath))
                {
                    RealPath = potentialPath;
                    return;
                }
            }
            catch (Exception) { }
            RealPath = null;
        }
        else
        {
            RealPath = Path;
        }
    }

    private static async Task WriteOutResource(string resourceName, string targetPath)
    {
        var a = Assembly.GetExecutingAssembly();
        var ns = typeof(App).Namespace;
        using var sourceStream = a.GetManifestResourceStream($"{ns}.resources.{resourceName}")
            ?? throw new FileNotFoundException("Embedded resource " + resourceName + " not found.");
        using var targetStream = File.OpenWrite(targetPath);
        await sourceStream.CopyToAsync(targetStream);
    }

    private static string ReadTemplate(string name)
    {
        var a = Assembly.GetExecutingAssembly();
        var ns = typeof(App).Namespace;
        using var stream = a.GetManifestResourceStream(ns + ".templates." + name)
            ?? throw new FileNotFoundException("Embedded file template " + name + " not found.");
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void DeployTemplate(string name, string targetFilePath)
    {
        var template = ReadTemplate(name);
        File.WriteAllText(targetFilePath, template, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    public async Task SetupVisualStudioCodeSettings()
    {
        if (RealPath == null) return;
        var vscodeDir = Join(RealPath, ".vscode");
        if (!Directory.Exists(vscodeDir)) { Directory.CreateDirectory(vscodeDir); }
        var schemaDir = Join(vscodeDir, "schemas");
        if (!Directory.Exists(schemaDir)) { Directory.CreateDirectory(schemaDir); }
        var commandSchemaPath = Join(schemaDir, "command.schema.json");
        await WriteOutResource("command.schema.json", commandSchemaPath);
        var settingsFile = Join(vscodeDir, "settings.json");
        if (!File.Exists(settingsFile)) await WriteOutResource("vscode.settings.json", settingsFile);
    }

    private void InitializeSetup()
    {
        if (!File.Exists(SetupFile))
        {
            DeployTemplate("Setup.yml", SetupFile);
        }
        if (!Directory.Exists(CommandsDir))
        {
            Directory.CreateDirectory(CommandsDir);
        }
    }

    private void ReadSetup()
    {
        var setupFilePath = SetupFile;
        if (File.Exists(setupFilePath))
        {
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            Setup = deserializer.Deserialize<WinJockeySetup>(
                RobustReadAllText(setupFilePath, Encoding.UTF8)) ?? new();
        }
        else
        {
            Setup = new();
        }
    }

    private void ResetCommands()
    {
        Commands.Clear();
    }

    private void ReadCommands()
    {
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        Commands.Clear();
        if (Directory.Exists(CommandsDir))
        {
            foreach (var commandFile in Directory.GetFiles(CommandsDir, "*.yml"))
            {
                var command = deserializer.Deserialize<CommandConfiguration>(
                    RobustReadAllText(commandFile, Encoding.UTF8)) ?? new();
                command.Source = System.IO.Path.GetFileNameWithoutExtension(commandFile);
                Commands.Add(command);
            }
        }
    }

    private void OnChange() => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
}
