using Client.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class FormStoppedWorking : Form
    {
        public FormStoppedWorking()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormStoppedWorking_Load(object sender, EventArgs e)
        {
            SetupWorker();
        }

        private void SetupWorker()
        {
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerAsync();
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Tick += t_Tick;
            t.Interval = 1000;
            t.Enabled = true;
        }

        int c = 0;

        void t_Tick(object sender, EventArgs e)
        {
            c++;
            if (c == 10)
                this.Close();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            int val = e.ProgressPercentage;
            if (val < 100)
            {
                progressBar1.Value = val;
            }
            else
            {
                i = 0;
                progressBar1.Value = 0;
                //bw.RunWorkerAsync();
                SetupWorker();
            }
        }

        private static int i = 0;

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(50);
                bw.ReportProgress(i);
            }
        }

        BackgroundWorker bw;
    }
}
