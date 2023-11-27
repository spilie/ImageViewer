using ImageViewer.Model;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ImageViewer;

/// <summary>
/// SettingsWindow.xaml 的互動邏輯
/// </summary>
public partial class SettingsWindow : Window
{
    /// <summary>
    /// 設定檔案
    /// </summary>
    private Settings Settings { get; set; }

    /// <summary>
    /// 父層Windows
    /// </summary>
    private MainWindow Parents { get; set; }

    /// <summary>
    /// 建構式
    /// </summary>
    /// <param name="parent">父層</param>
    public SettingsWindow(MainWindow parent)
    {
        InitializeComponent();
        Parents = parent;
        Settings = Settings.GetInstance();
    }

    /// <summary>
    /// Load事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (Settings.IsAutoPlay)
            ChkAutoPlay.IsChecked = true;

        if (Settings.IsCyclePlay)
            ChkCyclePlay.IsChecked = true;

        TxtAutoPlaySec.Text = Settings.AutoPlaySec.ToString();
    }

    /// <summary>
    /// 存檔按鈕事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        bool autoPlay = ChkAutoPlay.IsChecked != null && ChkAutoPlay.IsChecked.Value;
        bool cyclePlay = ChkCyclePlay.IsChecked != null && ChkCyclePlay.IsChecked.Value;
        int autoPlaySec = Convert.ToInt32(TxtAutoPlaySec.Text);

        Settings.Save(autoPlay, cyclePlay, autoPlaySec);
        Parents.ReloadSettings();

        Close();
    }

    /// <summary>
    /// 關閉事件
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void BtnLeave_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 判斷輸入是否為數字
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">事件</param>
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}
