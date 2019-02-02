
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using MightyPecoBot.Network;
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

        List<Action<string>> Actions = new List<Action<string>>();
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
            Actions.Add((string data) =>
            {
                if (Regex.Match(data, IRCSymbols.ChannelCommands.GITHUB).Success)
                {
                    SendToChannel(GITHUB_URL);
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
            /**
                Need to add some kinds of composition
                so instead of a big if statement,
                it iterates over N expressions.
                This way users can add code/logic without
                messing with the if statements
             */
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
                    foreach (var action in Actions)
                    {
                        action(data);
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



    }
}