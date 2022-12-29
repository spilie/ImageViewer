using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ImageViewer.Model;

namespace ImageViewer
{
    /// <summary>
    /// SettingsWindow.xaml 的互動邏輯
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private MainWindow parent { get; set; }

        public SettingsWindow(MainWindow _parent)
        {
            InitializeComponent();
            this.parent = _parent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.IsAutoPlay)
                chkAutoPlay.IsChecked = true;

            if (Settings.IsCyclePlay)
                chkCyclePlay.IsChecked = true;

            txtAutoPlaySec.Text = Settings.AutoPlaySec.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Save((bool)chkAutoPlay.IsChecked, Convert.ToInt32(txtAutoPlaySec.Text), (bool)chkCyclePlay.IsChecked);
            this.parent.ReloadSettings();

            this.Close();
        }

        private void btnLeave_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Judge TextBox input is number or not
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
