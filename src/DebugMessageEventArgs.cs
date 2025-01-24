using System;

namespace Mastersign.WinJockey
{
    [Serializable]
    public class DebugMessageEventArgs : EventArgs
    {
        public string Message { get; }

        public DebugMessageEventArgs(string message)
        {
            Message = message;
        }
    }
}
