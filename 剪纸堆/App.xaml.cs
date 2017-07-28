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
        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);//配置项

        protected override void OnStartup(StartupEventArgs e)
        {

            checkConfig("LeftToScreenRight","300");
            checkConfig("TopToScreenTop", "100");
            checkConfig("Opacity", "0.5");
            checkConfig("MaxObject", "100");
            checkConfig("Startup", "false");
            checkConfig("Hide", "false");
            Window winMain = new MainWindow()
            {
                Left =   int.Parse(cfa.AppSettings.Settings["LeftToScreenRight"].Value),// SystemParameters.WorkArea.Width - 300,
                Top =  int.Parse(cfa.AppSettings.Settings["TopToScreenTop"].Value),
                Opacity=double.Parse(cfa.AppSettings.Settings["Opacity"].Value),
                Visibility= cfa.AppSettings.Settings["Hide"].Value=="true"?Visibility.Hidden:Visibility.Visible

            };




            winMain.Show();
        }

        private void checkConfig(string key,string defaultValue)
        {
            if(cfa.AppSettings.Settings[key]==null)
            {
                cfa.AppSettings.Settings.Add(key, defaultValue);
                cfa.Save();
            }
        }
    }
}
