using ImageViewer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

namespace ImageViewer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer autoPlayTimer { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool isSelectedFolder { get; set; }
        private Regex imageNameRegex { get; } = new Regex(@"(.+(jpg|png))");
        private FileInfo[] displayImageInfos { get; set; } = new FileInfo[] { };

        public int counter = 0;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            autoPlayTimer = new DispatcherTimer();

            autoPlayTimer.Tick += new EventHandler(autoPlayTimer_Tick);
            autoPlayTimer.Interval = new TimeSpan(0, 0, Settings.AutoPlaySec);
            autoPlayTimer.Start();

            btnFirstPicture.IsEnabled = false;
            btnPrePicture.IsEnabled = false;
            btnNextPicture.IsEnabled = false;
            btnLastPicture.IsEnabled = false;
        }

        private void autoPlayTimer_Tick(object sender, EventArgs e)
        {
            if (!Settings.IsAutoPlay)
                return;

            if (counter == displayImageInfos.Count())
            {
                if (Settings.IsCyclePlay)
                    counter = 0;
                else
                    return;
            }

            if (this.isSelectedFolder)
            {
                bindImage();
                counter++;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).Show();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            string selectedPath = dialog.SelectedPath;

            if (string.IsNullOrEmpty(selectedPath))
                return;

            txtFolderName.Text = selectedPath;
            ListDirectory(selectedPath);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void tvFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnFirstPicture.IsEnabled = false;

            System.Windows.Controls.TreeView tree = sender as System.Windows.Controls.TreeView;
            TreeViewItem item = tree.SelectedItem as TreeViewItem;

            if (tree.SelectedItem == null)
                return;

            string selectedPath = item.Tag.ToString();
            txtFolderName.Text = selectedPath;

            if (File.Exists(selectedPath))
                this.isSelectedFolder = false;
            else
                this.isSelectedFolder = true;

            ReloadSettings();

            if (this.isSelectedFolder)
            {
                displayImageInfos = new DirectoryInfo(selectedPath).GetFiles("*", SearchOption.TopDirectoryOnly).Where(x => imageNameRegex.IsMatch(x.Name)).ToArray();

                counter = 0;
                bindImage();
            }
            else
            {
                btnFirstPicture.IsEnabled = false;
                btnPrePicture.IsEnabled = false;
                btnNextPicture.IsEnabled = false;
                btnLastPicture.IsEnabled = false;

                if (File.Exists(selectedPath))
                {
                    displayImageInfos = new FileInfo[] { new FileInfo(selectedPath) };
                    counter = 0;
                    bindImage();
                }
            }
        }

        private void btnFirstPicture_Click(object sender, RoutedEventArgs e)
        {
            ReloadSettings();
            counter = 0;
            bindImage();
        }

        private void btnPrePicture_Click(object sender, RoutedEventArgs e)
        {
            ReloadSettings();
            counter--;
            bindImage();
        }

        private void btnNextPicture_Click(object sender, RoutedEventArgs e)
        {
            ReloadSettings();
            counter++;
            bindImage();
        }

        private void btnLastPicture_Click(object sender, RoutedEventArgs e)
        {
            ReloadSettings();
            counter = displayImageInfos.Count() - 1;
            bindImage();
        }

        /// <summary>
        /// Reload Settings
        /// </summary>
        public void ReloadSettings()
        {
            autoPlayTimer.Tick -= new EventHandler(autoPlayTimer_Tick);

            autoPlayTimer = new DispatcherTimer();
            autoPlayTimer.Tick += new EventHandler(autoPlayTimer_Tick);
            autoPlayTimer.Interval = new TimeSpan(0, 0, Settings.AutoPlaySec);
            autoPlayTimer.Start();
        }

        /// <summary>
        /// TreeView Directory
        /// </summary>
        /// <param name="_path"></param>
        private void ListDirectory(string path)
        {
            tvFolders.Items.Clear();
            DirectoryInfo rootDirectoryInfo = new DirectoryInfo(path);
            tvFolders.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
        }

        /// <summary>
        /// Create TreeViewItem
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name, Tag = directoryInfo.FullName, Foreground = Brushes.White };

            foreach (var directory in directoryInfo.GetDirectories().Where(x => !x.Name.Contains("System Volume Information")))
                directoryNode.Items.Add(CreateDirectoryNode(directory));

            foreach (var file in directoryInfo.GetFiles().Where(x => imageNameRegex.IsMatch(x.Name)))
                directoryNode.Items.Add(new TreeViewItem { Header = file.Name, Tag = file.FullName, Foreground = Brushes.White });

            return directoryNode;
        }

        /// <summary>
        /// Set Image Uri
        /// </summary>
        private void bindImage()
        {
            btnFirstPicture.IsEnabled = true;
            btnPrePicture.IsEnabled = true;
            btnNextPicture.IsEnabled = true;
            btnLastPicture.IsEnabled = true;

            if (displayImageInfos.Count() == 0)
            {
                btnFirstPicture.IsEnabled = false;
                btnPrePicture.IsEnabled = false;
                btnNextPicture.IsEnabled = false;
                btnLastPicture.IsEnabled = false;

                imgDisplay.Source = null;

                return;
            }

            if (this.counter > displayImageInfos.Count() - 1)
            {
                this.counter = displayImageInfos.Count() - 1;
                string src = displayImageInfos[this.counter].FullName;
                imgDisplay.Source = new BitmapImage(new Uri(src));
                return;
            }

            string imageUri = displayImageInfos[this.counter].FullName;
            imgDisplay.Source = new BitmapImage(new Uri(imageUri));

            if (this.counter == 0)
            {
                btnFirstPicture.IsEnabled = false;
                btnPrePicture.IsEnabled = false;
            }

            if (this.counter == displayImageInfos.Count() - 1)
            {
                btnNextPicture.IsEnabled = false;
                btnLastPicture.IsEnabled = false;
            }
        }
    }
}
