
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using MightyPecoBot.Network;
using MightyPecoBot.Parsing;
using MightyPecoBot.Callbacks;
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

        private Thread ReceivingThread;

        private Timer Timer_memory;

        List<Func<string, CallbackAction>> NormalCallbacks = new List<Func<string, CallbackAction>>();

        List<Func<ChannelMessage, CallbackAction>> Callbacks_ChannelMessage = new List<Func<ChannelMessage, CallbackAction>>();
        List<Func<UserActionUponChannel, CallbackAction>> Callbacks_JoinedChannel = new List<Func<UserActionUponChannel, CallbackAction>>();
        List<Func<UserActionUponChannel, CallbackAction>> Callbacks_LeaveChannel = new List<Func<UserActionUponChannel, CallbackAction>>();
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
            NormalCallbacks.Add((string data) =>
            {
                if (Regex.Match(data, IRCSymbols.PING).Success)
                {
                    sendPONG();
                }
                return CallbackAction.SKIP_OTHERS;
            });
            /*
                This parses the channel,username and message and fires the OnChannelMessage Event
             */
            NormalCallbacks.Add((string data) =>
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
                return CallbackAction.SKIP_OTHERS;
            });

            NormalCallbacks.Add((string data) =>
            {
                if (Regex.Match(data, @":Unknown command").Success)
                {
                    BotLogger.LogError("[ >> Unknown command! ]");
                }
                return CallbackAction.SKIP_OTHERS;
            });

            /*
                This parses that user joins the channel and fires onJoinChannel callbacks
             */
            NormalCallbacks.Add((string data) =>
            {
                Match match = Regex.Match(data, @":(.+)!.+ JOIN #(.+)");
                if (match.Success)
                {
                    UserActionUponChannel joinChannel = new UserActionUponChannel(channel: match.Groups[2].Value, username: match.Groups[1].Value);
                    RunOnJoinedChannelCallback(joinChannel);
                }
                return CallbackAction.SKIP_OTHERS;
            });

            NormalCallbacks.Add((string data) =>
            {
                Match match = Regex.Match(data, @":(.+)!.+ PART #(.+)");
                if (match.Success)
                {
                    UserActionUponChannel leaveChannel = new UserActionUponChannel(channel: match.Groups[2].Value, username: match.Groups[1].Value);
                    RunOnLeaveChannelCallback(leaveChannel);
                }
                return CallbackAction.SKIP_OTHERS;
            });


            Callbacks_ChannelMessage.Add((ChannelMessage channelMessage) =>
            {
                if (channelMessage.Message.Contains(IRCSymbols.CustomChannelCommands.GITHUB))
                {
                    SendToChannel(channel: channelMessage.Channel, message: GITHUB_URL);
                }
                return CallbackAction.SKIP_OTHERS;
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
            initTimer();
        }

        private void initTimer()
        {
            Timer_memory = new Timer((e) =>
            {
                long bytes = GC.GetTotalMemory(false);
                double megabytes = Math.Round(bytes * 1E-6, 3);
                BotLogger.LogDebug($"Memory usage is : {megabytes}MB");

            }, null, 1000, 60 * 1000);

        }

        private void sendPONG()
        {
            BotLogger.LogDebug("[ >> Sending PONG ]");
            SendToIRC("PONG :tmi.twitch.tv");
        }
        private void SendOauth(string oauth)
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
                    RunCallbacks(data);
                }
            }
        }

        private void RunCallbacks(string data)
        {
            long start = DateTime.Now.Millisecond;
            foreach (var callback in NormalCallbacks)
            {
                callback(data);
            }
            long deltaTime = DateTime.Now.Millisecond - start;
            BotLogger.LogDebug($"It took: {deltaTime}ms to run callbacks.");
        }

        private void RunOnChannelMessageCallbacks(ChannelMessage channelMessage)
        {
            BotLogger.LogMessage($"#{channelMessage.Channel} <{channelMessage.Username}> {channelMessage.Message}");
            foreach (var callback in this.Callbacks_ChannelMessage)
                if (callback(channelMessage) == CallbackAction.SKIP_OTHERS)
                    break;
        }

        private void RunOnJoinedChannelCallback(UserActionUponChannel information)
        {
            BotLogger.LogMessage($"{information.Username} joined the channel: #{information.Channel}");
            foreach (var callback in this.Callbacks_JoinedChannel)
                if (callback(information) == CallbackAction.SKIP_OTHERS)
                    break;
        }

        private void RunOnLeaveChannelCallback(UserActionUponChannel information)
        {
            BotLogger.LogMessage($"{information.Username} left the channel: #{information.Channel}");
            foreach (var callback in this.Callbacks_LeaveChannel)
                if (callback(information) == CallbackAction.SKIP_OTHERS)
                    break;
        }
        public void OnChannelMessage(Func<ChannelMessage, CallbackAction> callback)
        {
            this.Callbacks_ChannelMessage.Add(callback);
        }

        public void OnJoinChannel(Func<UserActionUponChannel, CallbackAction> callback)
        {
            this.Callbacks_JoinedChannel.Add(callback);
        }

        public void OnLeaveChannel(Func<UserActionUponChannel, CallbackAction> callback)
        {
            this.Callbacks_LeaveChannel.Add(callback);
        }

        //FIXME: need to fix all of this to receive a channel
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
        public void ChangeEmoteOnly(bool isEmoteOnly)
        {
            if (isEmoteOnly)
            { SendToChannel(IRCSymbols.Commands.EMOTE_ONLY_ON); }
            else
            { SendToChannel(IRCSymbols.Commands.EMOTE_ONLY_OFF); }
        }

        /**
            Changes the cannel to follower only or not
         */
        public void ChangeFollower(string channel, bool isFollowerOnly)
        {
            if (isFollowerOnly)
            { SendToChannel(channel, IRCSymbols.Commands.FOLLOWERS_ON); }
            else
            { SendToChannel(channel, IRCSymbols.Commands.FOLLOWERS_OFF); }
        }
        /**
            Sends help to the channel asking for the commands
         */
        public void SendHelp(string extracommand = null)
        {
            if (String.IsNullOrEmpty(extracommand))
                SendToChannel(IRCSymbols.Commands.HELP);
            else
                SendToChannel($"{IRCSymbols.Commands.HELP} {extracommand}");
        }

        public void HostChannel(string channelToHost)
        {
            SendToChannel($"{IRCSymbols.Commands.HOST} {channelToHost}");
        }

        public void UnHost()
        {
            SendToChannel(IRCSymbols.Commands.UNHOST);
        }

        public void SetMarker(string markingName = null)
        {
            if (String.IsNullOrEmpty(markingName))
            {
                SendToChannel(IRCSymbols.Commands.MARKER);
            }
            else
            {
                SendToChannel($"{IRCSymbols.Commands.MARKER} {markingName}");
            }
        }

        public void SendMe(string message)
        {
            SendToChannel(message);
        }

        public void SendMOD(string username)
        {
            SendToChannel($"{IRCSymbols.Commands.PROMOTE_TO_MOD}, {username}");
        }

        public void SendUnmod(string username)
        {
            SendToChannel($"{IRCSymbols.Commands.UNMOD}, {username}");
        }

        public void ListMods()
        {
            //TODO: Parse message to list moderators
            SendToChannel(IRCSymbols.Commands.LIST_MODS);
        }

        public void SendSetR9KMode(bool isR9KMode)
        {
            if (isR9KMode)
                SendToChannel(IRCSymbols.Commands.R9KBETA_ON);
            else
                SendToChannel(IRCSymbols.Commands.R9KBETA_OFF);
        }

        public void SendRaidChannel(string channel)
        {
            SendToChannel($"{IRCSymbols.Commands.RAID} {channel}");
        }

        public void SendUnRaid()
        {
            SendToChannel($"{IRCSymbols.Commands.UNRAID}");
        }

        public void SendSetSlowChannel(bool isSlow)
        {
            if (isSlow)
                SendToChannel(IRCSymbols.Commands.SLOW_ON);
            else
                SendToChannel(IRCSymbols.Commands.SLOW_OFF);
        }

        public void SendSubsOnly(bool isSubsOnly)
        {
            if (isSubsOnly)
                SendToChannel(IRCSymbols.Commands.SUBSCRIBERS_ONLY);
            else
                SendToChannel(IRCSymbols.Commands.SUBSCRIBERS_OFF);
        }

        /**
        "/timeout <username> [duration][time unit] [reason]" - 
        Temporarily prevent a user from chatting.
        Duration (optional, default=10 minutes) must be a positive integer;
        time unit (optional, default=s) must be one of s, m, h, d, w;
        maximum duration is 2 weeks. 
        Combinations like 1d2h are also allowed. 
        Reason is optional and will be shown to the target user and other moderators.
        Use "untimeout" to remove a timeout.
         */
        public void SendTimeout(string username, int duration, string time_unit, string reason)
        {
            SendToChannel($"{IRCSymbols.Commands.TIMEOUT} {username} {duration} {time_unit} {reason}");
        }

        public void SendRemoveTimeout(string username)
        {
            SendToChannel($"{IRCSymbols.Commands.UNTIMEOUT} {username}");
        }
    }
}