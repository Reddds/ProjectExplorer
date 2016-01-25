using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ProjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for ScreenShotWindow.xaml
    /// </summary>
    public partial class ScreenShotWindow : Window
    {
        public ScreenShotWindow(string imagePath)
        {
            InitializeComponent();

            IFullScreenShot.Source = new BitmapImage(new Uri(imagePath));
        }
    }
}
