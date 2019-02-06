
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

        //FIXME: Be aware of port, may change with SSL and Websockets
        public string Username { get; }
        public string DefaultChannel { get; }

        public IClientSocket Socket;

        public volatile bool Running = false;

        private Thread ReceivingThread;

        private Timer Timer_memory;

        CallbackHandler CallbackHandler;
        /*
        
            Need to hide all these callbacks
         */
        public TwitchBot(string username, string channel)
        {
            Username = username;
            DefaultChannel = channel;
            CallbackHandler = new CallbackHandler();
            Socket = SocketFactory.createSocketConnection(ConnectionType.TCP_UNENCRYPTED);
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
            CallbackHandler.AddToplevelCallback((string data) =>
            {
                if (Regex.Match(data, IRCSymbols.Keywords.PING).Success)
                {
                    sendPONG();
                }
                return CallbackAction.SKIP_OTHERS;
            });
            /*
                This parses the channel,username and message and fires the OnChannelMessage Event
             */
            CallbackHandler.AddToplevelCallback((string data) =>
            {
                /* 
                    There are two cases, 

                    * one is a regular privmsg
                    * second is privmsg with bits information

                    */
                if (data.Contains(IRCSymbols.Keywords.BITS))
                {
                    Match match = Regex.Match(data, @"@badges=([a-zA-Z]+/\d+).+;bits=(\d+).+:([a-zA-Z_0-9]+)!.+PRIVMSG\s#(\w+)\s:(.+)");
                    if (match.Success)
                    {
                        var tuple = IRCSymbols.ParseBadge(match.Groups[1].Value);
                        int bits = Int32.Parse(match.Groups[2].Value);
                        string username = match.Groups[3].Value;
                        string channel = match.Groups[4].Value;
                        string message = match.Groups[5].Value;
                        BitsEvent bitsEvent = new BitsEvent(tuple.badge, tuple.version, bits, username, channel, message);

                        return CallbackAction.SKIP_OTHERS;
                    }
                }
                else
                if (data.Contains(IRCSymbols.Keywords.PRIVMSG))
                {


                    Match match = Regex.Match(data, @":(\w+)!.+#(\w+) :(.+)$");
                    if (match.Success)
                    {
                        string username = match.Groups[1].Value;
                        string channel = match.Groups[2].Value;
                        string message = match.Groups[3].Value;
                        string badge = String.Empty;
                        int version = -1;
                        Match match_message_id = Regex.Match(data, @";id=([a-zA-Z0-9-]+);");
                        string message_id = null;
                        if (match_message_id.Success)
                        {
                            message_id = match_message_id.Groups[1].Value;
                        }

                        Match match_badge = Regex.Match(data, @"@badges=(\w+/\d+|);");
                        if (match_badge.Success)
                        {

                            badge = match_badge.Groups[1].Value;
                            if (!String.IsNullOrEmpty(badge))
                            {
                                string[] parsed = badge.Split('/');
                                badge = parsed[0];
                                version = Int32.Parse(parsed[1]);
                            }
                        }
                        ChannelMessageEvent channel_message = new ChannelMessageEvent(channel, username, message, message_id, badge, version);
                        CallbackHandler.RunOnChannelMessageCallbacks(channel_message);
                    }
                }
                return CallbackAction.SKIP_OTHERS;
            });
            /*
                Callback that parses and triggers & logs bad error
             */
            CallbackHandler.AddToplevelCallback((string data) =>
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
            CallbackHandler.AddToplevelCallback((string data) =>
            {
                Match match = Regex.Match(data, @":(.+)!.+ JOIN #(.+)");
                if (match.Success)
                {
                    UserActionUponChannel joinChannel = new UserActionUponChannel(channel: match.Groups[2].Value, username: match.Groups[1].Value);
                    CallbackHandler.RunOnJoinedChannelCallback(joinChannel);
                }
                return CallbackAction.SKIP_OTHERS;
            });
            /*
                Callback to parse and trigger OnLeaveChannel event
             */
            CallbackHandler.AddToplevelCallback((string data) =>
            {
                Match match = Regex.Match(data, @":(.+)!.+ PART #(.+)");
                if (match.Success)
                {
                    UserActionUponChannel leaveChannel = new UserActionUponChannel(channel: match.Groups[2].Value, username: match.Groups[1].Value);
                    CallbackHandler.RunOnLeaveChannelCallback(leaveChannel);
                }
                return CallbackAction.SKIP_OTHERS;
            });

            /* Usernotice */
            CallbackHandler.AddToplevelCallback((string data) =>
            {

                //If it matches a sub/resub
                Match match = Regex.Match(data, @"@badges=(\w+/\d+).+;display-name=([a-zA-Z0-9]+).+;msg-id=(resub|sub);msg-param-cumulative-months=(\d+);msg-param-streak-months=(\d+);.+;msg-param-sub-plan=([a-zA-Z0-9]+).+USERNOTICE\s#(\w+)\s:(.+)");
                if (match.Success)
                {
                    var badge_version = match.Groups[1].Value.Split('/');
                    string badge = badge_version[0];
                    string version = badge_version[1];
                    string username = match.Groups[2].Value;
                    string subtype = match.Groups[3].Value;
                    int commulative_months = Int32.Parse(match.Groups[3].Value);
                    int consecutive_months = Int32.Parse(match.Groups[4].Value);
                    string subplan = match.Groups[6].Value;
                    string channel = match.Groups[7].Value;
                    string message = match.Groups[8].Value;

                    SubscriptionEvent subs_event = new SubscriptionEvent
                                                (badge, version, username,
                                                subtype, commulative_months, consecutive_months,
                                                subplan, channel, message);
                    CallbackHandler.RunOnSubscribeCallback(subs_event);
                    return CallbackAction.SKIP_OTHERS;
                }
                return CallbackAction.CONTINUE;
            });
            /*
                Parses a gift event
             */
            CallbackHandler.AddToplevelCallback((string data) =>
            {
                Match match = Regex.Match(data, @"@badges=(\w+/\d+).+;display-name=([a-zA-Z0-9]+).+;msg-id=(\w+);msg-param-months=(\d+);msg-param-recipient-display-name=([a-zA-Z0-9_]+);.+msg-param-sub-plan=([a-bA-Z0-9]+);");
                if (match.Success)
                {
                    var badge_version = IRCSymbols.ParseBadge(match.Groups[1].Value);
                    string gifter = match.Groups[2].Value;
                    string typeOfGift = match.Groups[3].Value;
                    int total_months = Int32.Parse(match.Groups[4].Value);
                    string recipient = match.Groups[5].Value;
                    string subplan = match.Groups[6].Value;
                    string message = match.Groups[7].Value;
                    GiftEvent giftevent = new GiftEvent(badge_version.badge, badge_version.version, gifter, typeOfGift, total_months, subplan, recipient, message);
                    CallbackHandler.RunOnSubGiftCallback(giftevent);
                    return CallbackAction.SKIP_OTHERS;
                }
                return CallbackAction.CONTINUE;
            });
            /*
            Parses a RaidingEvent
             */
            CallbackHandler.AddToplevelCallback((string data) =>
            {

                Match match = Regex.Match(data, @"@badges=([a-zA-Z0-9]+/\d+);.+login=([a-zA-Z0-9]+);.+;msg-param-viewerCount=(\d+).+;system-msg=(.+)\sUSERNOTICE\s#(.+)");
                if (match.Success)
                {
                    var tuple = IRCSymbols.ParseBadge(match.Groups[1].Value);
                    var raiderchannel = match.Groups[2].Value;
                    var viewers = Int32.Parse(match.Groups[3].Value);
                    var message = match.Groups[4].Value;
                    var raidedchannel = match.Groups[5].Value;
                    RaidingEvent raiding_event = new RaidingEvent(tuple.badge, tuple.version, raiderchannel, viewers, message, raidedchannel);
                    CallbackHandler.RunOnRaidingCallbacks(raiding_event);
                    return CallbackAction.SKIP_OTHERS;
                }

                return CallbackAction.CONTINUE;
            });

            /*
                Parses a RitualEvent
             */

            CallbackHandler.AddToplevelCallback((string data) =>
            {
                Match match = Regex.Match(data, @"display-name=([a-zA-Z_0-9]+).+;login=([a-zA-Z_0-9]+).+;msg-id=(ritual);msg-param-ritual-name=([a-zA-Z0-9_]+);.+;system-msg=(.+);tmi.+USERNOTICE\s#([a-zA-Z0-9]+)\s+:(.+)");
                if (match.Success)
                {
                    string username = match.Groups[2].Value;
                    string ritual = match.Groups[3].Value;
                    string typeOfRitual = match.Groups[4].Value;
                    string event_message = match.Groups[5].Value;
                    string channel = match.Groups[6].Value;
                    string user_message = match.Groups[7].Value;
                    RitualEvent ritual_event = new RitualEvent(username, ritual, typeOfRitual, event_message, channel, user_message);
                    CallbackHandler.RunOnRitualEvent(ritual_event);
                    return CallbackAction.SKIP_OTHERS;
                }
                return CallbackAction.CONTINUE;
            });

            /*
                Callback if user sends !github command, it responds back with github url
             */
            CallbackHandler.AddToChannelMessageCallback((ChannelMessageEvent channelMessage) =>
            {
                if (channelMessage.Message.Contains(IRCSymbols.CustomChannelCommands.GITHUB))
                {
                    SendToChannel(channel: channelMessage.Channel, message: GITHUB_URL);
                    return CallbackAction.SKIP_OTHERS;
                }
                return CallbackAction.CONTINUE;
            });

            CallbackHandler.AddToSubscriptionCallback(
                (SubscriptionEvent subscription) =>
            {
                SendToChannel(subscription.Channel, "Thanks for the (re)subscription!");
                return CallbackAction.SKIP_OTHERS;
            });
        }

        /**
            This methods cleanups the the running thread for the receiving socket
        */
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
            SendToIRC(IRCSymbols.FormatJoin(DefaultChannel));
            RequestTwitchMembershipStateEvents();
            this.Running = true;
            ReceivingThread.Start();
        }
        public void Debug()
        {
            BotLogger.LogDebug("[ >>Sending HelloWorld! ]");
            SendToIRC(IRCSymbols.FormatChannelMessage(DefaultChannel, "HelloWorld!"));
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
            BotLogger.LogDebug("[ >> Sending OAUTH ]");
            Socket.Send(IRCSymbols.FormatOAuth(oauth));
        }

        public void SendToIRC(string message)
        {
            BotLogger.LogMessage(message);
            Socket.Send(message);
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
                    CallbackHandler.RunCallbacks(data);
                }
            }
        }


        public void OnChannelMessage(Func<ChannelMessageEvent, CallbackAction> callback)
        {
            this.CallbackHandler.AddToChannelMessageCallback(callback);
        }

        public void OnJoinChannel(Func<UserActionUponChannel, CallbackAction> callback)
        {
            this.CallbackHandler.AddToJoinedChannelCallback(callback);
        }

        public void OnLeaveChannel(Func<UserActionUponChannel, CallbackAction> callback)
        {
            this.CallbackHandler.AddToLeaveChannelCallback(callback);
        }

        public void OnSubscribe(Func<SubscriptionEvent, CallbackAction> callback)
        {
            this.CallbackHandler.AddToSubscriptionCallback(callback);
        }

        public void OnSubGift(Func<GiftEvent, CallbackAction> callback)
        {
            this.CallbackHandler.AddToSubGiftCallback(callback);
        }

        public void OnRaidingEvent(Func<RaidingEvent, CallbackAction> callback)
        {

            this.CallbackHandler.AddToRaidingEventCallback(callback);
        }

        public void OnBitsEvent(Func<BitsEvent, CallbackAction> callback)
        {
            this.CallbackHandler.AddToBitsEventCallback(callback);
        }

        //FIXME: need to fix all of this to receive a channel
        /*
            Twitch commands implementation
         */

        /**
            Bans a user from the channel
         */
        public void BanUser(string channel, string username)
        {
            string data = $"{IRCSymbols.Commands.BAN} {username}";
            SendToChannel(channel, data);
        }

        /**
            Unbans a user from the channel
         */
        public void UnbanUser(string channel, string username)
        {
            string data = $"{IRCSymbols.Commands.UNBAN} {username}";
            SendToChannel(channel, data);
        }

        /**
            Deletes messages from the chat
         */
        public void ClearChat(string channel)
        {
            string data = $"{IRCSymbols.Commands.CLEAR}";
            SendToChannel(channel, data);
        }

        /**
            Changes your username color
            #Either use predefined colors from twitch or hex format #000000
         */
        public void ChangeColor(string channel, string color)
        {
            string data = $"{IRCSymbols.Commands.COLOR} {color}";
            SendToChannel(channel, data);
        }

        /**
            Send a commercial break
         */
        public void Commercial(string channel)
        {
            SendToChannel(channel, IRCSymbols.Commands.COMMERCIAL);
        }
        /**
        
            Sends delete command
            * Caveat: I don't know what it does
         */
        public void Delete(string channel)
        {
            SendToChannel(channel, IRCSymbols.Commands.DELETE);
        }

        /**
            Disconnects the bot,
            it stops everything!
         */
        public void Disconnect(string channel = null)
        {
            Running = false;
            if (string.IsNullOrEmpty(channel))
            { SendToChannel(this.DefaultChannel, IRCSymbols.Commands.DISCONNECT); }
            else
            { SendToChannel(channel, IRCSymbols.Commands.DISCONNECT); }
            CleanUp();
        }

        /**
            Sets the channel chat to be emote only(true) or not (false)
         */
        public void ChangeEmoteOnly(string channel, bool isEmoteOnly)
        {
            if (isEmoteOnly)
            { SendToChannel(channel, IRCSymbols.Commands.EMOTE_ONLY_ON); }
            else
            { SendToChannel(channel, IRCSymbols.Commands.EMOTE_ONLY_OFF); }
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
        public void SendHelp(string channel, string extracommand = null)
        {
            if (String.IsNullOrEmpty(extracommand))
                SendToChannel(channel, IRCSymbols.Commands.HELP);
            else
                SendToChannel(channel, $"{IRCSymbols.Commands.HELP} {extracommand}");
        }

        public void HostChannel(string channelToHost, string channel = null)
        {
            string target_channel = null;
            if (string.IsNullOrEmpty(channel))
                target_channel = this.DefaultChannel;
            else
                target_channel = channel;
            SendToChannel(channel, $"{IRCSymbols.Commands.HOST} {channelToHost}");
        }

        public void UnHost(string channel = null)
        {
            string target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(channel, IRCSymbols.Commands.UNHOST);
        }

        public void SetMarker(string channel, string markingName = null)
        {
            if (String.IsNullOrEmpty(markingName))
            {
                SendToChannel(channel, IRCSymbols.Commands.MARKER);
            }
            else
            {
                SendToChannel(channel, $"{IRCSymbols.Commands.MARKER} {markingName}");
            }
        }
        /*
            Sends a message in the fird person like, with a differnt color.
         */
        public void SendMe(string message, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, message);
        }

        public void SendMod(string username, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.PROMOTE_TO_MOD}, {username}");
        }

        public void SendUnmod(string username, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.UNMOD}, {username}");
        }

        public void ListMods(string channel = null)
        {
            //TODO: Parse message to list moderators
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, IRCSymbols.Commands.LIST_MODS);
        }

        public void SendSetR9KMode(bool isR9KMode, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            if (isR9KMode)
                SendToChannel(target_channel, IRCSymbols.Commands.R9KBETA_ON);
            else
                SendToChannel(target_channel, IRCSymbols.Commands.R9KBETA_OFF);
        }

        public void SendRaidChannel(string whereTo, string fromWhatChannel = null)
        {
            var target_channel = (fromWhatChannel == null) ? DefaultChannel : fromWhatChannel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.RAID} {whereTo}");
        }

        public void SendUnRaid(string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(channel, $"{IRCSymbols.Commands.UNRAID}");
        }

        public void SendSetSlowChannel(bool isSlow, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            if (isSlow)
                SendToChannel(target_channel, IRCSymbols.Commands.SLOW_ON);
            else
                SendToChannel(target_channel, IRCSymbols.Commands.SLOW_OFF);
        }

        public void SendSubsOnly(bool isSubsOnly, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            if (isSubsOnly)
                SendToChannel(target_channel, IRCSymbols.Commands.SUBSCRIBERS_ONLY);
            else
                SendToChannel(target_channel, IRCSymbols.Commands.SUBSCRIBERS_OFF);
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
        public void SendTimeout(string username, int duration, string time_unit, string reason, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.TIMEOUT} {username} {duration} {time_unit} {reason}");
        }

        public void SendRemoveTimeout(string username, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.UNTIMEOUT} {username}");
        }

        public void SendVIP(string username, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.VIP} {username}");
        }

        public void SendUNVIP(string username, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.UNVIP} {username}");
        }

        public void SendListVIPS(string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, IRCSymbols.Commands.VIPS);
        }

        public void SendWhisper(string username, string message, string channel = null)
        {
            var target_channel = (channel == null) ? DefaultChannel : channel;
            SendToChannel(target_channel, $"{IRCSymbols.Commands.WHISPER} {username} {message}");
        }
    }
}