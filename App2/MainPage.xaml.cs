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
        Accelerometer acc;
        private uint reportinterval;

        bool flag = false;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            App.Current.Resuming += OnAppResuming;
            acc = Accelerometer.GetDefault();
 
            if (acc != null)
            {
                // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
                // This value will be used later to activate the sensor.
                uint minReportInterval = acc.MinimumReportInterval;
                reportinterval = 180;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            button.IsEnabled = true;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (button.IsEnabled)
            {
                Window.Current.VisibilityChanged -= new WindowVisibilityChangedEventHandler(VisibilityChanged);
                acc.ReadingChanged -= new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                // Restore the default report interval to release resources while the sensor is not in use
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
                // Establish the report interval
                acc.ReportInterval = reportinterval;

                Window.Current.VisibilityChanged += new WindowVisibilityChangedEventHandler(VisibilityChanged);
                acc.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);

                button.IsEnabled = false;
            }
        }
        
        async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;
                statusTextBlock.Text = "状态：正在接收加速度数据";
                xTextBlock.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                yTextBlock.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                zTextBlock.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);
                System.Diagnostics.Debug.WriteLine(flag);

                if (Math.Abs(reading.AccelerationX) > 1.7d || Math.Abs(reading.AccelerationY) > 1.7d || Math.Abs(reading.AccelerationZ + 1) > 1.8d)
                {
                    Judgeflag();
                    System.Diagnostics.Debug.WriteLine("shake" + flag);
                }
            });
        }
                
        private void Judgeflag()
        {
            if (flag == true)
            {
                CaptureOperator.CloseTorch();
                flag = false;
                textBlock.Text = "Shake It ON!";
            }
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
        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            WebViewControl.Navigate(HomeUri);

            HardwareButtons.BackPressed += this.MainPage_BackPressed;
        }

        /// <summary>
        /// 在离开此页时调用。
        /// </summary>
        /// <param name="e">描述如何导航此页的事件数据。</param>
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