using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginManager;
using System.Diagnostics;
using WebEye;


namespace PluginDemo
{
    public class Client : IPluginClient
    {
        public object Execute(string parameters)
        {
            MessageBox.Show("Plugin execution works!");
            return null;
            //return GetWebCamImage();
        }

        public void Initialize()
        {
            // do something if needed on start...
            //MessageBox.Show("Init");
            //throw new NotImplementedException();
        }

        private Bitmap GetWebCamImage()
        {
            Bitmap image = null;
            var webCameraControl = new WebCameraControl();
            WebCameraId camera = null;
            foreach (WebCameraId c in webCameraControl.GetVideoCaptureDevices())
            {
                if (c != null)
                {
                    camera = c;
                    break;
                }
            }
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

        public void Kill()
        {
            //MessageBox.Show("Plugin kill method not implemented!");
            // do something if needed on exit
        }
    }
}
