using System;
using System.Windows.Forms;

namespace Mastersign.WinJockey
{
    /// <summary>
    /// This window exists to steal the focus of the currently used application,
    /// before executing shell commands.
    /// As a result, the application run by the shell command
    /// is opened in foreground with focus.
    /// 
    /// This works even if the user recently typed in an input box of another application.
    /// Then Windows usually prevents an application started by another
    /// application in the background from stealing the focus.
    /// </summary>
    public partial class FocusWindow : Form
    {
        private Action action;

        public FocusWindow(Action action)
        {
            this.action = action;
            InitializeComponent();
        }

        public static void ExecuteWithFocus(IntPtr ownerHWnd, Action action)
        {
            var fw = new FocusWindow(action);
            fw.StartPosition = FormStartPosition.Manual;
            fw.Left = -1;
            fw.Top = -1;
            fw.Show(new WindowHandle(ownerHWnd));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            WinMan.WinApi.SetForegroundWindow(Handle);
            WinMan.WinApi.BringWindowToTop(Handle);
            WinMan.WinApi.SetCapture(Handle);
            action?.Invoke();
            action = null; // prevent accidently executing action twice
            Close();
        }

        private class WindowHandle : IWin32Window
        {
            public WindowHandle(IntPtr handle)
            {
                Handle = handle;
            }

            public IntPtr Handle { get; }
        }
    }
}
