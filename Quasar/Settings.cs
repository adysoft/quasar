using System.Collections.Generic;

namespace Quasar
{
    public class Settings
    {
        public Settings()
        {
            Threads = new List<ScreenshotThread>();
        }

        public string TempFolder { get; set; }

        public List<ScreenshotThread> Threads { get; set; }
    }
}
