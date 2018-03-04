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
using System.Windows.Controls.Primitives;
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
using static FloatClipboard.SharedStaticData;

namespace FloatClipboard
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


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);

    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if(set.Visiable==false)
            {
                Visibility = Visibility.Hidden;
            }
        }

        #region 剪贴板监控
        /// <summary>
        /// Next clipboard viewer window 
        /// </summary>
        private IntPtr hWndNextViewer;

        /// <summary>
        /// The <see cref="HwndSource"/> for this window.
        /// </summary>
        private HwndSource hWndSource;

        private bool isViewing;
        /// <summary>
        /// 初始化剪贴板监控
        /// </summary>
        private void InitCBViewer()
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndSource.AddHook(this.WinProc);   // start processing window messages
            hWndNextViewer = Win32.SetClipboardViewer(hWndSource.Handle);   // set this window as a viewer
            isViewing = true;
        }
        /// <summary>
        /// 关闭剪贴板监控（无用）
        /// </summary>
        private void CloseCBViewer()
        {
            // remove this window from the clipboard viewer chain
            Win32.ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);

            hWndNextViewer = IntPtr.Zero;
            hWndSource.RemoveHook(this.WinProc);

            isViewing = false;
        }
        /// <summary>
        /// 剪贴板监控
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
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
                    if (openLastNow)
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
        #endregion


        #region 字段声明
        List<Border> buttons = new List<Border>();
        public System.Windows.Forms.NotifyIcon notifyIcon;
        XmlDocument xml = new XmlDocument();
        XmlElement root;
        bool openLastNow = true;
        /// <summary>
        /// 主界面高度动画
        /// </summary>
        DoubleAnimation aniHeight;
        /// <summary>
        /// 滚动条透明度动画
        /// </summary>
        DoubleAnimation aniScrollBarOpacity;
        /// <summary>
        /// 主界面透明度动画
        /// </summary>
        DoubleAnimation aniOpacity;
        /// <summary>
        /// 动画板
        /// </summary>
        Storyboard storyBoard;
        /// <summary>
        /// 滚动条
        /// </summary>
        ScrollBar scroll;
        /// <summary>
        /// 因为复制了文本以后剪贴板改变会重新激发事件，所以用这个来判断剪贴板是否是手动改变的
        /// </summary>
        int needAddButton = 0;
        #endregion

        #region 按钮
        /// <summary>
        /// 创建新的按钮，这个是自动取剪贴板并且存档
        /// </summary>
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
                root.SetAttribute("Count", (int.Parse(root.GetAttribute("Count")) + 1).ToString());
                XmlElement xe = xml.CreateElement("String_" + root.GetAttribute("Count"));
                xe.SetAttribute("Time", DateTime.Now.ToString() + "." + DateTime.Now.Millisecond);
                xe.SetAttribute("Value", strValue);
                root.AppendChild(xe);
                xml.Save("OldClipBoard.xml");


                AddNewButton(strValue);

                if (stk.Children.Count > 2 * set.MaxObject)
                {
                    stk.Children.RemoveAt(stk.Children.Count - 1);
                    stk.Children.RemoveAt(stk.Children.Count - 1);
                }
            }
        }

        /// <summary>
        /// 增加新的按钮
        /// </summary>
        /// <param name="strValue"></param>
        private void AddNewButton(string strValue)
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

            tempButton.Click += BtnTextClickEventHandler;


            // Child = temp

            Grid tempGrid = new Grid();
            tempGrid.Children.Add(tempBorder);
            tempGrid.Children.Add(tempButton);

            stk.Children.Insert(0, new TextBlock());
            stk.Children.Insert(0, tempGrid);

            //spnl.Children.Add(tempGrid);
            //spnl.Children.Add(new TextBlock());
        }

        /// <summary>
        /// 按钮的事件，即复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTextClickEventHandler(object sender, RoutedEventArgs e)
        {
            needAddButton = 2;
            Clipboard.SetText((sender as Button).Content as string);

        }
        #endregion


        #region 窗口和动画
        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void WinMainLoadedEventHandler(object sender, RoutedEventArgs e)
        {
            scroll = FindVisualChildHelper.FindVisualChild<ScrollBar>(sv);

            IconAndNotify();

            if (!File.Exists("OldClipBoard.xml"))
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
                root = xml.CreateElement("剪纸堆");
            }

            LoadHistory();
            //foreach (XmlElement i in root)
            //{
            //    addNewButton(i.GetAttribute("Value"));
            //}
            //waitTimer.Interval = new TimeSpan(10000 * 1000);
            // waitTimer.Tick += new EventHandler(AnimationWatingTimerTickEventHandler);
            InitalizeAnimation();

            InitCBViewer();

             WinMainMouseLeaveEventHandler(null, null);

        }

        public void LoadHistory()
        {
            root = xml.DocumentElement;
            int count = int.Parse(root.GetAttribute("Count"));
            for (int i = (count > set.MaxObject ? count - set.MaxObject + 1 : 1); i <= count; i++)
            {
                AddNewButton(root["String_" + i.ToString()].GetAttribute("Value"));
            }
        }

        /// <summary>
        /// 窗口图标和托盘图标
        /// </summary>
        private void IconAndNotify()
        {

            //设置托盘的各个属性
            notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                BalloonTipText = "设置界面在托盘",
                Text = "剪纸堆",
                Icon = Properties.Resources.icon,
                Visible = true
            };
            notifyIcon.MouseClick += (object notifySender, System.Windows.Forms.MouseEventArgs notifyE) =>
           {
               if (notifyE.Button == System.Windows.Forms.MouseButtons.Left)
               {
                   Visibility = Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
               }
           };
            // new System.Windows.Forms.MouseEventHandler(NotifyIconClickEventHandler);

            System.Windows.Forms.MenuItem miExit = new System.Windows.Forms.MenuItem("退出");
            miExit.Click += (object sender3, EventArgs e3) =>
           {
               notifyIcon.Visible = false;
               Close();
           };
            System.Windows.Forms.MenuItem miSettings = new System.Windows.Forms.MenuItem("设置");
            miSettings.Click += (object sender3, EventArgs e3) =>
           {
               //Win32.POINT p = new Win32.POINT();
               //Win32.GetCursorPos(out p);
               // //Point p = Mouse.GetPosition(e3.Source as FrameworkElement);
               // Window settingPage = new WinSettings()
               //{
               //    Left = p.X,
               //    Top = p.Y
               //};
               WinSettings winSettings = new WinSettings();
               //{
               //    WindowStyle = WindowStyle.SingleBorderWindow,
               //    WindowStartupLocation = WindowStartupLocation.CenterScreen,
               //    ShowInTaskbar = true,
               //};
               winSettings.Show();
           };

            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { miSettings, miExit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
            // this.Icon = new BitmapImage(new Uri(tempFileName));
        }

        /// <summary>
        /// 按住标题时可以拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeadingGridMouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// 鼠标进入窗口执行动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WinMainMouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            WindowAnimation(1);
            //Debug.WriteLine("enter");
        }

        /// <summary>
        /// 窗口动画的初始化部分
        /// </summary>
        private void InitalizeAnimation()
        {
            //string tempName = "tempName";
            //NameScope.SetNameScope(animationObject as DependencyObject, new NameScope());
            //RegisterName(tempName, this);
            //aniHeight.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            //Storyboard.SetTargetName(aniHeight, tempName);
            //Storyboard.SetTargetProperty(aniHeight, new PropertyPath(property));
            //storyBoard.Children.Add(aniHeight);

            aniHeight = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };
            aniScrollBarOpacity = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };
            aniOpacity = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(1),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(aniHeight, this);
            Storyboard.SetTargetProperty(aniHeight, new PropertyPath(HeightProperty));
            Storyboard.SetTarget(aniScrollBarOpacity, scroll);
            Storyboard.SetTargetProperty(aniScrollBarOpacity, new PropertyPath(OpacityProperty));
            Storyboard.SetTarget(aniOpacity, this);
            Storyboard.SetTargetProperty(aniOpacity, new PropertyPath(OpacityProperty));

            storyBoard = new Storyboard();
            storyBoard.Children.Add(aniHeight);
            storyBoard.Children.Add(aniOpacity);
            storyBoard.Children.Add(aniScrollBarOpacity);
        }

        /// <summary>
        /// 窗口动画的实行部分；1：展开；2：收拢
        /// </summary>
        /// <param name="type"></param>
        private void WindowAnimation(int type)
        {
            aniScrollBarOpacity.To = type == 1 ? 1 : 0;
            aniOpacity.To = type == 1 ? 1 : set.Opacity;

            if (stk.ActualHeight + 64 > SystemParameters.WorkArea.Height)
            {
                aniHeight.To = type == 1 ? SystemParameters.WorkArea.Height - Top : set.Height;
            }
            else
            {
                aniHeight.To = type == 1 ? stk.ActualHeight + 64 : set.Height;
            }
            if(type==2)
            {
                sv.ScrollToHome();
            }
            storyBoard.Begin(this);
        }

        /// <summary>
        /// 滚动条透明度改变的动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void ScrollBarOpacityChangeTimerTickEventHandler(object sender, EventArgs e)
        //{

        //    //Debug.WriteLine(da.To == 300);
        //    if (animation.To == 300)
        //    {
        //        if (scroll.Opacity>= 0)
        //        {
        //         scroll.Opacity=scroll.Opacity- 0.04;
        //        }
        //        else
        //        {
        //            opacityTimer.Stop();
        //        }
        //    }
        //    else
        //    {
        //        if (scroll.Opacity <= 1)
        //        {
        //            scroll.Opacity = scroll.Opacity+ 0.04;
        //        }
        //        else
        //        {
        //            opacityTimer.Stop();
        //        }
        //    }
        //}

        /// <summary>
        /// 鼠标移出窗口准备缩回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WinMainMouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            //try
            //{
            //    waitTimer.IsEnabled = false;
            //    waitTimer.Start();
            //}
            //catch
            //{

            //}
            // Debug.WriteLine("leave");

            await Task.Delay(2000);
            if (Mouse.GetPosition(this as FrameworkElement).X < 0 && Mouse.GetPosition(this as FrameworkElement).Y < 0)
            {
                WindowAnimation(2);
            }

            set.Left = Left;
            set.Top = Top;

            set.Save();
            //Debug.WriteLine("Changed");
        }

        /// <summary>
        /// 缩回动画的等待
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void AnimationWatingTimerTickEventHandler(object sender, EventArgs e)
        //{
        //    if (Mouse.GetPosition(this as FrameworkElement).X < 0 && Mouse.GetPosition(this as FrameworkElement).Y < 0)
        //        WindowAnimation(2);
        //    //waitTimer.IsEnabled = false;
        //    return;
        //}

        /// <summary>
        /// 右键标题来呼出设置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtHeadingPreviewMouseRightButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            //Point p = Mouse.GetPosition(e.Source as FrameworkElement);
            //WinSettings settingPage = new WinSettings()
            //{
            //    Left = p.X + Left,
            //    Top = p.Y + Top,
            //};
            //Topmost = false;
            //settingPage.ShowDialog();
            //Topmost = true;

            MenuItem menuSettings = new MenuItem() { Header = "设置" };
            menuSettings.Click += (p1, p2) =>
                {
                    new WinSettings().ShowDialog();
                    WindowAnimation(2);
                };
            MenuItem menuHide = new MenuItem() { Header = "隐藏" };
            menuHide.Click += (p1, p2) => Hide();
            MenuItem menuExit = new MenuItem() { Header = "退出" };
            menuExit.Click += (p1, p2) => Close();

            ContextMenu menu = new ContextMenu()
            {
                PlacementTarget=this,
                Items =
                {
                    menuSettings,
                    menuHide,
                    menuExit,
                },
                IsOpen = true,
            };

        }
        

        #endregion
    }
}
