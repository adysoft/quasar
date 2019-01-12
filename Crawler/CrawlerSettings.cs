namespace Crawler
{
    public class CrawlerSettings
    {
        public CrawlerSettings(string name, string url, string rootDirectory, string cacheDirectory, string tempRootDirectory)
        {
            Name = name;
            Url = url;
            RootDirectory = rootDirectory;
            CacheDirectory = cacheDirectory;
            TempRootDirectory = tempRootDirectory;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string RootDirectory { get; }
        public string CacheDirectory { get; }
        public string TempRootDirectory { get; }
    }
}
