namespace Quasar
{
    public class JobData
    {
        public JobData(ScreenshotThread screenshotThread, string tempDirectory)
        {
            ScreenshotThread = screenshotThread;
            TempDirectory = tempDirectory;
        }

        public ScreenshotThread ScreenshotThread { get; set; }
        public string TempDirectory { get; set; }
    }
}
