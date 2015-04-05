using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProcessController
{
    public delegate void AddCheckedListItemCallback(string s, bool b);
    public delegate void RemoveCheckedListItemCallback(string s);

    public partial class Form1 : Form
    {
        private bool allowRestart = true;
        private List<Process> processes = new List<Process>();
        
        public Form1()
        {
            InitializeComponent();
            CreateProcess();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CreateProcess();
        }

        private void CreateProcess()
        {
            Process process = Launch();
            processes.Add(process);
            AddToListBox(process.Id + ", " + process.ProcessName, true);
        }

        private void AddToListBox(string s, bool b)
        {
            if (checkedListBox1.InvokeRequired)
            {
                AddCheckedListItemCallback c = new AddCheckedListItemCallback(AddToListBox);
                this.Invoke(c, new object[] { s, b });
            }
            else
            {
                checkedListBox1.Items.Add(s, b);
            }
        }

        private void RemoveFromListBox(string s)
        {
            if (checkedListBox1.InvokeRequired)
            {
                RemoveCheckedListItemCallback c = new RemoveCheckedListItemCallback(RemoveFromListBox);
                this.Invoke(c, new object[] { s });
            }
            else
            {
                checkedListBox1.Items.Remove(s);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            allowRestart = false;
            IEnumerator enumerator = checkedListBox1.CheckedItems.GetEnumerator();
            List<string> itemsToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current.ToString();
                Process process = GetProcessByName(item);// processes.Find((Process p) => (p.Id + ", " + p.ProcessName).Equals(item));
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
                itemsToRemove.Add(item);
            }
            foreach (string itr in itemsToRemove)
            {
                checkedListBox1.Items.Remove(itr);
            }
        }

        Process GetProcessByName(string name)
        {
            return processes.Find((Process p) => (p.Id + ", " + p.ProcessName).Equals(name));
        }

        Process Launch()
        {
            Process process = new Process();
            process.StartInfo.FileName = "KeyLogTest.exe";
            process.EnableRaisingEvents = true;
            process.Start();
            //process.Disposed += LaunchIfCrashed;
            process.Exited += LaunchIfCrashed;
            return process;
        }

        void LaunchIfCrashed(object o, EventArgs e)
        {
            // exit code -1: Closed using process controller
            // exit ocde 0: Closed from outside

            if (!allowRestart)
            {   // Manual close from Process Controller (don't restart prcoess)
                allowRestart = true;
                //MessageBox.Show("Process killed from controller");
                return;
            }
            else
            {
               // MessageBox.Show("Process killed from outside");
                Process process = (Process)o;
                if (process.ExitCode == 0)
                {
                    RemoveFromListBox(process.Id + ", " + process.ProcessName);
                    CreateProcess();
                }
            }            
        }

        private void checkedListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Process p = GetProcessByName(checkedListBox1.SelectedItem.ToString());
            //MessageBox.Show(""+p.Id);
            MethodInfo[] methods;
        }

        private void checkedListBox1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Process p = GetProcessByName(checkedListBox1.SelectedItem.ToString());
            textBox1.Clear();
            textBox1.AppendText(String.Format("Memory: {0}, ", p.PrivateMemorySize64));
            textBox1.AppendText(String.Format("Threads: {0}, ", p.Threads.Count));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
