
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using MightyPecoBot.Network;
using MightyPecoBot.Parsing;

namespace MightyPecoBot
{
    class TwitchBot
    {

        public const string GITHUB_URL = "https://github.com/raulcorreia7/pecobot";
        public const string URL = "irc.chat.twitch.tv";

        //FIXME: Be aware of port, may change with SSL and Websockets
        public const int PORT = 6667;
        public string Username { get; }
        public string Channel { get; }

        public IClientSocket Socket;

        public volatile bool Running = false;

        public Thread ReceivingThread;

        List<Action<string>> MajorCallbacks = new List<Action<string>>();

        List<Action<ChannelMessage>> Callbacks_ChannelMessage = new List<Action<ChannelMessage>>();
        List<Action<UserActionUponChannel>> Callbacks_JoinedChannel = new List<Action<UserActionUponChannel>>();
        List<Action<UserActionUponChannel>> Callbacks_LeaveChannel = new List<Action<UserActionUponChannel>>();
        public TwitchBot(string username, string channel)
        {
            Username = username;
            Channel = channel;
            Socket = new TCPClientSocket(URL, PORT);
            ReceivingThread = new Thread(ReceiveData);
            DefaultActions();
        }

        ~TwitchBot()
        {
            CleanUp();

        }

        private void DefaultActions()
        {
            /*
                This responds to Ping requests
             */
            MajorCallbacks.Add((string data) =>
            {
                if (Regex.Match(data, IRCSymbols.PING).Success)
                {
                    sendPONG();
                }
            });
            /*
                This parses the channel,username and message and fires the OnChannelMessage Event
             */
            MajorCallbacks.Add((string data) =>
            {
                //If PRIVMSG, fire onMessageEvent
                //Optimize this to just one regular expression
                if (Regex.Match(data, IRCSymbols.PRIVMSG).Success)
                {
                    Match match = Regex.Match(data, @":(\w+)!.+#(\w+) :(.+)$");
                    if (match.Success)
                    {
                        string username = match.Groups[1].Value;
                        string channel = match.Groups[2].Value;
                        string message = match.Groups[3].Value;
                        Match match_message_id = Regex.Match(data, @";id=([a-zA-Z0-9-]+);");
                        string message_id = null;
                        if (match_message_id.Success)
                        {
                            message_id = match_message_id.Groups[1].Value;
                        }
                        Match match_isMod = Regex.Match(data, @";mod=(\d+)");
                        bool isMod = false;
                        if (match_isMod.Success)
                        {
                            isMod = match_isMod.Groups[1].Value == "1";
                        }
                        ChannelMessage channel_message = new ChannelMessage(channel, username, message, message_id, isMod);
                        RunOnChannelMessageCallbacks(channel_message);
                    }
                }
            });

            /*
                This parses that user joins the channel and fires onJoinChannel callbacks
             */
            MajorCallbacks.Add((string data) =>
            {
                Match match = Regex.Match(data, @":(.+)!.+ JOIN #(.+)");
                if (match.Success)
                {
                    UserActionUponChannel joinChannel = new UserActionUponChannel(channel: match.Groups[2].Value, username: match.Groups[1].Value);
                    RunOnJoinedChannelCallback(joinChannel);
                }

            });

            MajorCallbacks.Add((string data) =>
            {
                Match match = Regex.Match(data, @":(.+)!.+ PART #(.+)");
                if (match.Success)
                {
                    UserActionUponChannel leaveChannel = new UserActionUponChannel(channel: match.Groups[2].Value, username: match.Groups[1].Value);
                    RunOnLeaveChannelCallback(leaveChannel);
                }

            });


            Callbacks_ChannelMessage.Add((ChannelMessage channelMessage) =>
            {
                if (channelMessage.Message.Contains(IRCSymbols.CustomChannelCommands.GITHUB))
                {
                    SendToChannel(channel: channelMessage.Channel, message: GITHUB_URL);
                }

            });
        }

        private void CleanUp()
        {
            BotLogger.LogDebug("Cleaning up the bot!");
            try
            {
                ReceivingThread.Interrupt();
                if (!ReceivingThread.Join(1000))
                {
                    ReceivingThread.Abort();
                }

            }
            catch (Exception e)
            {
                if (e is ThreadAbortException tabex)
                {
                    BotLogger.LogDebug("Couldn't terminate the thread in time. Forced it's shutdown.");
                }
                else
                {
                    if (e is ThreadInterruptedException tiex)
                    {
                        BotLogger.LogDebug("Interrupted the exception!");
                    }

                }
            }

            BotLogger.LogDebug("Thread terminated!");

        }

        public void Connect(string oauth)
        {
            BotLogger.LogDebug("[ >>Connecting! ]");
            Socket.Connect();
            SendOauth(oauth);
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

        private void sendPONG()
        {
            BotLogger.LogDebug("[ >> Sending PONG ]");
            SendToIRC("PONG :tmi.twitch.tv");
        }
        public void SendOauth(string oauth)
        {
            BotLogger.LogDebug("[ >> Sending OAUTH >");
            SendToIRC(IRCSymbols.FormatOAuth(oauth));
        }

        public void SendToIRC(string message)
        {
            BotLogger.LogMessage(message);
            Socket.Send(message);
        }
        public void SendToChannel(string message)
        {
            BotLogger.LogMessage(message);
            string data = IRCSymbols.FormatChannelMessage(Channel, message);
            Socket.Send(data);
        }

        public void SendToChannel(string channel, string message)
        {
            BotLogger.LogMessage(message);
            string data = IRCSymbols.FormatChannelMessage(channel, message);
            Socket.Send(data);
        }

        private void RequestTwitchMembershipStateEvents()
        {
            BotLogger.LogDebug("Requesting Membership capabilities.");
            string data = "CAP REQ :twitch.tv/membership twitch.tv/commands twitch.tv/tags";
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
                    foreach (var callback in MajorCallbacks)
                    {
                        callback(data);
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

        /**
            Send a commercial break
         */
        public void Commercial()
        {
            SendToChannel(IRCSymbols.Commands.COMMERCIAL);
        }
        /**
        
            Sends delete command
            * Caveat: I don't know what it does
         */
        public void Delete()
        {
            SendToChannel(IRCSymbols.Commands.DELETE);
        }

        /**
            Disconnects the bot,
            it stops everything!
         */
        public void Disconnect()
        {
            Running = false;
            SendToChannel(IRCSymbols.Commands.DISCONNECT);
            CleanUp();

        }

        /**
            Sets the channel chat to be emote only(true) or not (false)
         */
        public void EmoteOnly(bool isEmoteOnly)
        {

            if (isEmoteOnly)
                SendToChannel(IRCSymbols.Commands.EMOTE_ONLY_ON);
            else
                SendToChannel(IRCSymbols.Commands.EMOTE_ONLY_OFF);
        }

        private void RunOnChannelMessageCallbacks(ChannelMessage channelMessage)
        {
            BotLogger.LogMessage($"#{channelMessage.Channel} <{channelMessage.Username}> {channelMessage.Message}");
            foreach (var callback in this.Callbacks_ChannelMessage)
                callback(channelMessage);
        }

        private void RunOnJoinedChannelCallback(UserActionUponChannel information)
        {
            BotLogger.LogMessage($"{information.Username} joined the channel: #{information.Channel}");
            foreach (var callback in this.Callbacks_JoinedChannel)
                callback(information);
        }

        private void RunOnLeaveChannelCallback(UserActionUponChannel information)
        {
            BotLogger.LogMessage($"{information.Username} left the channel: #{information.Channel}");
            foreach (var callback in this.Callbacks_LeaveChannel)
                callback(information);
        }
        public void OnChannelMessage(Action<ChannelMessage> callback)
        {
            this.Callbacks_ChannelMessage.Add(callback);
        }

        public void onJoinChannel(Action<UserActionUponChannel> callback)
        {
            this.Callbacks_JoinedChannel.Add(callback);
        }

        public void onLeaveChannel(Action<UserActionUponChannel> callback)
        {
            this.Callbacks_LeaveChannel.Add(callback);
        }
    }
}