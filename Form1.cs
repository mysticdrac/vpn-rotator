using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.IO;
namespace vpnrotator
{
    public partial class Form1 : Form
    {
        Process p;
        ProcessStartInfo s;
        StringBuilder sortoutput;
        StringBuilder str;
        ManualResetEvent _event = new ManualResetEvent(false);
        List<string> vpn;
        Threads t;
        public Form1()
        {
            InitializeComponent();
            sortoutput = new StringBuilder();
            str = new StringBuilder();
             DirectoryInfo dinfo = new DirectoryInfo(Environment.CurrentDirectory+"\\vpn\\");
            FileInfo[] finfos=  dinfo.GetFiles();
            vpn = new List<string>();
            foreach (FileInfo finfo in finfos) {
                if(finfo.Extension.ToLower().Equals(".ovpn"))
                                    vpn.Add(finfo.Name);          
            }
        }

        void ErrordataHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            if(!_event.WaitOne(0))
                _event.Set();
        }


        void SortOutputHandler(object sendingProcess,
        DataReceivedEventArgs outLine){
          if (this.textBox1.InvokeRequired) { textBox1.BeginInvoke(new DataReceivedEventHandler(SortOutputHandler), new[] { sendingProcess, outLine }); }
            else
          {
              try
              {
                  this.textBox1.AppendText(outLine.Data + "\n");
              }
              catch (Exception) { }
            }
        
        }
        void startprocess() {
            EventWaitHandle resetEvent;
            for (int i = 0; i < vpn.Count; i++)
            {

                s = new ProcessStartInfo();
                s.FileName = "openvpn";
                s.Arguments = "--config vpn/" + vpn[i] + " --service MyEventName 0";
                s.CreateNoWindow = true;
                s.RedirectStandardInput = true;
                s.RedirectStandardOutput = true;
                s.RedirectStandardError = true;
                s.UseShellExecute = false;
                p = Process.Start(s);
                p.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
                p.ErrorDataReceived += new DataReceivedEventHandler(ErrordataHandler);
                p.BeginOutputReadLine();
                /*
                while (!p.HasExited)
                {
                    Application.DoEvents(); // This keeps your form responsive by processing events
                }
                 */
                t = new Threads(this);

                int time = 0;
                this.Invoke((MethodInvoker)delegate { time = int.Parse(this.comboBox1.Text); });

                new Thread(new ParameterizedThreadStart(t.wait))
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest

                }.Start((time * 1000));

                // s.StandardInput.Close();

                _event.WaitOne();
                try
                {
                    resetEvent = EventWaitHandle.OpenExisting("MyEventName");
                    resetEvent.Set();
                    resetEvent.Close();//.Reset();
                }
                catch (Exception) { }
                 p.Close();
                Thread.Sleep(5000);
                _event.Reset();

            }
            p.Close();
            button1.Invoke((MethodInvoker)delegate { button1.Text = "Start"; });
            

        }

        internal void setevent()
        {
            if (!_event.WaitOne(0))
                _event.Set();

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = 0;
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.ToLower().Equals("start"))
            {
                new Thread(new ThreadStart(startprocess)).Start();
                button1.Text = "Stop";
            }
            else {
                if (p != null) {
                    p.Close();
                    p.Dispose();
                }
                button1.Text = "Start";
            
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (p != null)
            {
                p.Close();
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(13)) {
                
                p.StandardInput.WriteLine(str.ToString());
                str = new StringBuilder();
            }
            str.Append(e.KeyChar);
        }
    }
}
