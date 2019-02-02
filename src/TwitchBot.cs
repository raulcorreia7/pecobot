
using System.Text.RegularExpressions;
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
            BotLogger.LogDebug("[ >>Connecting! ]");
            Socket.Connect();
            SendToIRC(IRCSymbols.FormatOAuth(oauth));
            SendToIRC(IRCSymbols.FormatUsername(Username));
            SendToIRC(IRCSymbols.FormatJoin(Channel));
            RequestTwitchMembershipStateEvents();
            this.Running = true;
            ReceivingThread.Start();
        }
        public void Debug()
        {
            BotLogger.LogDebug("[ >>Sending HelloWorld! ]");
            SendToIRC(IRCSymbols.FormatChannelMessage(Channel, "HelloWorld!"));
        }

        public void SendToIRC(string message)
        {
            if (!message.ToLower().Contains("oauth"))
                BotLogger.LogMessage(message);
            Socket.Send(message);
        }
        public void SendToChannel(string message)
        {
            BotLogger.LogMessage(message);
            string data = IRCSymbols.FormatChannelMessage(Channel, message);
            Socket.Send(data);
        }

        private void RequestTwitchMembershipStateEvents()
        {
            BotLogger.LogDebug("Requesting Membership capabilities.");
            string data = "CAP REQ :twitch.tv/membership";
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
                    /*  Parse
                        decide what to do with data

                    */
                    BotLogger.LogDebug(data);
                    if (Regex.Match(data, IRCSymbols.PING).Success)
                    {
                        SendToIRC("PONG :tmi.twitch.tv");
                    }
                    else
                    {
                        if (Regex.Match(data, IRCSymbols.PRIVMSG).Success)
                        {
                            Match match = Regex.Match(data, @":(\w+)!.+:(.+)$");
                            if (match.Success)
                            {
                                string username = match.Groups[1].Value;
                                string message = match.Groups[2].Value;
                                BotLogger.LogMessage($"<{username}> {message}");
                            }
                        }
                    }
                }
            }
        }


        /*
            Twitch commands implementation
         */

        /**
            Bans a user from the channel
         */
        public void BanUser(string username)
        {
            string data = $"{IRCSymbols.Commands.BAN} {username}";
            SendToChannel(data);
        }

        /**
            Unbans a user from the channel
         */
        public void UnbanUser(string username)
        {
            string data = $"{IRCSymbols.Commands.UNBAN} {username}";
            SendToChannel(data);
        }

        /**
            Deletes messages from the chat
         */
        public void ClearChat()
        {
            string data = $"{IRCSymbols.Commands.CLEAR}";
            SendToChannel(data);
        }

        /**
            Changes your username color
            #Either use predefined colors from twitch or hex format #000000
         */
        public void ChangeColor(string color)
        {
            string data = $"{IRCSymbols.Commands.COLOR} {color}";
            SendToChannel(data);
        }

        public void Commercial()
        {
            SendToChannel(IRCSymbols.Commands.COMMERCIAL);
        }

        public void Delete()
        {
            SendToChannel(IRCSymbols.Commands.DELETE);
        }

        

    }
}