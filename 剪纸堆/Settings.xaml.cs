using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace 剪纸堆
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        MainWindow winMain;

        public Settings(MainWindow _winMain)
        {
            winMain = _winMain;
            InitializeComponent();
            cfa = winMain.cfa;
            scbOpacity.Value = Math.Floor(winMain.Opacity * 100);
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\FloatClipBoard.lnk"))
            {
                cbxStartup.IsChecked = true;
            }
            if (cfa.AppSettings.Settings["Hide"].Value == "true")
            {
                cbxHide.IsChecked = true;
            }
            scbMax.Value = int.Parse(cfa.AppSettings.Settings["MaxObject"].Value);

        }
        Configuration cfa;

        /// <summary>
        /// 透明度滚动条改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScbOpacityValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkOpacity.Text = "透明度：" + Math.Floor(scbOpacity.Value).ToString() + "%";

        }

        /// <summary>
        /// 最多项目滚动条改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScbMaxValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkMax.Text = "最多：" + Math.Floor(scbMax.Value).ToString() + "条";
        }

        /// <summary>
        /// 单击保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOKClickEventHandler(object sender, RoutedEventArgs e)
        {
            winMain.Opacity = Math.Floor(scbOpacity.Value) / 100;

            cfa.AppSettings.Settings["Opacity"].Value = (Math.Floor(scbOpacity.Value) / 100).ToString();
            cfa.AppSettings.Settings["MaxObject"].Value = Math.Floor(scbMax.Value).ToString();
            cfa.AppSettings.Settings["Hide"].Value = cbxHide.IsChecked == true ? "true" : "false";
            cfa.AppSettings.Settings["Startup"].Value = cbxStartup.IsChecked == true ? "true" : "false";

            cfa.Save();
            //Close();
            Application.Current.Shutdown();
            System.Reflection.Assembly.GetEntryAssembly();
            string startpath = System.IO.Directory.GetCurrentDirectory();
            Process.Start(startpath + "/剪纸堆.exe");
        }

        /// <summary>
        /// 单击退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExitClickEventHandler(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// 单击开机自启按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkStartupClickEventHandler(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {

                string Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);// System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\新建文件夹 (3)"; //"%USERPROFILE%\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";

                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshShortcut sc = (IWshShortcut)shell.CreateShortcut(Path + "\\FloatClipBoard.lnk");
                sc.TargetPath = Process.GetCurrentProcess().MainModule.FileName;
                sc.WorkingDirectory = Environment.CurrentDirectory;
                sc.Save();
                //Debug.WriteLine(Path);
                if (System.IO.File.Exists(Path + "\\FloatClipBoard.lnk"))
                {
                    MessageBox.Show("成功", "结果", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("失败", "结果", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);// System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\新建文件夹 (3)"; //"%USERPROFILE%\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup";

                System.IO.File.Delete(Path + "\\FloatClipBoard.lnk");
                if (System.IO.File.Exists(Path + "\\FloatClipBoard.lnk"))
                {
                    MessageBox.Show("失败", "结果", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("成功", "结果", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
        }

    }
}
