using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.ViewModel
{
    public class ScreenshotViewModel : BaseViewModel
    {
        public ScreenshotViewModel()
        {
            OpenThreadCommand = new RelayCommand(OnOpenThreadCommand);
        }

        private void OnOpenThreadCommand(object obj)
        {
            Process.Start(ThreadPost);
        }

        public RelayCommand OpenThreadCommand { get; set; }
        public string LocalPath { get; set; }
        public string TempPath { get; set; }
        public string ThreadPost { get; set; }
        public string ThumbnailPath { get; set; }
    }
}
