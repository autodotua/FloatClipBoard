using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace 剪纸堆
{
    internal static class Win32
    {
        /// <summary>
        /// The WM_DRAWCLIPBOARD message notifies a clipboard viewer window that 
        /// the content of the clipboard has changed. 
        /// </summary>
        internal const int WM_DRAWCLIPBOARD = 0x0308;

        /// <summary>
        /// A clipboard viewer window receives the WM_CHANGECBCHAIN message when 
        /// another window is removing itself from the clipboard viewer chain.
        /// </summary>
        internal const int WM_CHANGECBCHAIN = 0x030D;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Next clipboard viewer window 
        /// </summary>
        private IntPtr hWndNextViewer;

        /// <summary>
        /// The <see cref="HwndSource"/> for this window.
        /// </summary>
        private HwndSource hWndSource;

        private bool isViewing;

        private void InitCBViewer()
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndSource.AddHook(this.WinProc);   // start processing window messages
            hWndNextViewer = Win32.SetClipboardViewer(hWndSource.Handle);   // set this window as a viewer
            isViewing = true;
        }

        private void CloseCBViewer()
        {
            // remove this window from the clipboard viewer chain
            Win32.ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);

            hWndNextViewer = IntPtr.Zero;
            hWndSource.RemoveHook(this.WinProc);

            isViewing = false;
        }

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32.WM_CHANGECBCHAIN:
                    if (wParam == hWndNextViewer)
                    {
                        // clipboard viewer chain changed, need to fix it.
                        hWndNextViewer = lParam;
                    }
                    else if (hWndNextViewer != IntPtr.Zero)
                    {
                        // pass the message to the next viewer.
                        Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    }
                    break;

                case Win32.WM_DRAWCLIPBOARD:
                    // clipboard content changed
                    if(openLastNow)
                    {
                        openLastNow = false;
                    }
                    else
                    {
                        CreatNew();
                    }
                    // pass the message to the next viewer.
                    Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        private void CreatNew()
        { 
            if (Clipboard.ContainsText())
            {
                if (needAddButton > 0)
                {
                    needAddButton--;
                    return;
                }
                string strValue;
                do
                {
                    try
                    {
                        strValue = Clipboard.GetText();
                    }
                    catch
                    {
                        strValue = null;
                    }
                }
                while (strValue == null);
               // root.FirstChild.Value = (int.Parse(root.FirstChild.Value) + 1).ToString();
                root.SetAttribute("Count", (int.Parse(root.GetAttribute("Count") )+ 1).ToString());
                XmlElement xe = xml.CreateElement("String_"+root.GetAttribute("Count"));
                xe.SetAttribute("Time", DateTime.Now.ToString() + "." + DateTime.Now.Millisecond);
                xe.SetAttribute("Value", strValue);
                root.AppendChild(xe);
                xml.Save("OldClipBoard.xml");
                addNewButton(strValue);
            }
        }

        public static void sleep(int times)//延时，单位毫秒
        {
            int start = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - start) < times)
            {
                // Application.DoEvents();
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
        }
        private void addNewButton(string strValue)
        {
            Button tempButton = new Button()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxHeight = 100,
                MaxWidth = 180
            };
            Border tempBorder = new Border();
            tempButton.Content = strValue;
            tempButton.Style = Resources["tempButtonStyle"] as Style;
            tempBorder = new Border()
            {
                BorderThickness = new Thickness(5, 5, 5, 5),
                CornerRadius = new CornerRadius(3, 3, 3, 3),
                BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 255)),
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxHeight = 100,
                MaxWidth = 180,
                Effect = new System.Windows.Media.Effects.DropShadowEffect()
                {
                    Color = Color.FromRgb(80, 80, 100),
                    Opacity = 0.5,
                },
                Child = System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(tempButton)) as Button
            };

            tempButton.Style = Resources["buttonStyle"] as Style;

            tempButton.Click += TempButtonClickEventHandler;


            // Child = temp

            Grid tempGrid = new Grid();
            tempGrid.Children.Add(tempBorder);
            tempGrid.Children.Add(tempButton);

            spnl.Children.Insert(0, new TextBlock());
            spnl.Children.Insert(0, tempGrid);

            //spnl.Children.Add(tempGrid);
            //spnl.Children.Add(new TextBlock());
        }
        int needAddButton = 0;
        private void TempButtonClickEventHandler(object sender, RoutedEventArgs e)
        {
            needAddButton = 2;
            Clipboard.SetText((sender as Button).Content as string);

        }

        List<Border> buttons = new List<Border>();
        Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private System.Windows.Forms.NotifyIcon notifyIcon;
        XmlDocument xml = new XmlDocument();
        XmlElement root;
        bool openLastNow = true;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string tempFileName = System.IO.Path.GetTempFileName();
            FileStream fs = new FileStream(tempFileName, FileMode.Create);
            Properties.Resources.icon.Save(fs);
            fs.Close();

            //设置托盘的各个属性
            notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                BalloonTipText = "设置界面在托盘",
                Text = "剪纸堆",
                Icon = new System.Drawing.Icon(tempFileName),
                Visible = true
            };
            notifyIcon.MouseClick += delegate (object notifySender, System.Windows.Forms.MouseEventArgs notifyE)
             {

             };
            // new System.Windows.Forms.MouseEventHandler(NotifyIconClickEventHandler);

            System.Windows.Forms.MenuItem miExit = new System.Windows.Forms.MenuItem("退出");
            miExit.Click += new EventHandler(delegate (object sender3, EventArgs e3)
            {
                notifyIcon.Visible = false;
                Application.Current.Shutdown();
            });


            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { miExit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            this.Icon = new BitmapImage(new Uri(tempFileName));
            
            if (!System.IO.File.Exists("OldClipBoard.xml"))
            {
                XmlDeclaration xdec = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                xml.AppendChild(xdec);
                root = xml.CreateElement("剪纸堆");
                root.SetAttribute("Count", "0");
                xml.AppendChild(root);
                xml.Save("OldClipBoard.xml");
            }
            else
            {
                xml.Load("OldClipBoard.xml");
                
            }
           root  = xml.DocumentElement;

            //for (int i = 0; i < int.Parse(root.GetAttribute("Count")); i++)
            //{
            //    addNewButton();
            //}
            foreach (XmlElement i in root)
            {
                addNewButton(i.GetAttribute("Value"));
            }
            waitTimer.Interval = new TimeSpan(10000 * 1000);
            waitTimer.Tick += new EventHandler(waitTimer_Tick);
            InitCBViewer();


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.DragMove();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            WindowAnimation(1);
            //Debug.WriteLine("enter");
        }

        private void WindowAnimation(int type)
        {
            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName(this.Name, this);

            DoubleAnimation da = new DoubleAnimation();
            switch (type)
            {
                case 1:

                    da.To = SystemParameters.WorkArea.Height;
                    break;
                case 2:

                    da.To = 300;
                    break;
            }
            da.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            Storyboard.SetTargetName(da, this.Name);
            Storyboard.SetTargetProperty(da, new PropertyPath(Window.HeightProperty));
            Storyboard sb = new Storyboard();
            sb.Children.Add(da);
            sb.Begin(this);
        }
        DispatcherTimer waitTimer = new System.Windows.Threading.DispatcherTimer();
        private void mainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                
                waitTimer.IsEnabled = false;
                waitTimer.Start();  
            }
            catch
            {

            }
            // Debug.WriteLine("leave");
            ChangeAppSettings("LeftToScreenRight", Left.ToString());
            ChangeAppSettings("TopToScreenTop", Top.ToString());
            cfa.Save();
            //Debug.WriteLine("Changed");
        }

        private void waitTimer_Tick(object sender, EventArgs e)
        {
           if(Mouse.GetPosition(this as FrameworkElement).X<0 && Mouse.GetPosition(this as FrameworkElement).Y<0)
            WindowAnimation(2);
            waitTimer.IsEnabled = false;
            return;
        }

        private void ChangeAppSettings(string key, string targetValue)
        {
            
            if (cfa.AppSettings.Settings[key] == null)
            {
                cfa.AppSettings.Settings.Add(new KeyValueConfigurationElement(key, targetValue));
            }
            else
            {
                cfa.AppSettings.Settings[key].Value = targetValue;
            }
        }
    }
}
