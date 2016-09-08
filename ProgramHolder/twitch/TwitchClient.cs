using System;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace ProgramHolder.twitch {
    class TwitchClient {

        static ArtLogger.ArtLogger Logger = Program.ArtLogger;

        public delegate void DataRecievedHandler(object sender, String data, [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int lineNum = 0);
        public delegate void ChatConnectedHandler(object sender, String[] data, [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int lineNum = 0);
        /// <summary>
        /// Sends the current Nickname & Connected Channel
        /// </summary>
        public event ChatConnectedHandler OnChatConnected;
        /// <summary>
        /// Sends the data recieved from the chat
        /// </summary>
        public event DataRecievedHandler OnDataRecieved;

        static Int32 Port { get { return 6667; } }
        TcpClient _Client = new TcpClient("irc.twitch.tv", Port);
        TcpClient Client { get { return _Client; } }
        public Thread Listener { get; set; }

        String Channel { get; set; }
        String Nickname { get; set; }
        String OAuth { get; set; }

        byte[] data;

        NetworkStream Stream { get { return Client.GetStream(); } }

        public TwitchClient(String chan, String nick) {
            this.Channel = chan;
            this.Nickname = nick;
            this.OAuth = "j3y24zpjyxx1kx6fr1c6wjs8cyepyp";
        }

        public TwitchClient(String chan, String nick, String oauth) : this(chan, nick) {
            this.OAuth = oauth;
        }

        public void Connect() {
            // Send the message to the connected TcpServer. 

            Byte[] login = Encoding.ASCII.GetBytes(String.Format("PASS oauth:{0}\r\nNICK {1}\r\n", this.OAuth, this.Nickname));
            this.Stream.Write(login, 0, login.Length);
            Logger.Write("Login Details sent", ArtLogger.Logging.LogLevel.Sent);
            //Logger.Write("oauth: " + this.OAuth, ArtLogger.Logging.LogLevel.Debug);
            Logger.Write("oauth: Please don't hack me", ArtLogger.Logging.LogLevel.Debug);
            Logger.Write("nick: " + this.Nickname, ArtLogger.Logging.LogLevel.Debug);

            // Receive the TcpServer.response.
            // Buffer to store the response bytes.
            this.data = new Byte[512];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = Stream.Read(this.data, 0, this.data.Length);
            responseData = Encoding.ASCII.GetString(this.data, 0, bytes);
            String[] x = responseData.Split("\r\n".ToCharArray());
            Logger.Write(String.Format("WELCOME: {0}", x[0]), ArtLogger.Logging.LogLevel.Received);
            foreach (String item in (from String item in x where item != x[0] select item)) {
                Logger.Write(String.Format("{0}", item), ArtLogger.Logging.LogLevel.Received);
            }

            // send message to join channel

            string joinstring = "JOIN " + "#" + this.Channel;
            Byte[] join = Encoding.ASCII.GetBytes(joinstring + "\r\n");
            this.Stream.Write(join, 0, join.Length);
            Logger.Write(String.Format("Requested to join channel #", this.Channel), ArtLogger.Logging.LogLevel.Sent);
            Logger.Write(joinstring, ArtLogger.Logging.LogLevel.Debug);

            // PMs the channel to announce that it's joined and listening
            // These three lines are the example for how to send something to the channel

            string announcestring = this.Channel + "!" + this.Channel + "@" + this.Channel + ".tmi.twitch.tv PRIVMSG " + this.Channel + " BOT ENABLED";
            Byte[] announce = Encoding.ASCII.GetBytes(announcestring + "\r\n");
            this.Stream.Write(announce, 0, announce.Length);

            // Lets you know its working
            OnChatConnected(this, new String[] { this.Nickname, this.Channel });
        Logger.Write("Connected to twitch chat #" + this.Channel);
        }

        public void StartListener(TwitchClient c) {
            c.Listener = new Thread(() => ListenerThread(c));
            c.Listener.Start();
        }

        void ListenerThread(TwitchClient c) {
            while (c.Listener.IsAlive) {
                RecieveData();
            }
        }

    public void StopListener(TwitchClient c) {
            c.Listener.Abort();
            this.Stream.Close();
            this.Client.Close();
        }

        void RecieveData([CallerFilePath] string callingFilePath = "", [CallerLineNumber] int lineNum = 0) {
            byte[] myReadBuffer = new byte[1024];
            StringBuilder myCompleteMessage = new StringBuilder();
            int numberOfBytesRead = 0;

            // Incoming message may be larger than the buffer size.
            do {
                try {
                    numberOfBytesRead = this.Stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                } catch (Exception e) {
                    Logger.Write(String.Format("{0}: {1} .. {2}", callingFilePath, lineNum, e), ArtLogger.Logging.LogLevel.Error);
                }

                myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
            }

            // when we've received data, do Things

            while (this.Stream.DataAvailable);
            {
                Logger.Write(myCompleteMessage.ToString().Replace("\r\n", string.Empty), ArtLogger.Logging.LogLevel.Received);
                switch (myCompleteMessage.ToString()) {
                    // Every 5 minutes the Twitch server will send a PING, this is to respond with a PONG to keepalive

                    case "PING :tmi.twitch.tv\r\n":
                        try {
                            Byte[] say = Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
                            this.Stream.Write(say, 0, say.Length);
                            Logger.Write("Pong!", ArtLogger.Logging.LogLevel.Debug);
                        } catch (Exception e) {
                            Logger.Write(String.Format("{0}: {1} .. {2}", callingFilePath, lineNum, e), ArtLogger.Logging.LogLevel.Error);
                        }
                        break;

                    // If it's not a ping, it's probably something we care about.  Try to parse it for a message.
                    default:
                        OnDataRecieved(this, myCompleteMessage.ToString());
                        Logger.Write("Data not a ping!", ArtLogger.Logging.LogLevel.Received);
                        break;
                }
            }
        }

    }
}
