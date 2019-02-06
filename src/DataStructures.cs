

namespace MightyPecoBot.Parsing

{
    public class ChannelMessage
    {
        public string Channel;
        public string Username;
        public string Message;
        public string MessageID;
        public string badge;
        public int badge_version;

        public ChannelMessage(string channel, string username, string message, string messageID, string badge, int badge_version)
        {
            this.Channel = channel;
            this.Username = username;
            this.Message = message;
            if (!string.IsNullOrEmpty(messageID))
                this.MessageID = messageID;
            else
                this.MessageID = "NOMESSAGEID";

            if (string.IsNullOrEmpty(badge))
            {
                this.badge = badge.ToLower(); ;
                this.badge_version = badge_version;
            }
            else
            {
                this.badge = "NOBADGE";
                this.badge_version = -1;
            }
        }
        public bool IsAdmin => this.badge == IRCSymbols.Badges.ADMIN;
        public bool IsBroadcaster => this.badge == IRCSymbols.Badges.BROADCASTER;
        public bool IsGlobalMod => this.badge == IRCSymbols.Badges.GLOBAL_MOD;
        public bool IsModerator => this.badge == IRCSymbols.Badges.MODERATOR;
        public bool IsSubscriber => this.badge == IRCSymbols.Badges.SUBSCRIBER;
        public bool IsStaff => this.badge == IRCSymbols.Badges.STAFF;
    }

    public class UserActionUponChannel
    {
        public string Channel;

        public string Username;

        public UserActionUponChannel(string channel, string username)
        {
            this.Channel = channel;
            this.Username = username;
        }

    }

}