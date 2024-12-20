using System.Windows.Controls;

namespace Mastersign.WpfTools
{
    public class IntegratedFrame : Frame
    {
        public IntegratedFrame()
        {
            Navigated += (s, e) =>
            {
                if (Content is Page page)
                {
                    page.DataContext = DataContext;
                }
            };
        }
    }
}
