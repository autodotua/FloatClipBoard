using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace 剪纸堆
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);//配置项


            Window winMain = new MainWindow()
            {
                Left = cfa.AppSettings.Settings["LeftToScreenRight"] == null ? 300 : int.Parse(cfa.AppSettings.Settings["LeftToScreenRight"].Value),// SystemParameters.WorkArea.Width - 300,
                Top = cfa.AppSettings.Settings["TopToScreenTop"] == null ?100 : int.Parse(cfa.AppSettings.Settings["TopToScreenTop"].Value)
            };
            winMain.Show();
        }
    }
}
