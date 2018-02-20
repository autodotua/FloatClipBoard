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
using static FloatClipboard.SharedStaticData;

namespace FloatClipboard
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class WinSettings : Window
    {
        public WinSettings()
        {
            InitializeComponent();
            sldOpacity.Value = set.Opacity;
            sldHeight.Value = set.Height;
            if (System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\FloatClipBoard.lnk"))
            {
                cbxStartup.IsChecked = true;
            }
            cbxHide.IsChecked = !set.Visiable;
            sldMax.Value = set.MaxObject;

        }

        /// <summary>
        /// 透明度滚动条改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScbOpacityValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkOpacity.Text = "透明度：" + Math.Floor(sldOpacity.Value * 100).ToString() + "%";
           // win.Opacity = sldOpacity.Value;

        }

        /// <summary>
        /// 最多项目滚动条改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScbMaxValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkMax.Text = "最多：" + Math.Floor(sldMax.Value).ToString() + "条";
        }

        /// <summary>
        /// 单击保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOKClickEventHandler(object sender, RoutedEventArgs e)
        {

            set.Opacity = sldOpacity.Value;
            set.Visiable = !cbxHide.IsChecked.Value;
            set.Height = sldHeight.Value;
            set.Save();
            if (set.MaxObject != (int)Math.Floor(sldMax.Value))
            {
                set.MaxObject = (int)Math.Floor(sldMax.Value);
                set.Save();
                win.LoadHistory();
            }
           
            Close();
            //Application.Current.Shutdown();
            //System.Reflection.Assembly.GetEntryAssembly();
            //string startpath = System.IO.Directory.GetCurrentDirectory();
            //Process.Start(startpath + "/剪纸堆.exe");
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

        private void ScbHeightValueChangedEventHandler(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkHeight.Text = "收缩高度：" + Math.Floor(sldHeight.Value);
            win.Height=sldHeight.Value;
        }
    }
}
