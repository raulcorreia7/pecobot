

namespace MightyPecoBot.Parsing

{
    public class ChannelMessage
    {
        public string Channel { get; }
        public string Username { get; }
        public string Message { get; }
        public string MessageID { get; }
        public string badge { get; }
        public int badge_version { get; }

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
        public string Channel { get; }

        public string Username { get; }

        public UserActionUponChannel(string channel, string username)
        {
            this.Channel = channel;
            this.Username = username;
        }

    }

    public class SubscriptionEvent
    {
        public string Badge { get; }
        public string Version { get; }
        public string Username { get; }
        public string Subtype { get; }
        public int Commulative_months { get; }
        public int Consecutive_months { get; }
        public string Subplan { get; }
        public string Channel { get; }
        public string Message { get; }

        public SubscriptionEvent(string badge, string version, string username, string subtype
        , int commulative_months, int consecutive_months, string subplan
        , string channel, string message)
        {
            this.Badge = badge;
            this.Version = version;
            this.Username = username;
            this.Subtype = subtype;
            this.Commulative_months = commulative_months;
            this.Consecutive_months = consecutive_months;
            this.Subplan = subplan;
            this.Channel = channel;
            this.Message = message;
        }
    }
    public class GiftEvent
    {
        public string Badge;
        public string Version;
        public string Gifter;
        public string TypeOfGift;
        public int Total_months_subscribed;
        public string Recipient;

        public string Message;
        public GiftEvent(string badge, string version, string gifter, string typeOfGift, int total_months_subscribed, string subplan, string recipient, string message)
        {
            this.Badge = badge;
            this.Version = version;
            this.Gifter = gifter;
            this.TypeOfGift = typeOfGift;
            this.Total_months_subscribed = total_months_subscribed;
            this.Recipient = recipient;
            this.Message = message;

        }
    }

    public class RaidingEvent
    {
        public string Badge;
        public string Version;
        public string RaiderChannel;
        public int NumberOfViewers;
        public string Message;
        public string RaidedChannel;

        public RaidingEvent(string badge, string version, string RaiderChannel
                            , int numberOfViewers, string message, string RaidedChannel)
        {
            this.Badge = badge;
            this.Version = version;
            this.RaidedChannel = RaidedChannel;
            this.RaiderChannel = RaiderChannel;
            this.NumberOfViewers = numberOfViewers;
            this.Message = message;
        }
    }

}