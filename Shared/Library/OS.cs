using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Library
{
    public enum EOSName
    {
        Windows95,
        Windows98,
        Windows98SE,
        WindowsMe,
        WindowsNT35,
        WindowsNT40,
        Windows2000,
        WindowsXP,
        WindowsVista,
        Windows7,
        Windows8,
        Uknown
    }

    public class OS
    {
        public static EOSName GetOSName()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            // Determine the platform.
            switch (osInfo.Platform)
            {
                // Platform is Windows 95, Windows 98, 
                // Windows 98 Second Edition, or Windows Me.
                case System.PlatformID.Win32Windows:

                    switch (osInfo.Version.Minor)
                    {
                        case 0:
                            return EOSName.Windows95;
                        case 10:
                            if (osInfo.Version.Revision.ToString() == "2222A")
                                return EOSName.Windows98SE;
                            else
                                return EOSName.Windows98;
                        case 90:
                            return EOSName.WindowsMe;
                    }
                    break;

                // Platform is Windows NT 3.51, Windows NT 4.0, Windows 2000,
                // or Windows XP.
                case System.PlatformID.Win32NT:

                    switch (osInfo.Version.Major)
                    {
                        case 3:
                            return EOSName.WindowsNT35;
                        case 4:
                            return EOSName.WindowsNT40;
                        case 5:
                            if (osInfo.Version.Minor == 0)
                                return EOSName.Windows2000;
                            else
                                return EOSName.WindowsXP;
                    }
                    break;
            }
            return EOSName.Uknown;
        }

        public static List<ProcessInfo> GetProcesses()
        {
            //var result = new StringBuilder();
            var result = new List<ProcessInfo>();
            var processList = Process.GetProcesses();
            foreach (Process p in processList)
            {
                result.Add(new ProcessInfo() { Name = p.ProcessName, PID = p.Id });
                //result.AppendLine("Name: " + p.ProcessName + ", PID: " + p.Id);// + ", Start Time: " + p.StartTime + ", CPU Time: " + p.TotalProcessorTime + ", Threads: " + p.Threads);
            }
            //return result.ToString();
            return result;
        }
    }

    public class ProcessInfo
    {
        public string Name { get; set; }
        public int PID { get; set; }
    }
}
