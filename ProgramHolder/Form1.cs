using ProgramHolder.twitch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgramHolder {
    public partial class Form1 : Form {

        ContextMenuStrip tabpagemenu = new ContextMenuStrip();

        ArtLogger.ArtLogger Logger = Program.ArtLogger;
        TwitchClient TwitchClient;
        RichTextBox TwitchChat;
        /// <summary>
        /// Contains program [bool]
        /// Process object [Process]
        /// </summary>
        object[] _panelTag = { false, new Process() };

        public Form1() {
            AllocConsole();

            InitializeComponent();

            Application.ApplicationExit += Application_ApplicationExit;

            this.panel1.Tag = _panelTag;

            SetupTabMenu();

            this.tabControl1.TabPages.Add(CreateTabPage());
            this.tabControl1.SelectedTab = this.tabControl1.TabPages[this.tabControl1.TabCount - 1];
            this.tabControl1.MouseClick += TabControl1_MouseClick;

            var y = new TabPage("+");
            y.Name = "tabAdd";
            y.BackColor = Color.DarkGray;

            this.tabControl1.TabPages.Add(y);
        }

        private void TabControl1_MouseClick(object sender, MouseEventArgs e) {
            for (int i = 0; i < this.tabControl1.TabCount; i++) {
                if (this.tabControl1.GetTabRect(i).Contains(e.Location)) {
                    this.tabpagemenu.Tag = this.tabControl1.TabPages[i];
                    if (e.Button == MouseButtons.Right) {
                        if (this.tabControl1.TabPages[i].Text != "+") {
                            this.tabpagemenu.Show(this.tabControl1, e.Location);
                        }
                    } else {
                        CheckAdd();
                    }
                }
            }
        }

        private void Application_ApplicationExit(object sender, EventArgs e) {
            try {
                TwitchClient.StopListener(TwitchClient);
            } catch (Exception) {
                return;
            }
        }

        private void SetupTabMenu() {
            ToolStripButton tsClose = new ToolStripButton("&Close", null, TsClose_Click, "tsClose");
            Logger.Write("tsClose Created", ArtLogger.Logging.LogLevel.Debug);

            ToolStripButton tsBreaker = new ToolStripButton("---------", null, null, "tsBreaker");
            Logger.Write("tsBreaker Created", ArtLogger.Logging.LogLevel.Debug);

            ToolStripButton tsNotes = new ToolStripButton("Set to Notes", null, TsNotes_Click, "tsNotes");
            Logger.Write("tsNotes Created", ArtLogger.Logging.LogLevel.Debug);

            ToolStripButton tsBlank = new ToolStripButton("", null, null, "tsBlank");
            Logger.Write("tsBlank Created", ArtLogger.Logging.LogLevel.Debug);

            ToolStripButton tsMem = new ToolStripButton("Memory Listener", null, TsMem_Click, "tsMem");
            Logger.Write("tsMem Created", ArtLogger.Logging.LogLevel.Debug);

            ToolStripButton tsTwitch = new ToolStripButton("Twitch Chat", null, TsTwitch_Click, "tsTwitch");
            Logger.Write("tsTwitch Created", ArtLogger.Logging.LogLevel.Debug);

            var x = new ToolStripItem[] { tsClose, tsBreaker, tsNotes, tsMem, tsTwitch };

            this.tabpagemenu.Items.AddRange(x);

            String y = "";

            foreach (ToolStripButton item in x) {
                y += item.Name + " ";
            }

            Logger.Write(y + "added to Context strip", ArtLogger.Logging.LogLevel.Debug);

            this.tabpagemenu.Width = 15;
        }

        #region Twitch

        private void TsTwitch_Click(object sender, EventArgs e) {
            TabPage x = (TabPage)this.tabpagemenu.Tag;
            //if (!(0 < (from TabPage tab in this.tabControl1.TabPages where (0 < (from RichTextBox box in tab.Controls where box.Name == "TwitchChat" select box).Count()) select tab).Count())) {
            if (!(0 < x.Controls.Count)) {
                x.Controls.Clear();
                Logger.Write("Cleared controls of Tab " + x.Name, ArtLogger.Logging.LogLevel.Info);

                this.TwitchChat = new RichTextBox();
                this.TwitchChat.Name = "TwitchChat";
                this.TwitchChat.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
                this.TwitchChat.Dock = DockStyle.Fill;
                this.TwitchChat.Font = new Font("Verdana", 9);
                this.TwitchChat.ForeColor = Color.White;
                this.TwitchChat.BackColor = Color.Black;
                this.TwitchChat.MouseDoubleClick += TwitchChat_MouseDoubleClick;
                this.TwitchChat.ReadOnly = true;
                this.TwitchChat.WordWrap = true;
                this.TwitchChat.Disposed += TwitchChat_Disposed;

                x.Controls.Add(this.TwitchChat);
                Logger.Write("Added TwitchChat to Tab " + x.Name, ArtLogger.Logging.LogLevel.Info);

                //this.TwitchClient = new twitch.TwitchClient("mrep1cman", "mrep1cman");
                String chan = new ChannelBox("Input channel").ShowDialog(true);
                String nick = new ChannelBox("Input nickname").ShowDialog(true);
                this.TwitchClient = new TwitchClient(chan, nick);
                this.TwitchClient.OnChatConnected += TwitchClient_OnChatConnected;
                this.TwitchClient.OnDataRecieved += TwitchClient_OnDataRecieved;

                this.TwitchClient.Connect();

                this.TwitchClient.StartListener(TwitchClient);
            } else {
                Logger.Write("Tab " + x.Name + " already contains " + x.Controls[0].GetType(), ArtLogger.Logging.LogLevel.Warning);
            }
        }

        private void TwitchChat_Disposed(object sender, EventArgs e) {
            this.TwitchClient.StopListener(this.TwitchClient);
            this.TwitchClient = null;
        }

        private void TwitchClient_OnChatConnected(object sender, string[] data, [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int lineNum = 0) {
            this.tabpagemenu.Invoke(new Action(delegate () {
                var x = (from TabPage tab in this.tabControl1.TabPages where (0 < (from RichTextBox box in tab.Controls where box.Name == "TwitchChat" select box).Count()) select tab).ElementAt(0);
                x.Name = "#" + data[1];
                x.Text = x.Name;
            }));
        }

        private void TwitchChat_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left && ModifierKeys.HasFlag(Keys.Control)) {
                this.TwitchChat.Text = "";
            }
        }

        String[] _Keywords = new String[] { "JOINED" };
        String[] _Bots = new String[] { "nightbot" };

        private void TwitchClient_OnDataRecieved(object sender, string data, [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int lineNum = 0) {
            try {
                string messageParser = data.ToString();
                string[] message = messageParser.Split(':');
                string[] preamblex = message[1].Split(' ');

                string tochat = "";
                string[] sendingUser = preamblex[0].Split('!');
                List<String[]> preambleFix = new List<String[]>();

                int g = 0;

                if (1 < (from String y in preamblex where y.StartsWith(sendingUser[0]) select y).Count()) {
                    for (int i = 0; i < preamblex.Count(); i++) {
                        if (preamblex[i].StartsWith(sendingUser[0])) {
                            g++;
                            if (g == 2) {
                                preambleFix.Add(new String[] { preamblex[i - 2], preamblex[i - 1] });
                            }
                        } else if (preamblex[i] == preamblex[preamblex.Length - 1]) {
                            preambleFix.Add(new String[] { preamblex[preamblex.Length - 2], preamblex[preamblex.Length - 1] });
                        }
                    }
                } else {
                    preambleFix.Add(preamblex);
                }

                // This means it's a message to the channel.  Yes, PRIVMSG is IRC for messaging a channel too
                foreach (String[] preamble in preambleFix) {

                    if (preamble[1] == "PRIVMSG") {
                        tochat = sendingUser[0] + ": " + message[2];
                    }
                    // A user joined.
                    else if (preamble[1] == "JOIN") {
                        tochat = "JOINED: " + sendingUser[0];
                    }
                    Logger.Write("[TOCHAT] " + tochat.Replace("\r\n", string.Empty), ArtLogger.Logging.LogLevel.Debug);
                    this.TwitchChat.Invoke(new Action(delegate () {
                        if ((!(String.IsNullOrWhiteSpace(tochat)) && (!(String.IsNullOrEmpty(tochat))))) {
                            tochat = tochat.Replace(Environment.NewLine, string.Empty);
                            String[] msgData = tochat.Split(": ".ToCharArray());
                            if (!_Keywords.Contains(msgData[0])) {
                                RichTextBoxExtensions.AppendText(this.TwitchChat, msgData[0], Color.Red, this.TwitchChat.Font);
                                String msg = string.Empty;
                                for (int i = 2; i < msgData.Length; i++) { //rebuild message
                                    if (!(msgData[i] == msgData[msgData.Length - 1])) {
                                        msg += msgData[i] + " ";
                                    } else {
                                        msg += msgData[i];
                                    }
                                }
                                this.TwitchChat.AppendText(": " + msg + Environment.NewLine);
                            } else if (_Bots.Contains(msgData[0])) {
                                RichTextBoxExtensions.AppendText(this.TwitchChat, tochat + Environment.NewLine, Color.Cyan, this.TwitchChat.Font);
                            } else {
                                RichTextBoxExtensions.AppendText(this.TwitchChat, tochat + Environment.NewLine, Color.Magenta, this.TwitchChat.Font);
                            }
                        }
                        this.TwitchChat.SelectionStart = this.TwitchChat.Text.Length-1;
                        this.TwitchChat.ScrollToCaret();
                    }
                    ));
                }

                
            }
            // Catches an error. No idea what causes the error but there we go
            catch (Exception e) {
                Logger.Write(String.Format("{0}: {1} .. {2}", callingFilePath, lineNum, e), ArtLogger.Logging.LogLevel.Error);
            }
        }

        #endregion

        private void TsMem_Click(object sender, EventArgs e) {
            var x = (TabPage)this.tabpagemenu.Tag;

            if (!(0 < x.Controls.Count)) {

            } else {
                Logger.Write("Tab " + x.Name + " already contains " + x.Controls[0].GetType(), ArtLogger.Logging.LogLevel.Warning);
            }
        }

        private void TsClose_Click(object sender, EventArgs e) {
            var x = (TabPage)this.tabpagemenu.Tag;
            
            if (this.tabControl1.TabCount > 2) {
                this.tabControl1.TabPages.Remove(x);
                Logger.Write("Removed tab " + x.Name);
            }

        }

        private void TsNotes_Click(object sender, EventArgs e) {
            TabPage x = (TabPage)this.tabpagemenu.Tag;

            if (!(0 < x.Controls.Count)) {

                x.Controls.Clear();
                Logger.Write("Cleared controls of Tab " + x.Name, ArtLogger.Logging.LogLevel.Info);

                RichTextBox rt = new RichTextBox();
                rt.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
                rt.Dock = DockStyle.Fill;

                x.Controls.Add(rt);
                Logger.Write("Added richtextbox to Tab " + x.Name, ArtLogger.Logging.LogLevel.Info);
            } else {
                Logger.Write("Tab " + x.Name + " already contains " + x.Controls[0].GetType(), ArtLogger.Logging.LogLevel.Warning);
            }
        }

        #region TabPage

        private TabPage CreateTabPage() {
            TabPage x = new TabPage();

            //x.ContextMenuStrip = this.tabpagemenu;

            return x;
        }

        void CheckAdd() {
            if (((TabPage)this.tabpagemenu.Tag).Text == "+") {
                var z = this.tabControl1.SelectedIndex;
                this.tabControl1.TabPages.Add(CreateTabPage());


                foreach (TabPage item in this.tabControl1.TabPages) {
                    if (item.Text == "+") {
                        this.tabControl1.TabPages.Remove(item);
                    }
                }

                this.tabControl1.TabPages.Add(new TabPage("+"));
                Logger.Write("New tab added");
                this.tabControl1.SelectedIndex = z;
            }

        }

        #endregion

        #region ProcessPanel

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);


        private void panel1_Click(object sender, EventArgs e) {
            object[] panelTag = ((object[])((Panel)sender).Tag);

            if ((bool)panelTag[0] == false) {
                var x = new ProcessBox().ShowDialog(true);
                if (x != null) {
                    panelTag[1] = x;
                    panelTag[0] = true;


                    MoveWindow(((Process)panelTag[1]).MainWindowHandle, 0, 0, 500, 500, true);
                    SetParent(((Process)panelTag[1]).MainWindowHandle, panel1.Handle);

                    ((Process)panelTag[1]).EnableRaisingEvents = true;
                    ((Process)panelTag[1]).Exited += Process_Exited;
                }

            }
        }

        private void Process_Exited(object sender, EventArgs e) {
            object[] panelTag = ((object[])(this.panel1.Tag));
            panelTag[0] = false;
            Logger.Write(String.Format("Process '{0}' ({1}) Exited", ((Process)sender).ProcessName, ((Process)sender).Id), ArtLogger.Logging.LogLevel.Info);
        }

        #endregion

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            object[] panelTag = ((object[])(this.panel1.Tag));
            ((Process)panelTag[1]).Close();
        }
    }
}
