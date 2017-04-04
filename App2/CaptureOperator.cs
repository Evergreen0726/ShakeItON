using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.Media.MediaProperties;

namespace App2
{
    static class CaptureOperator
    {
        #region 私有字段
        static MediaCapture m_capture = null;
        static bool m_istorchOpened = false;
        static bool m_iscaptureCreated = false;
        #endregion

        #region 属性
        /// <summary>
        /// 指示摄像是否已打开
        /// </summary>
        public static bool IsTorchOpened
        {
            get { return m_istorchOpened; }
        }
        /// <summary>
        /// 指示MediaCapture是否已初始化
        /// </summary>
        public static bool IsCaptureCreated
        {
            get { return m_iscaptureCreated; }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 初始化捕捉对象
        /// </summary>
        public async static Task CreateCaptureAsync()
        {
            // 找出后置摄像头，一般闪光灯在后置摄像头上
            DeviceInformation backCapture = (from d in await GetCaptureDeviceseAsync() where d.EnclosureLocation.Panel == Panel.Back select d).FirstOrDefault();

            if (backCapture != null)
            {
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.VideoDeviceId = backCapture.Id; //设备ID
                settings.StreamingCaptureMode = StreamingCaptureMode.Video;
                settings.PhotoCaptureSource = PhotoCaptureSource.Auto;
                // 初始化
                m_capture = new MediaCapture();
                await m_capture.InitializeAsync(settings);
                m_iscaptureCreated = true;
            }
        }

        /// <summary>
        /// 获取摄像头设备列表（前置，后置摄像头）
        /// </summary>
        /// <returns></returns>
        private async static Task<DeviceInformation[]> GetCaptureDeviceseAsync()
        {
            var dvs = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return dvs.ToArray();
        }

        /// <summary>
        /// 清理捕捉对象
        /// </summary>
        /// <returns></returns>
        public static void CleanupCaptureAsync()
        {
            if (m_capture != null)
            {
                m_capture.Dispose();
                m_capture = null;
                m_iscaptureCreated = false;
            }
        }

        public static void OpenTorch()
        {
            // 开闪光灯
            var vdcontrol = m_capture.VideoDeviceController.TorchControl;
            if (vdcontrol.Supported)
            {
                vdcontrol.Enabled = true;
                m_istorchOpened = true;
            }
        }

        public static void CloseTorch()
        {
            // 关闭闪光灯
            var torch = m_capture.VideoDeviceController.TorchControl;
            if (torch.Supported)
            {
                torch.Enabled = false;
                m_istorchOpened = false;
            }
        }


        #endregion
    }
}
