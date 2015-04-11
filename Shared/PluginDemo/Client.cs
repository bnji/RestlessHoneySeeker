using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginManager;
using System.Diagnostics;


namespace PluginDemo
{
    public class Client : IPluginClient
    {
        public object Execute()
        {
            var result = new StringBuilder();
            var processList = Process.GetProcesses();
            foreach (Process p in processList)
            {
                result.AppendLine("Name: " + p.ProcessName + ", PID: " + p.Id);// + ", Start Time: " + p.StartTime + ", CPU Time: " + p.TotalProcessorTime + ", Threads: " + p.Threads);
            }
            return result.ToString();
        }

        public void Initialize()
        {
            // do something if needed on start...
            //MessageBox.Show("Init");
            //throw new NotImplementedException();
        }
    }
}
