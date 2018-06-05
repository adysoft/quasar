using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    public class CrawlerSettings
    {
        public CrawlerSettings(string name, string url, string rootDirectory, string cacheDirectory)
        {
            Name = name;
            Url = url;
            RootDirectory = rootDirectory;
            CacheDirectory = cacheDirectory;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string RootDirectory { get; }
        public string CacheDirectory { get; }
    }
}
