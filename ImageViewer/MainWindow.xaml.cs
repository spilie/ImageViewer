using ImageViewer.Model;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageViewer;

/// <summary>
/// MainWindow.xaml 的互動邏輯
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 設定檔
    /// </summary>
    private Settings Settings { get; set; }

    /// <summary>
    /// 自動播放定時器
    /// </summary>
    private DispatcherTimer AutoPlayTimer { get; set; }

    /// <summary>
    /// 建構式
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Settings = Settings.GetInstance();

        AutoPlayTimer = new DispatcherTimer();
        AutoPlayTimer.Tick += new EventHandler(AutoPlayTimer_Tick);
        AutoPlayTimer.Interval = new TimeSpan(0, 0, Settings.AutoPlaySec);
    }

    /// <summary>
    /// 是否選擇資料夾
    /// </summary>
    private bool IsSelectedFolder { get; set; }

    /// <summary>
    /// 圖檔判斷Regex
    /// </summary>
    private Regex ImageNameRegex { get; } = new Regex(@"(.+(jpg|png))");

    /// <summary>
    /// 顯示的檔案清單
    /// </summary>
    private FileInfo[] DisplayImageInfos { get; set; } = [];

    /// <summary>
    /// 圖片計數器
    /// </summary>
    public int Counter { get; set; } = 0;

    /// <summary>
    /// Load事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        AutoPlayTimer.Start();

        BtnFirstPicture.IsEnabled = false;
        BtnPrePicture.IsEnabled = false;
        BtnNextPicture.IsEnabled = false;
        BtnLastPicture.IsEnabled = false;
    }

    /// <summary>
    /// 自動撥放定時器 觸發事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void AutoPlayTimer_Tick(object? sender, EventArgs e)
    {
        //不為自動撥放，不執行動作
        if (!Settings.IsAutoPlay)
        {
            return;
        }

        //如果計數器已經到達最後張，判斷是否循環撥放
        if (Counter == DisplayImageInfos.Length)
        {
            if (Settings.IsCyclePlay)
                Counter = 0;
            else
                return;
        }

        //選擇資料夾時，綁定圖片
        if (IsSelectedFolder)
        {
            BindingImage();
            Counter++;
        }
    }

    /// <summary>
    /// 設定按鈕事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        new SettingsWindow(this).Show();
    }

    /// <summary>
    /// 選擇事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnSelect_Click(object sender, RoutedEventArgs e)
    {
        FolderBrowserDialog dialog = new();
        dialog.ShowDialog();

        string selectedPath = dialog.SelectedPath;

        if (string.IsNullOrEmpty(selectedPath))
            return;

        TxtFolderName.Text = selectedPath;
        ListDirectory(selectedPath);
    }

    /// <summary>
    /// 離開按鈕
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    /// <summary>
    /// TreeView雙點事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void TvFolders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        BtnFirstPicture.IsEnabled = false;

        if (sender is not System.Windows.Controls.TreeView tree)
        {
            return;
        }

        if (tree.SelectedItem == null || tree.SelectedItem is not TreeViewItem item)
            return;

        string? selectedPath = item.Tag.ToString();
        TxtFolderName.Text = selectedPath;

        if (string.IsNullOrEmpty(selectedPath))
        {
            return;
        }

        if (File.Exists(selectedPath))
            IsSelectedFolder = false;
        else
            IsSelectedFolder = true;

        ReloadSettings();

        if (IsSelectedFolder)
        {
            DisplayImageInfos = new DirectoryInfo(selectedPath).GetFiles("*", SearchOption.TopDirectoryOnly).Where(x => ImageNameRegex.IsMatch(x.Name)).ToArray();

            Counter = 0;
            BindingImage();
        }
        else
        {
            BtnFirstPicture.IsEnabled = false;
            BtnPrePicture.IsEnabled = false;
            BtnNextPicture.IsEnabled = false;
            BtnLastPicture.IsEnabled = false;

            if (File.Exists(selectedPath))
            {
                DisplayImageInfos = [new(selectedPath)];
                Counter = 0;
                BindingImage();
            }
        }
    }

    /// <summary>
    /// 點選第一張圖片
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnFirstPicture_Click(object sender, RoutedEventArgs e)
    {
        ReloadSettings();
        Counter = 0;
        BindingImage();
    }

    /// <summary>
    /// 點選上一張圖片
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnPrePicture_Click(object sender, RoutedEventArgs e)
    {
        ReloadSettings();
        Counter--;
        BindingImage();
    }

    /// <summary>
    /// 點選下一張圖片
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnNextPicture_Click(object sender, RoutedEventArgs e)
    {
        ReloadSettings();
        Counter++;
        BindingImage();
    }

    /// <summary>
    /// 點選最後張圖片
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnLastPicture_Click(object sender, RoutedEventArgs e)
    {
        ReloadSettings();
        Counter = DisplayImageInfos.Length - 1;
        BindingImage();
    }

    /// <summary>
    /// 重讀設定檔
    /// </summary>
    public void ReloadSettings()
    {
        AutoPlayTimer.Tick -= new EventHandler(AutoPlayTimer_Tick);

        AutoPlayTimer = new DispatcherTimer();
        AutoPlayTimer.Tick += new EventHandler(AutoPlayTimer_Tick);
        AutoPlayTimer.Interval = new TimeSpan(0, 0, Settings.AutoPlaySec);
        AutoPlayTimer.Start();
    }

    /// <summary>
    /// 列出選擇的資料夾清單
    /// </summary>
    /// <param name="path">路徑</param>
    private void ListDirectory(string path)
    {
        TvFolders.Items.Clear();
        DirectoryInfo rootDirectoryInfo = new(path);
        TvFolders.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
    }

    /// <summary>
    /// 產生樹狀圖的節點
    /// </summary>
    /// <param name="directoryInfo">資料夾資訊</param>
    /// <returns></returns>
    private TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
    {
        var directoryNode = new TreeViewItem { Header = directoryInfo.Name, Tag = directoryInfo.FullName, Foreground = System.Windows.Media.Brushes.White };

        foreach (var directory in directoryInfo.GetDirectories().Where(x => !x.Name.Contains("System Volume Information")))
            directoryNode.Items.Add(CreateDirectoryNode(directory));

        foreach (var file in directoryInfo.GetFiles().Where(x => ImageNameRegex.IsMatch(x.Name)))
            directoryNode.Items.Add(new TreeViewItem { Header = file.Name, Tag = file.FullName, Foreground = System.Windows.Media.Brushes.White });

        return directoryNode;
    }

    /// <summary>
    /// 綁定圖片資訊
    /// </summary>
    private void BindingImage()
    {
        BtnFirstPicture.IsEnabled = true;
        BtnPrePicture.IsEnabled = true;
        BtnNextPicture.IsEnabled = true;
        BtnLastPicture.IsEnabled = true;

        if (DisplayImageInfos.Length == 0)
        {
            BtnFirstPicture.IsEnabled = false;
            BtnPrePicture.IsEnabled = false;
            BtnNextPicture.IsEnabled = false;
            BtnLastPicture.IsEnabled = false;

            ImgDisplay.Source = null;

            return;
        }

        if (Counter > DisplayImageInfos.Length - 1)
        {
            Counter = DisplayImageInfos.Length - 1;
            string src = DisplayImageInfos[Counter].FullName;
            ImgDisplay.Source = new BitmapImage(new Uri(src));
            return;
        }

        string imageUri = DisplayImageInfos[Counter].FullName;
        ImgDisplay.Source = new BitmapImage(new Uri(imageUri));

        if (Counter == 0)
        {
            BtnFirstPicture.IsEnabled = false;
            BtnPrePicture.IsEnabled = false;
        }

        if (Counter == DisplayImageInfos.Length - 1)
        {
            BtnNextPicture.IsEnabled = false;
            BtnLastPicture.IsEnabled = false;
        }
    }
}
