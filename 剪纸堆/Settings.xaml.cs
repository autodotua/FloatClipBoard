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
         
            scbMax.Value = int.Parse(cfa.AppSettings.Settings["MaxObject"].Value);
            
        }
        Configuration cfa;

        private void scbOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkOpacity.Text = "透明度：" + Math.Floor(scbOpacity.Value).ToString() + "%";

        }

        private void scbMax_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbkMax.Text = "最多：" + Math.Floor(scbMax.Value).ToString() + "条";
        }

        private void btnOKClickEventHandler(object sender, RoutedEventArgs e)
        {
            winMain.Opacity = Math.Floor(scbOpacity.Value) / 100;
            
            cfa.AppSettings.Settings["Opacity"].Value = (Math.Floor(scbOpacity.Value) / 100).ToString();
            cfa.AppSettings.Settings["MaxObject"].Value = Math.Floor(scbMax.Value).ToString();
            cfa.Save();
            //Close();
            Application.Current.Shutdown();
            System.Reflection.Assembly.GetEntryAssembly();
            string startpath = System.IO.Directory.GetCurrentDirectory();
            Process.Start(startpath + "/剪纸堆.exe");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
