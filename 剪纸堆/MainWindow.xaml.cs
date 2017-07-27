using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

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
                    this.addNewButton();
                    // pass the message to the next viewer.
                    Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
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
        private void addNewButton()
        {if(needAddButton>0)
            {
                needAddButton--;
                return;
            }
            Button tempButton = new Button()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxHeight = 100,
                MaxWidth = 180
            };
            Border tempBorder = new Border();
         

               // IDataObject iData = new DataObject();
          //  iData = Clipboard.GetDataObject();
          if(Clipboard.ContainsText())
            //if (iData.GetDataPresent(DataFormats.Text))
            {
                string strValue;
                //iData.GetDataPresent(DataFormats.Bitmap)  
                do
                {
                    try
                    {
                        strValue = Clipboard.GetText();
                    }
                    catch
                    {
                        strValue = null;
                        //sleep(1000);
                                            }
                    //Debug.WriteLine(value);
                }
                while (strValue == null);
                tempButton.Content = strValue;
                tempButton.Style = Resources["tempButtonStyle"] as Style;
                tempBorder = new Border()
                {
                    BorderThickness = new Thickness(5,5,5, 5),
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
                    //Child = System.Windows.Markup.XamlReader.Parse(System.Windows.Markup.XamlWriter.Save(tempButton)) as Button

                };

                tempButton.Style = Resources["buttonStyle"] as Style;

                tempButton.Click += TempButtonClickEventHandler;


                    // Child = temp
                };
                Grid tempGrid = new Grid();
             //tempGrid.Children.Add(tempBorder);
           tempGrid.Children.Add(tempButton);

            spnl.Children.Insert(3, new TextBlock());
            spnl.Children.Insert(3, tempGrid);

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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitCBViewer();
          


        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.DragMove();
        }
    }
}
