using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ProgramHolder.objects {
    public partial class ProcessHolder : UserControl {

        ArtLogger.ArtLogger Logger = Program.ArtLogger;

        public bool IsChecked { get; set; }
        public Process Process { get; set; }

        public ProcessHolder(Process p) {
            Logger.Write(String.Format("Using Process {0} with ID {1}", p.ProcessName, p.Id), ArtLogger.Logging.LogLevel.Debug);
            InitializeComponent();

            Process = p;

            Logger.Write(string.Format("Using Icon image of {0}", p.MainModule.FileName));
            this.pictureBox1.Image = Icon.ExtractAssociatedIcon(p.MainModule.FileName).ToBitmap();
            this.labelName.Text = p.ProcessName;
        }

        public ProcessHolder() {
            Logger.Write("Using test ProcessHolder", ArtLogger.Logging.LogLevel.Debug);
            InitializeComponent();

            this.pictureBox1.BackColor = Color.Green;
            this.labelName.Text = "TEST";
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e) {
            this.IsChecked = ((CheckBox)sender).Checked;
            Logger.Write(String.Format("Process Holder {0}: IsChecked={1}", this.labelName.Text, this.IsChecked), ArtLogger.Logging.LogLevel.Info);

            ((ProcessBox)this.Parent.Parent).ItemChecked(this);
        }
    }
}
