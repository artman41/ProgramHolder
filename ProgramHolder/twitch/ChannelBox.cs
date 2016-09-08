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

namespace ProgramHolder.twitch {
    public partial class ChannelBox : Form {

        bool _isSelected = false;
        bool isSelected {
            get { return _isSelected;  } set { _isSelected = value; textboxCheck(); }
        }

        public ChannelBox(String x) {
            InitializeComponent();
            this.Text = x;
            textboxCheck();
        }

        public String ShowDialog(bool x) {
            this.ShowDialog();
            return this.textBox1.Text;
        }

        private void buttonOK_Click(object sender, EventArgs e) {
            if (!this.textBox1.Text.Contains(" ")) {
                this.Close();
            }
        }

        private void textBox1_Click(object sender, EventArgs e) {
            this.isSelected = true;
        }

        private void ChannelBox_Click(object sender, EventArgs e) {
            this.isSelected = false;
        }

        void textboxCheck() {
            if (this.isSelected) {
                this.textBox1.Text = "";
            } else {
                this.textBox1.Text = this.Text;
            }
        }
    }
}
