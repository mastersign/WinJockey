using System.Diagnostics;
using System.IO;
using System.Text;

#nullable enable

namespace Mastersign.WinJockey;

internal static class FileHelper
{
    public static string RobustReadAllText(string filename, Encoding encoding, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromMilliseconds(250);
        Exception? ex = null;
        var sw = new Stopwatch();
        sw.Start();
        while (sw.Elapsed < timeout)
        {
            try
            {
                return File.ReadAllText(filename, encoding);
            }
            catch (IOException e)
            {
                ex = e;
                if (e.HResult == -2147024864)
                    Thread.Sleep(10);
                else
                {
                    Debug.WriteLine($"{e.GetType().FullName}: {e.Message} [HRESULT={e.HResult}]");
                    throw;
                }
            }
        }
        sw.Stop();
        if (ex != null) throw ex;
        throw new TimeoutException();
    }
}
