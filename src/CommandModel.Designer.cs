using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Mastersign.WinJockey
{
    #region Scaleton Model Designer generated code
    
    // Scaleton Version: 0.3.0
    
    public enum ActionType
    {
        None,
        URL,
        Shell,
        Exec,
        SwitchVirtualDesktop,
        PinWindow,
        UnpinWindow,
        MaximizeWindow,
        NormalWindow,
        MinimizeWindow,
        CloseWindow,
        MoveWindow,
        LockSession,
        StandbySystem,
        HibernateSystem,
        ShutdownSystem,
        RebootSystem,
        WakeOnLan,
    }
    
    public partial class WinJockeyConfiguration : INotifyPropertyChanged
    {
        public WinJockeyConfiguration()
        {
            this._commands = new BindingList<CommandConfiguration>();
            this.Initialize();
        }
        
        #region Change Tracking
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
        
        #region Property Path
        
        private string _path;
        
        public event EventHandler PathChanged;
        
        protected virtual void OnPathChanged()
        {
            EventHandler handler = PathChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Path");
        }
        
        public virtual string Path
        {
            get { return _path; }
            set
            {
                if (string.Equals(value, _path))
                {
                    return;
                }
                _path = value;
                this.OnPathChanged();
            }
        }
        
        #endregion
        
        #region Property Autostart
        
        private bool _autostart;
        
        public event EventHandler AutostartChanged;
        
        protected virtual void OnAutostartChanged()
        {
            EventHandler handler = AutostartChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Autostart");
        }
        
        public virtual bool Autostart
        {
            get { return _autostart; }
            set
            {
                if ((value == _autostart))
                {
                    return;
                }
                _autostart = value;
                this.OnAutostartChanged();
            }
        }
        
        #endregion
        
        #region Property Setup
        
        private WinJockeySetup _setup;
        
        public event EventHandler SetupChanged;
        
        protected virtual void OnSetupChanged()
        {
            EventHandler handler = SetupChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Setup");
        }
        
        public virtual WinJockeySetup Setup
        {
            get { return _setup; }
            set
            {
                if ((value == _setup))
                {
                    return;
                }
                _setup = value;
                this.OnSetupChanged();
            }
        }
        
        #endregion
        
        #region Property Commands
        
        private BindingList<CommandConfiguration> _commands;
        
        public event EventHandler CommandsChanged;
        
        protected virtual void OnCommandsChanged()
        {
            EventHandler handler = CommandsChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Commands");
        }
        
        public virtual BindingList<CommandConfiguration> Commands
        {
            get { return _commands; }
            set
            {
                if ((value == _commands))
                {
                    return;
                }
                _commands = value;
                this.OnCommandsChanged();
            }
        }
        
        #endregion
    }
    
    public partial class WinJockeySetup : INotifyPropertyChanged
    {
        public WinJockeySetup()
        {
        }
        
        #region Change Tracking
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
        
        #region Property MqttServer
        
        private MqttSetup _mqttServer;
        
        public event EventHandler MqttServerChanged;
        
        protected virtual void OnMqttServerChanged()
        {
            EventHandler handler = MqttServerChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"MqttServer");
        }
        
        public virtual MqttSetup MqttServer
        {
            get { return _mqttServer; }
            set
            {
                if ((value == _mqttServer))
                {
                    return;
                }
                _mqttServer = value;
                this.OnMqttServerChanged();
            }
        }
        
        #endregion
        
        #region Property DefaultBrowser
        
        private ProcessStart _defaultBrowser;
        
        public event EventHandler DefaultBrowserChanged;
        
        protected virtual void OnDefaultBrowserChanged()
        {
            EventHandler handler = DefaultBrowserChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"DefaultBrowser");
        }
        
        public virtual ProcessStart DefaultBrowser
        {
            get { return _defaultBrowser; }
            set
            {
                if ((value == _defaultBrowser))
                {
                    return;
                }
                _defaultBrowser = value;
                this.OnDefaultBrowserChanged();
            }
        }
        
        #endregion
        
        #region Property Browsers
        
        private global::System.Collections.Generic.Dictionary<System.String,ProcessStart> _browsers;
        
        public event EventHandler BrowsersChanged;
        
        protected virtual void OnBrowsersChanged()
        {
            EventHandler handler = BrowsersChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Browsers");
        }
        
        public virtual global::System.Collections.Generic.Dictionary<System.String,ProcessStart> Browsers
        {
            get { return _browsers; }
            set
            {
                if ((value == _browsers))
                {
                    return;
                }
                _browsers = value;
                this.OnBrowsersChanged();
            }
        }
        
        #endregion
        
        #region Property DefaultEditor
        
        private ProcessStart _defaultEditor;
        
        public event EventHandler DefaultEditorChanged;
        
        protected virtual void OnDefaultEditorChanged()
        {
            EventHandler handler = DefaultEditorChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"DefaultEditor");
        }
        
        public virtual ProcessStart DefaultEditor
        {
            get { return _defaultEditor; }
            set
            {
                if ((value == _defaultEditor))
                {
                    return;
                }
                _defaultEditor = value;
                this.OnDefaultEditorChanged();
            }
        }
        
        #endregion
    }
    
    public partial class MqttSetup : INotifyPropertyChanged
    {
        public MqttSetup()
        {
            this._port = DEF_PORT;
        }
        
        #region Change Tracking
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
        
        #region Property Host
        
        private string _host;
        
        public event EventHandler HostChanged;
        
        protected virtual void OnHostChanged()
        {
            EventHandler handler = HostChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Host");
        }
        
        public virtual string Host
        {
            get { return _host; }
            set
            {
                if (string.Equals(value, _host))
                {
                    return;
                }
                _host = value;
                this.OnHostChanged();
            }
        }
        
        #endregion
        
        #region Property Port
        
        private ushort _port;
        
        public event EventHandler PortChanged;
        
        protected virtual void OnPortChanged()
        {
            EventHandler handler = PortChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Port");
        }
        
        private const ushort DEF_PORT = (ushort)1883;
        
        [DefaultValue(DEF_PORT)]
        public virtual ushort Port
        {
            get { return _port; }
            set
            {
                if ((value == _port))
                {
                    return;
                }
                _port = value;
                this.OnPortChanged();
            }
        }
        
        #endregion
        
        #region Property BaseTopic
        
        private string _baseTopic;
        
        public event EventHandler BaseTopicChanged;
        
        protected virtual void OnBaseTopicChanged()
        {
            EventHandler handler = BaseTopicChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"BaseTopic");
        }
        
        public virtual string BaseTopic
        {
            get { return _baseTopic; }
            set
            {
                if (string.Equals(value, _baseTopic))
                {
                    return;
                }
                _baseTopic = value;
                this.OnBaseTopicChanged();
            }
        }
        
        #endregion
    }
    
    public partial class ProcessStart : INotifyPropertyChanged
    {
        public ProcessStart()
        {
        }
        
        #region Change Tracking
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
        
        #region Property Exe
        
        private string _exe;
        
        public event EventHandler ExeChanged;
        
        protected virtual void OnExeChanged()
        {
            EventHandler handler = ExeChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Exe");
        }
        
        public virtual string Exe
        {
            get { return _exe; }
            set
            {
                if (string.Equals(value, _exe))
                {
                    return;
                }
                _exe = value;
                this.OnExeChanged();
            }
        }
        
        #endregion
        
        #region Property Args
        
        private string _args;
        
        public event EventHandler ArgsChanged;
        
        protected virtual void OnArgsChanged()
        {
            EventHandler handler = ArgsChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Args");
        }
        
        public virtual string Args
        {
            get { return _args; }
            set
            {
                if (string.Equals(value, _args))
                {
                    return;
                }
                _args = value;
                this.OnArgsChanged();
            }
        }
        
        #endregion
    }
    
    public partial class CommandConfiguration : INotifyPropertyChanged
    {
        public CommandConfiguration()
        {
        }
        
        #region Change Tracking
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
        
        #region Property MqttTopic
        
        private string _mqttTopic;
        
        public event EventHandler MqttTopicChanged;
        
        protected virtual void OnMqttTopicChanged()
        {
            EventHandler handler = MqttTopicChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"MqttTopic");
        }
        
        public virtual string MqttTopic
        {
            get { return _mqttTopic; }
            set
            {
                if (string.Equals(value, _mqttTopic))
                {
                    return;
                }
                _mqttTopic = value;
                this.OnMqttTopicChanged();
            }
        }
        
        #endregion
        
        #region Property Action
        
        private ActionType _action;
        
        public event EventHandler ActionChanged;
        
        protected virtual void OnActionChanged()
        {
            EventHandler handler = ActionChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Action");
        }
        
        public virtual ActionType Action
        {
            get { return _action; }
            set
            {
                if ((value == _action))
                {
                    return;
                }
                _action = value;
                this.OnActionChanged();
            }
        }
        
        #endregion
        
        #region Property Url
        
        private string _url;
        
        public event EventHandler UrlChanged;
        
        protected virtual void OnUrlChanged()
        {
            EventHandler handler = UrlChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Url");
        }
        
        public virtual string Url
        {
            get { return _url; }
            set
            {
                if (string.Equals(value, _url))
                {
                    return;
                }
                _url = value;
                this.OnUrlChanged();
            }
        }
        
        #endregion
        
        #region Property Browser
        
        private string _browser;
        
        public event EventHandler BrowserChanged;
        
        protected virtual void OnBrowserChanged()
        {
            EventHandler handler = BrowserChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Browser");
        }
        
        public virtual string Browser
        {
            get { return _browser; }
            set
            {
                if (string.Equals(value, _browser))
                {
                    return;
                }
                _browser = value;
                this.OnBrowserChanged();
            }
        }
        
        #endregion
        
        #region Property Cmd
        
        private string _cmd;
        
        public event EventHandler CmdChanged;
        
        protected virtual void OnCmdChanged()
        {
            EventHandler handler = CmdChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Cmd");
        }
        
        public virtual string Cmd
        {
            get { return _cmd; }
            set
            {
                if (string.Equals(value, _cmd))
                {
                    return;
                }
                _cmd = value;
                this.OnCmdChanged();
            }
        }
        
        #endregion
        
        #region Property CmdArgs
        
        private string _cmdArgs;
        
        public event EventHandler CmdArgsChanged;
        
        protected virtual void OnCmdArgsChanged()
        {
            EventHandler handler = CmdArgsChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"CmdArgs");
        }
        
        public virtual string CmdArgs
        {
            get { return _cmdArgs; }
            set
            {
                if (string.Equals(value, _cmdArgs))
                {
                    return;
                }
                _cmdArgs = value;
                this.OnCmdArgsChanged();
            }
        }
        
        #endregion
        
        #region Property WorkingDir
        
        private string _workingDir;
        
        public event EventHandler WorkingDirChanged;
        
        protected virtual void OnWorkingDirChanged()
        {
            EventHandler handler = WorkingDirChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"WorkingDir");
        }
        
        public virtual string WorkingDir
        {
            get { return _workingDir; }
            set
            {
                if (string.Equals(value, _workingDir))
                {
                    return;
                }
                _workingDir = value;
                this.OnWorkingDirChanged();
            }
        }
        
        #endregion
        
        #region Property WindowStyle
        
        private global::System.Diagnostics.ProcessWindowStyle? _windowStyle;
        
        public event EventHandler WindowStyleChanged;
        
        protected virtual void OnWindowStyleChanged()
        {
            EventHandler handler = WindowStyleChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"WindowStyle");
        }
        
        public virtual global::System.Diagnostics.ProcessWindowStyle? WindowStyle
        {
            get { return _windowStyle; }
            set
            {
                if ((value == _windowStyle))
                {
                    return;
                }
                _windowStyle = value;
                this.OnWindowStyleChanged();
            }
        }
        
        #endregion
        
        #region Property Zone
        
        private string _zone;
        
        public event EventHandler ZoneChanged;
        
        protected virtual void OnZoneChanged()
        {
            EventHandler handler = ZoneChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Zone");
        }
        
        public virtual string Zone
        {
            get { return _zone; }
            set
            {
                if (string.Equals(value, _zone))
                {
                    return;
                }
                _zone = value;
                this.OnZoneChanged();
            }
        }
        
        #endregion
        
        #region Property ZonePatch
        
        private string _zonePatch;
        
        public event EventHandler ZonePatchChanged;
        
        protected virtual void OnZonePatchChanged()
        {
            EventHandler handler = ZonePatchChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"ZonePatch");
        }
        
        public virtual string ZonePatch
        {
            get { return _zonePatch; }
            set
            {
                if (string.Equals(value, _zonePatch))
                {
                    return;
                }
                _zonePatch = value;
                this.OnZonePatchChanged();
            }
        }
        
        #endregion
        
        #region Property Screen
        
        private int? _screen;
        
        public event EventHandler ScreenChanged;
        
        protected virtual void OnScreenChanged()
        {
            EventHandler handler = ScreenChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Screen");
        }
        
        public virtual int? Screen
        {
            get { return _screen; }
            set
            {
                if ((value == _screen))
                {
                    return;
                }
                _screen = value;
                this.OnScreenChanged();
            }
        }
        
        #endregion
        
        #region Property WindowPosition
        
        private WindowBounds _windowPosition;
        
        public event EventHandler WindowPositionChanged;
        
        protected virtual void OnWindowPositionChanged()
        {
            EventHandler handler = WindowPositionChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"WindowPosition");
        }
        
        public virtual WindowBounds WindowPosition
        {
            get { return _windowPosition; }
            set
            {
                if ((value == _windowPosition))
                {
                    return;
                }
                _windowPosition = value;
                this.OnWindowPositionChanged();
            }
        }
        
        #endregion
        
        #region Property VirtualDesktop
        
        private int? _virtualDesktop;
        
        public event EventHandler VirtualDesktopChanged;
        
        protected virtual void OnVirtualDesktopChanged()
        {
            EventHandler handler = VirtualDesktopChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"VirtualDesktop");
        }
        
        public virtual int? VirtualDesktop
        {
            get { return _virtualDesktop; }
            set
            {
                if ((value == _virtualDesktop))
                {
                    return;
                }
                _virtualDesktop = value;
                this.OnVirtualDesktopChanged();
            }
        }
        
        #endregion
        
        #region Property ShutdownMessage
        
        private string _shutdownMessage;
        
        public event EventHandler ShutdownMessageChanged;
        
        protected virtual void OnShutdownMessageChanged()
        {
            EventHandler handler = ShutdownMessageChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"ShutdownMessage");
        }
        
        public virtual string ShutdownMessage
        {
            get { return _shutdownMessage; }
            set
            {
                if (string.Equals(value, _shutdownMessage))
                {
                    return;
                }
                _shutdownMessage = value;
                this.OnShutdownMessageChanged();
            }
        }
        
        #endregion
        
        #region Property ShutdownTimeout
        
        private int? _shutdownTimeout;
        
        public event EventHandler ShutdownTimeoutChanged;
        
        protected virtual void OnShutdownTimeoutChanged()
        {
            EventHandler handler = ShutdownTimeoutChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"ShutdownTimeout");
        }
        
        public virtual int? ShutdownTimeout
        {
            get { return _shutdownTimeout; }
            set
            {
                if ((value == _shutdownTimeout))
                {
                    return;
                }
                _shutdownTimeout = value;
                this.OnShutdownTimeoutChanged();
            }
        }
        
        #endregion
        
        #region Property ShutdownForce
        
        private bool _shutdownForce;
        
        public event EventHandler ShutdownForceChanged;
        
        protected virtual void OnShutdownForceChanged()
        {
            EventHandler handler = ShutdownForceChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"ShutdownForce");
        }
        
        public virtual bool ShutdownForce
        {
            get { return _shutdownForce; }
            set
            {
                if ((value == _shutdownForce))
                {
                    return;
                }
                _shutdownForce = value;
                this.OnShutdownForceChanged();
            }
        }
        
        #endregion
        
        #region Property Mac
        
        private string _mac;
        
        public event EventHandler MacChanged;
        
        protected virtual void OnMacChanged()
        {
            EventHandler handler = MacChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Mac");
        }
        
        public virtual string Mac
        {
            get { return _mac; }
            set
            {
                if (string.Equals(value, _mac))
                {
                    return;
                }
                _mac = value;
                this.OnMacChanged();
            }
        }
        
        #endregion
        
        #region Property Subnet
        
        private string _subnet;
        
        public event EventHandler SubnetChanged;
        
        protected virtual void OnSubnetChanged()
        {
            EventHandler handler = SubnetChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Subnet");
        }
        
        public virtual string Subnet
        {
            get { return _subnet; }
            set
            {
                if (string.Equals(value, _subnet))
                {
                    return;
                }
                _subnet = value;
                this.OnSubnetChanged();
            }
        }
        
        #endregion
    }
    
    public partial class WindowBounds
    {
        public WindowBounds()
        {
        }
        
        #region Property X
        
        private double _x;
        
        public virtual double X
        {
            get { return _x; }
            set
            {
                if ((Math.Abs(value - _x) < double.Epsilon))
                {
                    return;
                }
                _x = value;
            }
        }
        
        #endregion
        
        #region Property Y
        
        private double _y;
        
        public virtual double Y
        {
            get { return _y; }
            set
            {
                if ((Math.Abs(value - _y) < double.Epsilon))
                {
                    return;
                }
                _y = value;
            }
        }
        
        #endregion
        
        #region Property W
        
        private double _w;
        
        public virtual double W
        {
            get { return _w; }
            set
            {
                if ((Math.Abs(value - _w) < double.Epsilon))
                {
                    return;
                }
                _w = value;
            }
        }
        
        #endregion
        
        #region Property H
        
        private double _h;
        
        public virtual double H
        {
            get { return _h; }
            set
            {
                if ((Math.Abs(value - _h) < double.Epsilon))
                {
                    return;
                }
                _h = value;
            }
        }
        
        #endregion
    }
    
    public partial class WinJockeyRuntime : INotifyPropertyChanged
    {
        public WinJockeyRuntime()
        {
            this._debugMessages = new BindingList<string>();
            this.Initialize();
        }
        
        #region Change Tracking
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        
        #endregion
        
        #region Property DebugMessages
        
        private BindingList<string> _debugMessages;
        
        public event EventHandler DebugMessagesChanged;
        
        protected virtual void OnDebugMessagesChanged()
        {
            EventHandler handler = DebugMessagesChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"DebugMessages");
        }
        
        public virtual BindingList<string> DebugMessages
        {
            get { return _debugMessages; }
            set
            {
                if ((value == _debugMessages))
                {
                    return;
                }
                _debugMessages = value;
                this.OnDebugMessagesChanged();
            }
        }
        
        #endregion
        
        #region Property Actions
        
        private Actions _actions;
        
        public event EventHandler ActionsChanged;
        
        protected virtual void OnActionsChanged()
        {
            EventHandler handler = ActionsChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Actions");
        }
        
        public virtual Actions Actions
        {
            get { return _actions; }
            set
            {
                if ((value == _actions))
                {
                    return;
                }
                _actions = value;
                this.OnActionsChanged();
            }
        }
        
        #endregion
        
        #region Property MqttConnection
        
        private MqttConnection _mqttConnection;
        
        public event EventHandler MqttConnectionChanged;
        
        protected virtual void OnMqttConnectionChanged()
        {
            EventHandler handler = MqttConnectionChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"MqttConnection");
        }
        
        public virtual MqttConnection MqttConnection
        {
            get { return _mqttConnection; }
            set
            {
                if ((value == _mqttConnection))
                {
                    return;
                }
                _mqttConnection = value;
                this.OnMqttConnectionChanged();
            }
        }
        
        #endregion
        
        #region Property Config
        
        private WinJockeyConfiguration _config;
        
        public event EventHandler ConfigChanged;
        
        protected virtual void OnConfigChanged()
        {
            EventHandler handler = ConfigChanged;
            if (!ReferenceEquals(handler, null))
            {
                handler(this, EventArgs.Empty);
            }
            this.OnPropertyChanged(@"Config");
        }
        
        public virtual WinJockeyConfiguration Config
        {
            get { return _config; }
            set
            {
                if ((value == _config))
                {
                    return;
                }
                _config = value;
                this.OnConfigChanged();
            }
        }
        
        #endregion
    }
    
    #endregion
}