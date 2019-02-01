
using System.Threading;
using MightyPecoBot.Network;
namespace MightyPecoBot
{
    class TwitchBot
    {

        public const string URL = "irc.chat.twitch.tv";

        //FIXME: Be aware of port, may change with SSL and Websockets
        public const int PORT = 6667;
        public string Username { get; }
        public string Channel { get; }

        public IClientSocket Socket;

        public volatile bool Running = false;

        public Thread ReceivingThread;
        public TwitchBot(string username, string channel)
        {
            Username = username;
            Channel = channel;
            Socket = new TCPClientSocket(URL, PORT);
            ReceivingThread = new Thread(ReceiveData);
        }


        public void Connect(string oauth)
        {
            BotLogger.LogDebug("Connecting!");
            Socket.Connect();
            SendToIRC(IRCSymbols.FormatOAuth(oauth));
            SendToIRC(IRCSymbols.FormatUsername(Username));
            SendToIRC(IRCSymbols.FormatJoin(Channel));
            this.Running = true;
            ReceivingThread.Start();
        }
        public void Debug()
        {
            BotLogger.LogDebug("Sending HelloWorld!");
            SendToIRC(IRCSymbols.FormatChannelMessage(Channel, "HelloWorld!"));
        }

        public void SendToIRC(string message)
        {
            if (!message.ToLower().Contains("oauth"))
                BotLogger.LogMessage("Sending to IRC: " + message);
            Socket.Send(message);
        }
        public void SendToChannel(string message)
        {
            BotLogger.LogMessage("Sending to Channel: " + message);
            string data = IRCSymbols.PRIVMSG + "#" + Channel + " :" + message;
            Socket.Send(data);
        }

        private void ReceiveData()
        {
            while (Running)
            {
                string data = null;
                data = Socket.Receive();
                if (data != null)
                {
                    // do switch a case to decide if its worth to log or not
                    BotLogger.LogDebug(data);
                }
            }
        }
    }
}