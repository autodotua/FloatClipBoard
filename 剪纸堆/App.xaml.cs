using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static FloatClipboard.SharedStaticData;

namespace FloatClipboard
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += AppDispatcherUnhandledExceptionEventHandler;
            set = new Properties.Settings();
            win = new MainWindow()
            {
                Left = set.Left,
                Top = set.Top,
                Opacity = set.Opacity,
                Visibility = set.Visiable ? Visibility.Visible : Visibility.Collapsed,
            };

            win.Show();
        }

        private static void AppDispatcherUnhandledExceptionEventHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ToString(), "发生异常", MessageBoxButton.OK, MessageBoxImage.Error);

            string logName = "UnhandledException.log";
            if (File.Exists(logName))
            {
                string oldFile = File.ReadAllText(logName);
                File.WriteAllText(logName,
                oldFile
                + Environment.NewLine + Environment.NewLine
                + DateTime.Now.ToString()
                + Environment.NewLine
                + e.Exception.ToString());
            }
            else
            {
                File.WriteAllText(logName,
                  DateTime.Now.ToString()
                  + Environment.NewLine
                   + e.Exception.ToString());
            }
            Current.Shutdown();
            return;
        }

    }
}
