using ProgramHolder.objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgramHolder {
    public partial class ProcessBox : Form {

        Process p;

        String[] BadProcesses = { "taskhostw", "NvStreamUserAgent", "Taskmgr", "conhost", "nvxdsync", "dwm", "fontdrvhost", "winlogon", "csrss" };

        public ProcessBox() {
            InitializeComponent();
        }

        public Process ShowDialog(bool x) {
            this.ShowDialog();
            return p;
        }

        private void ProcessBox_Load(object sender, EventArgs e) {
            var currentSessionID = Process.GetCurrentProcess().SessionId;
            foreach (Process item in (from item in Process.GetProcesses() where item.SessionId == currentSessionID select item).ToArray()) {
                if (!(BadProcesses.Contains(item.ProcessName)) && (!String.IsNullOrEmpty(item.MainWindowTitle))) {
                    this.flowLayoutPanel1.Controls.Add(new ProcessHolder(item));
                }
            }
        }

        public void ItemChecked(object sender) {
            p = ((ProcessHolder)sender).Process;

            foreach (ProcessHolder item in this.flowLayoutPanel1.Controls) {
                if(item != ((ProcessHolder)sender)) {
                    item.CheckBox.Checked = false;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void buttonRefresh_Click(object sender, EventArgs e) {
            this.flowLayoutPanel1.Controls.Clear();
            var currentSessionID = Process.GetCurrentProcess().SessionId;
            foreach (Process item in (from item in Process.GetProcesses() where item.SessionId == currentSessionID select item).ToArray()) {
                if (!(BadProcesses.Contains(item.ProcessName)) && (!String.IsNullOrEmpty(item.MainWindowTitle))) {
                    this.flowLayoutPanel1.Controls.Add(new ProcessHolder(item));
                }
            }
        }
    }
}
