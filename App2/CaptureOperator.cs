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
        #region ˽���ֶ�
        static MediaCapture m_capture = null;
        static bool m_istorchOpened = false;
        static bool m_iscaptureCreated = false;
        #endregion

        #region ����
        /// <summary>
        /// ָʾ�����Ƿ��Ѵ�
        /// </summary>
        public static bool IsTorchOpened
        {
            get { return m_istorchOpened; }
        }
        /// <summary>
        /// ָʾMediaCapture�Ƿ��ѳ�ʼ��
        /// </summary>
        public static bool IsCaptureCreated
        {
            get { return m_iscaptureCreated; }
        }
        #endregion

        #region ����
        /// <summary>
        /// ��ʼ����׽����
        /// </summary>
        public async static Task CreateCaptureAsync()
        {
            // �ҳ���������ͷ��һ��������ں�������ͷ��
            DeviceInformation backCapture = (from d in await GetCaptureDeviceseAsync() where d.EnclosureLocation.Panel == Panel.Back select d).FirstOrDefault();

            if (backCapture != null)
            {
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.VideoDeviceId = backCapture.Id; //�豸ID
                settings.StreamingCaptureMode = StreamingCaptureMode.Video;
                settings.PhotoCaptureSource = PhotoCaptureSource.Auto;
                // ��ʼ��
                m_capture = new MediaCapture();
                await m_capture.InitializeAsync(settings);
                m_iscaptureCreated = true;
            }
        }

        /// <summary>
        /// ��ȡ����ͷ�豸�б�ǰ�ã���������ͷ��
        /// </summary>
        /// <returns></returns>
        private async static Task<DeviceInformation[]> GetCaptureDeviceseAsync()
        {
            var dvs = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return dvs.ToArray();
        }

        /// <summary>
        /// ����׽����
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
            // �������
            var vdcontrol = m_capture.VideoDeviceController.TorchControl;
            if (vdcontrol.Supported)
            {
                vdcontrol.Enabled = true;
                m_istorchOpened = true;
            }
        }

        public static void CloseTorch()
        {
            // �ر������
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
