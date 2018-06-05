using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Quasar.ViewModel;
using Path = System.IO.Path;

namespace Quasar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            Title = $"Quasar {version.ToString()}";
            DataContext = new MainWindowViewModel();
        }

        private void OnScreenshotLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2 && sender is Image image)
            {
                string path = image.Source.ToString();
                Process.Start(path);
            }
        }
    }
}
