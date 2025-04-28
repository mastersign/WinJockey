using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static System.IO.Path;
using static Mastersign.WinJockey.FileHelper;

namespace Mastersign.WinJockey
{
    partial class WinJockeyConfiguration
    {
        private const string REDIRECT_FILENAME = "redirect.txt";
        private const string SETUP_FILENAME = "setup.yml";
        private const string COMMANDS_DIRNAME = "commands";

        public Dispatcher Dispatcher { get; set; }

        private FileSystemWatcher watcher;

        public event EventHandler ConfigurationChanged;

        private string RedirectFile => Combine(Path, REDIRECT_FILENAME);

        public string RealPath { get; private set; }

        private string SetupFile => Combine(RealPath, SETUP_FILENAME);

        internal string CommandsDir => Combine(RealPath, COMMANDS_DIRNAME);

        private void Initialize()
        {
            PathChanged += PathChangedhandler;
            Path = Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Mastersign", "WinJockey");
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
            var vscodePath = Combine(RealPath, ".vscode");
            if (e.FullPath.StartsWith(vscodePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Debug.WriteLine($"{e.ChangeType} {e.FullPath}");
            Synchronize(e.FullPath);
        }

        public void Synchronize(string path = null)
        {
            if (Dispatcher == null) return;
            Debug.WriteLine("Synchronize!");
            if (RealPath == null || !Directory.Exists(RealPath))
            {
                Setup = new WinJockeySetup();
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
                        && !potentialPath.Contains("\n")
                        && !potentialPath.Contains("\r")
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
            using (var sourceStream = a.GetManifestResourceStream($"{ns}.resources.{resourceName}")
                ?? throw new FileNotFoundException("Embedded resource " + resourceName + " not found."))
            using (var targetStream = File.Open(targetPath, FileMode.Create))
            {
                await sourceStream.CopyToAsync(targetStream);
            }
        }

        private static string ReadTemplate(string name)
        {
            var a = Assembly.GetExecutingAssembly();
            var ns = typeof(App).Namespace;
            using (var stream = a.GetManifestResourceStream(ns + ".templates." + name)
                ?? throw new FileNotFoundException("Embedded file template " + name + " not found."))
            using (var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true))
            {
                return reader.ReadToEnd();
            }
        }

        public void DeployTemplate(string name, string targetFilePath)
        {
            var template = ReadTemplate(name);
            File.WriteAllText(targetFilePath, template, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        public void EditCommandConfiguration(CommandConfiguration command) 
            => OpenInConfigEditor(
                 string.Format(Properties.Resources.Common.EditCommand_Title_1, command.CommandName),
                 command.Source, "command");

        private void AsureConfiguration()
        {
            if (!Directory.Exists(RealPath))
            {
                Directory.CreateDirectory(RealPath);
            }
            if (!File.Exists(SetupFile))
            {
                DeployTemplate("Setup.yml", SetupFile);
            }
            if (!Directory.Exists(CommandsDir))
            {
                Directory.CreateDirectory(CommandsDir);
            }
        }

        public void OpenInExplorer()
        {
            AsureConfiguration();
            Process.Start(new ProcessStartInfo("explorer", '"' + RealPath + '"'));
        }

        public void EditSetup()
        {
            AsureConfiguration();
            OpenInConfigEditor(
                Properties.Resources.Common.EditConfiguration_Title, 
                Combine(SetupFile), "setup");
        }

        public void OpenInConfigEditor(string title, string filePath, string schema)
        {
            var editorCmd = Setup.Editor?.Exe;
            var args = Setup.Editor?.Args;
            if (!string.IsNullOrWhiteSpace(editorCmd) && File.Exists(editorCmd))
            {
                editorCmd = StringExpander.ExpandWinJockeyConfigLocation(editorCmd, RealPath);
                editorCmd = Environment.ExpandEnvironmentVariables(editorCmd);
                if (!string.IsNullOrWhiteSpace(args))
                {
                    args = StringExpander.ExpandWinJockeyConfigLocation(args, RealPath);
                    args = Environment.ExpandEnvironmentVariables(args);
                    args = args.Replace("$FILE$", filePath);
                }
                else
                {
                    args = '"' + filePath + '"';
                }
                Process.Start(new ProcessStartInfo(editorCmd, args));
            }
            else
            {
                ConfigEditorWindow.ShowAsDialog("WinJockey - " + title, filePath, schema);
            }
        }

        public async Task SetupVisualStudioCodeSettings()
        {
            if (RealPath == null) return;
            var vscodeDir = Combine(RealPath, ".vscode");
            if (!Directory.Exists(vscodeDir)) { Directory.CreateDirectory(vscodeDir); }
            var schemaDir = Combine(vscodeDir, "schemas");
            if (!Directory.Exists(schemaDir)) { Directory.CreateDirectory(schemaDir); }
            var commandSchemaPath = Combine(schemaDir, "command.schema.json");
            var setupSchemaPath = Combine(schemaDir, "setup.schema.json");
            await WriteOutResource("command.schema.json", commandSchemaPath);
            await WriteOutResource("setup.schema.json", setupSchemaPath);
            var settingsFile = Combine(vscodeDir, "settings.json");
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
                    RobustReadAllText(setupFilePath, Encoding.UTF8)) ?? new WinJockeySetup();
            }
            else
            {
                Setup = new WinJockeySetup();
            }
        }

        private void ResetCommands()
        {
            Dispatcher.Invoke(Commands.Clear);
        }

        private void ReadCommands()
        {
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            if (Directory.Exists(CommandsDir))
            {
                var commands = new List<CommandConfiguration>();
                foreach (var commandFile in Directory.GetFiles(CommandsDir, "*.yml"))
                {
                    var command = deserializer.Deserialize<CommandConfiguration>(
                        RobustReadAllText(commandFile, Encoding.UTF8)) ?? new CommandConfiguration();
                    command.Source = commandFile;
                    commands.Add(command);
                }
                commands.Sort((a, b) => a.CommandName.CompareTo(b.CommandName));
                Dispatcher.Invoke(() =>
                {
                    Commands.Clear();
                    foreach (var command in commands) Commands.Add(command);
                });
            }
            else
            {
                ResetCommands();
            }
        }

        private void OnChange() => ConfigurationChanged?.Invoke(this, EventArgs.Empty);
    }
}
