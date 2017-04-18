using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Media.Capture;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Core;

namespace App2
{
    public sealed partial class MainPage : Page
    {
        //加速度传感器变量
        Accelerometer acc;
        private uint reportinterval;
        //判断LED是否打开变量
        bool flag = false;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            App.Current.Resuming += OnAppResuming;
            acc = Accelerometer.GetDefault();
 
            if (acc != null)
            { 
                //设置加速度传感器读数间隔
                reportinterval = 180;
            }
        }

        // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
        // This value will be used later to activate the sensor.
        // uint minReportInterval = acc.MinimumReportInterval;

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //将按键设置为可按下
            button.IsEnabled = true;
        }

        /// <summary>
        /// 在离开此页时调用。
        /// </summary>
        /// <param name="e">描述如何导航此页的事件数据。</param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (button.IsEnabled)
            {
                //窗口不再可见
                Window.Current.VisibilityChanged -= new WindowVisibilityChangedEventHandler(VisibilityChanged);
                //停止ReadingChanged事件
                acc.ReadingChanged -= new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                //传感器不再使用，重置读数间隔
                acc.ReportInterval = 0;
            }

            base.OnNavigatingFrom(e);
        }

        private void VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (button.IsEnabled)
            {
                if (e.Visible)
                {
                    // Re-enable sensor input (no need to restore the desired reportInterval... it is restored for us upon app resume)
                    acc.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
                }
                else
                {
                    // Disable sensor input (no need to restore the default reportInterval... resources will be released upon app suspension)
                    acc.ReadingChanged -= new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
                }
            }
        }

        private void ShakeItOn(object sender, RoutedEventArgs e)
        {
            if (acc != null)
            {
                //设置加速度传感器读数间隔
                acc.ReportInterval = reportinterval;
                //处理当前程序窗口的VisibilityChanged事件，当窗口可见时才读取加速度数据
                Window.Current.VisibilityChanged += new WindowVisibilityChangedEventHandler(VisibilityChanged);
                //当有新的读数报告时，会发生ReadingChanged事件，需处理该事件
                acc.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
                //禁用按键
                button.IsEnabled = false;
            }
        }
        
        async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //异步读取加速度数据
                AccelerometerReading reading = e.Reading;
                statusTextBlock.Text = "状态：正在接收加速度数据";
                //将三轴加速度数据显示
                xTextBlock.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                yTextBlock.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                zTextBlock.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);
                System.Diagnostics.Debug.WriteLine(flag);
                //判断是否出现摇一摇操作
                if (Math.Abs(reading.AccelerationX) > 1.7d || Math.Abs(reading.AccelerationY) > 1.7d || Math.Abs(reading.AccelerationZ + 1) > 1.8d)
                {
                    //判断开启还是关闭LED
                    Judgeflag();
                    System.Diagnostics.Debug.WriteLine("shake" + flag);
                }
            });
        }
                
        private void Judgeflag()
        {
            //若开启，则关闭
            if (flag == true)
            {
                CaptureOperator.CloseTorch();
                flag = false;
                textBlock.Text = "Shake It ON!";
            }
            //若关闭，则开启
            else
            {
                CaptureOperator.OpenTorch();
                flag = true;
                textBlock.Text = "Shake It OFF!";
            }
        }

        async void OnAppResuming(object sender, object e)
        {
            if (flag == true)
            {
                // 等待初始化完成
                while (CaptureOperator.IsCaptureCreated == false)
                {
                    await Task.Delay(200);
                }
                CaptureOperator.OpenTorch();
            }
        }
    }

        /*
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            WebViewControl.Navigate(HomeUri);

            HardwareButtons.BackPressed += this.MainPage_BackPressed;
        }

        
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= this.MainPage_BackPressed;
        }

        /// <summary>
        /// 重写后退按钮按压事件以在 WebView (而不是应用程序)的返回栈中导航。
        /// </summary>
        private void MainPage_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (WebViewControl.CanGoBack)
            {
                WebViewControl.GoBack();
                e.Handled = true;
            }
        }

        private void Browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (!args.IsSuccess)
            {
                Debug.WriteLine("Navigation to this page failed, check your internet connection.");
            }
        }

        /// <summary>
        /// 在 WebView 的历史记录中向前导航。
        /// </summary>
        private void ForwardAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (WebViewControl.CanGoForward)
            {
                WebViewControl.GoForward();
            }
        }

        /// <summary>
        /// 导航到初始主页。
        /// </summary>
        private void HomeAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            WebViewControl.Navigate(HomeUri);
        }
        */

}