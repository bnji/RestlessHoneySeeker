using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Library
{
    public static class WebcamControl
    {
        public static Bitmap GetWebCamImage(int id = 0)
        {
            Bitmap image = null;
            var webCameraControl = new WebEye.WebCameraControl();
            WebEye.WebCameraId camera = null;
            try
            {
                camera = (webCameraControl.GetVideoCaptureDevices() as List<WebEye.WebCameraId>)[id];
            }
            catch { }
            if (camera != null)
            {
                webCameraControl.StartCapture(camera);
                System.Threading.Thread.Sleep(2000);
                image = webCameraControl.GetCurrentImage();
                System.Threading.Thread.Sleep(250);
                webCameraControl.StopCapture();
            }
            return image;
        }
    }
}
