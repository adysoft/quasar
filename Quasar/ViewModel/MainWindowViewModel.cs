using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using Crawler;

namespace Quasar.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        private double _progress;

        public MainWindowViewModel()
        {
            LoadSettings();
            SyncCommand = new RelayCommand(OnSyncCommand);
            RecentScreenshots = new ObservableCollection<ScreenshotViewModel>();
            Logs = new ObservableCollection<string>();
        }

        private void OnSyncCommand(object obj)
        {
            if (Settings != null)
            {
                foreach (ScreenshotThread screenshotThread in Settings.Threads)
                {
                    if (screenshotThread.Url.ToLower().Contains("resetera"))
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += OnReseteraDoWork;
                        bw.RunWorkerAsync(screenshotThread);
                    }
                    else if(screenshotThread.Url.ToLower().Contains("neogaf"))
                    {
                        BackgroundWorker bw = new BackgroundWorker();
                        bw.DoWork += OnNeogafDoWork;
                        bw.RunWorkerAsync(screenshotThread);
                    }
                }
            }
        }

        private void OnReseteraDoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is ScreenshotThread screenshotThread)
            {
                ReseteraCrawler crawler = new ReseteraCrawler(new CrawlerSettings(screenshotThread.Name, screenshotThread.Url, screenshotThread.RootDirectory, screenshotThread.CacheDirectory));
                crawler.OnErrorEvent += Crawler_OnErrorEvent;
                crawler.ThreadDownloadProgressEvent += Crawler_ThreadDownloadProgress;
                crawler.DownloadingScreenshotEvent += Crawler_DownloadingScreenshotEvent;
                crawler.ScreenshotDownloaded += Crawler_ScreenshotDownloaded;
                crawler.ThreadSyncedEvent += Crawler_ThreadSyncedEvent;
                e.Result = crawler.SyncScreenshots();
            }
        }

        private void OnNeogafDoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is ScreenshotThread screenshotThread)
            {
                NeogafCrawler crawler = new NeogafCrawler(new CrawlerSettings(screenshotThread.Name, screenshotThread.Url, screenshotThread.RootDirectory, screenshotThread.CacheDirectory));
                crawler.OnErrorEvent += Crawler_OnErrorEvent;
                crawler.ThreadDownloadProgressEvent += Crawler_ThreadDownloadProgress;
                crawler.DownloadingScreenshotEvent += Crawler_DownloadingScreenshotEvent;
                crawler.ScreenshotDownloaded += Crawler_ScreenshotDownloaded;
                crawler.ThreadSyncedEvent += Crawler_ThreadSyncedEvent;
                e.Result = crawler.SyncScreenshots();
            }
        }

        private void Crawler_ScreenshotDownloaded(Screenshot screenshot)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (RecentScreenshots.Count < 100)
                {
                    RecentScreenshots.Add(new ScreenshotViewModel {LocalPath = screenshot.LocalPath, ThreadPost = screenshot .ThreadPost});
                }
                else
                {
                    AddToLogs($"WARN: Maximum number of screenshot displayed (100).");
                }
            }));
        }

        private void Crawler_ThreadSyncedEvent(string threadName)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                AddToLogs($"INFO: Screenshots from {threadName} thread are now synced.");
                Progress = 100;
            }));
        }

        private void Crawler_DownloadingScreenshotEvent(string screenshotUrl)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                AddToLogs($"INFO: Downloading {screenshotUrl}");
            }));
        }

        private void Crawler_ThreadDownloadProgress(string pageUrl, double progress)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (!String.IsNullOrEmpty(pageUrl))
                {
                    AddToLogs($"INFO: Processing page {pageUrl}, overrall progress {progress}%");
                }

                Progress = progress;
            }));
        }

        private void Crawler_OnErrorEvent(Exception exception)
        {
            App.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                AddToLogs($"ERROR: {exception.Message}");
            }));
        }

        public ObservableCollection<ScreenshotViewModel> RecentScreenshots { get; set; }
        public ObservableCollection<string> Logs { get; set; }

        public RelayCommand SyncCommand { get; set; }

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            } 
        }

        public Settings Settings { get; set; }

        private void LoadSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            string settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
            using (TextReader reader = new StreamReader(settingsFile))
            {
                Settings = serializer.Deserialize(reader) as Settings;
                if (Settings!= null && (Settings.Threads == null || !Settings.Threads.Any()))
                {
                    MessageBox.Show("No screenshot threads found your settings file.");
                    Application.Current.Shutdown();
                }
            }
        }

        private void AddToLogs(string message)
        {
            Logs.Insert(0, message);
            if (Logs.Count > 1000)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        }
    }
}
