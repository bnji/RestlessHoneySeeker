using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Library
{
    public static class SystemPowerUtils
    {
        // http://www.computerhope.com/shutdown.htm
        public static void Shutdown()
        {
            RunProcess("shutdown", "/s /f /t 0");
        }

        public static void Restart()
        {
            RunProcess("shutdown", "/r /f /t 0");
        }

        // http://www.codeproject.com/Tips/480049/Shut-Down-Restart-Log-off-Lock-Hibernate-or-Sleep
        public static void Logoff()
        {
            ExitWindowsEx(0, 0);
        }

        public static void LockComputer()
        {
            LockWorkStation();
        }

        public static void Hibernate()
        {
            SetSuspendState(true, true, true);
        }

        public static void Sleep()
        {
            SetSuspendState(false, true, true);
        }

        static void RunProcess(string cmd, string parameters)
        {
            var psi = new ProcessStartInfo(cmd, parameters);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("user32")]
        public static extern void LockWorkStation();

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
    }
}
