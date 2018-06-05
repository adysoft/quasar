using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quasar
{
    public class ScreenshotThread
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string RootDirectory { get; set; }
        public string CacheDirectory => Path.Combine(RootDirectory, "cache");
    }
}
