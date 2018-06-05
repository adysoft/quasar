using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Crawler
{
    public abstract class BaseCrawler
    {
        protected BaseCrawler(CrawlerSettings settings)
        {
            StartPage = settings.Url;
            Settings = settings;
            if (!Directory.Exists(Settings.RootDirectory))
            {
                Directory.CreateDirectory(Settings.RootDirectory);
            }

            if (!Directory.Exists(Settings.CacheDirectory))
            {
                Directory.CreateDirectory(Settings.CacheDirectory);
            }
        }

        protected string StartPage { get; private set; }
        protected CrawlerSettings Settings { get; }
        protected abstract string GetPageUrlAtIndex(int index);

        public delegate void ThreadDownloadProgressEventHandler(string pageUrl, double progress);
        public delegate void DownloadingScreenshotEventHandler(string screenshotUrl);
        public delegate void OnErrorEventHandler(Exception exception);
        public delegate void ThreadSyncedEventHandler(string threadName);
        public delegate void ScreenshotDownloadedEventHandler(Screenshot screenshot);

        public event ThreadDownloadProgressEventHandler ThreadDownloadProgressEvent;
        public event DownloadingScreenshotEventHandler DownloadingScreenshotEvent;
        public event ThreadSyncedEventHandler ThreadSyncedEvent;
        public event OnErrorEventHandler OnErrorEvent;
        public event ScreenshotDownloadedEventHandler ScreenshotDownloaded;

        public List<Screenshot> SyncScreenshots()
        {
            List<Screenshot> synced = new List<Screenshot>();
            int pageCount = GetNumberOfPagesOnline();
            int startPage = Math.Max(GetNumberOfPagesOffline(), 1);
            int pagesToProcessed = pageCount - startPage + 1;
            int processedPage = 0;
            for (int i = Math.Max(startPage, 1); i <= pageCount; i++)
            {
                try
                {
                    string page = GetPageUrlAtIndex(i);
                    if (ThreadDownloadProgressEvent != null)
                    {
                        double progress = (processedPage++) / (double) pagesToProcessed;
                        ThreadDownloadProgressEvent(page, progress * 100);
                    }

                    var pageDirectory = Path.Combine(Settings.RootDirectory, $"page-{i}");
                    var screenshotUrls = GetScreenshotUrls(page);
                    synced = DownloadScreenshots(screenshotUrls.Item2, pageDirectory, page, screenshotUrls.Item1);
                }
                catch (Exception e)
                {
                    OnErrorEvent?.Invoke(new Exception($"Error when processing page ${i}", e));
                    break;
                }
                finally
                {
                    ThreadSyncedEvent?.Invoke(Settings.Name);
                }
            }

            return synced;
        }

        protected abstract int GetNumberOfPagesOnline();

        protected abstract string GetScreenshotAnchorUrl(string page, string source, string screenshotUrl);

        protected int GetNumberOfPagesOffline()
        {
            return Directory.GetDirectories(Settings.RootDirectory, "page-*").Length;
        }

        protected List<Screenshot> DownloadScreenshots(ReadOnlyCollection<string> screenshotUrls, string targetDirectory, string page, string pageSource)
        {
            var screenshots = new List<Screenshot>();
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var cached = new HashSet<string>(Directory.GetFiles(Settings.RootDirectory, "*.*", SearchOption.AllDirectories).Select(Path.GetFileNameWithoutExtension));
            foreach (string screenshotUrl in screenshotUrls)
            {
                try
                {
                    string hashCode = $"{screenshotUrl.GetHashCode():X}";
                    string ext = Path.GetExtension(screenshotUrl);
                    string fileName = hashCode + ext;
                    string cachedFile = Path.Combine(Settings.CacheDirectory, fileName);
                    if (!cached.Contains(hashCode))
                    {
                        DownloadingScreenshotEvent?.Invoke(screenshotUrl);
                        WebClient webClient = new WebClient();
                        webClient.Headers.Add("user-agent", "Quasar");
                        var data = webClient.DownloadData(screenshotUrl);
                        if (String.IsNullOrEmpty(ext) || ext.Length > 4)
                        {
                            if (IsJpeg(data))
                            {
                                ext = ".jpg";
                                fileName = hashCode + ext;
                                cachedFile = Path.Combine(Settings.CacheDirectory, fileName);
                            }
                            else if (IsPng(data))
                            {
                                ext = ".png";
                                fileName = hashCode + ext;
                                cachedFile = Path.Combine(Settings.CacheDirectory, fileName);
                            }
                            else
                            {
                                OnErrorEvent?.Invoke(new Exception($"Unknown file format: {screenshotUrl}"));
                            }
                        }

                        File.WriteAllBytes(cachedFile, data);
                        GetImageSize(cachedFile, out var width, out var height);
                        if (height * width > 1027 * 768)
                        {
                            string targetFile = Path.Combine(targetDirectory, fileName);
                            if (!File.Exists(targetFile))
                            {
                                File.Copy(cachedFile, targetFile);
                                File.Delete(cachedFile);
                                var screenshot = new Screenshot { LocalPath = targetFile, Url = screenshotUrl, ThreadPost = GetScreenshotAnchorUrl(page, pageSource, screenshotUrl) };
                                screenshots.Add(screenshot);
                                ScreenshotDownloaded?.Invoke(screenshot);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    OnErrorEvent?.Invoke(new Exception($"Failed to download {screenshotUrl}", e));
                }
            }

            return screenshots;
        }

        private void GetImageSize(string file, out int width, out int height)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(file);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            width = bitmap.PixelWidth;
            height = bitmap.PixelHeight;
        }

        protected Tuple<string, ReadOnlyCollection<string>> GetScreenshotUrls(string pageUrl)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Quasar");
            string src = webClient.DownloadString(pageUrl);
            string regEx = "img src\\s*=\\s*\"(.+?)\"";
            Regex imgSrcRegEx = new Regex(regEx);
            var matches = imgSrcRegEx.Matches(src);
            var screenshotUrls = new List<string>();
            foreach (Match match in matches)
            {
                if (match.Value.Contains("http"))
                {
                    string s = match.Value.Substring(match.Value.IndexOf("http", StringComparison.Ordinal));
                    s = s.Replace("\"", String.Empty);
                    if (!screenshotUrls.Contains(s))
                    {
                        screenshotUrls.Add(s);
                    }
                }
            }

            return new Tuple<string, ReadOnlyCollection<string>>(src, screenshotUrls.AsReadOnly());
        }

        private bool IsJpeg(byte[] data)
        {
            if (data.Length < 10)
            {
                return false;
            }

            using (var ms = new MemoryStream(data))
            {
                byte[] buffer = new byte[4];
                ms.Seek(6, SeekOrigin.Begin);
                ms.Read(buffer, 0, 4);
                return Encoding.ASCII.GetString(buffer) == "JFIF";
            }
        }

        private bool IsPng(byte[] data)
        {
            if (data.Length < 10)
            {
                return false;
            }

            using (var ms = new MemoryStream(data))
            {
                byte[] buffer = new byte[4];
                ms.Read(buffer, 0, 4);
                byte[] pngHeader = new[] {(byte) 137, (byte) 'P', (byte) 'N', (byte) 'G',};
                return Encoding.ASCII.GetString(buffer) == Encoding.ASCII.GetString(pngHeader);
            }
        }
    }
}
