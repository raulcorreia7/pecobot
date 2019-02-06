

namespace MightyPecoBot.Parsing

{
    public class ChannelMessageEvent
    {
        public string Channel { get; }
        public string Username { get; }
        public string Message { get; }
        public string MessageID { get; }
        public string Badge { get; }
        public int BadgeVersion { get; }

        public ChannelMessageEvent(string channel, string username, string message, string messageID, string badge, int badge_version)
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
                this.Badge = badge.ToLower(); ;
                this.BadgeVersion = badge_version;
            }
            else
            {
                this.Badge = "NOBADGE";
                this.BadgeVersion = -1;
            }
        }

        public bool IsAdmin => this.Badge == IRCSymbols.Badges.ADMIN;
        public bool IsBroadcaster => this.Badge == IRCSymbols.Badges.BROADCASTER;
        public bool IsGlobalMod => this.Badge == IRCSymbols.Badges.GLOBAL_MOD;
        public bool IsModerator => this.Badge == IRCSymbols.Badges.MODERATOR;
        public bool IsSubscriber => this.Badge == IRCSymbols.Badges.SUBSCRIBER;
        public bool IsStaff => this.Badge == IRCSymbols.Badges.STAFF;
    }

    public class BitsEvent
    {
        public string Badge;
        public string Version;
        public int NumberOfBits;
        public string Username;
        public string Channel;
        public string Message;
        public BitsEvent(string badge, string version, int numberOfBits
                        , string username, string channel, string message)
        {
            this.Badge = badge;
            this.Version = version;
            this.NumberOfBits = numberOfBits;
            this.Username = username;
            this.Channel = channel;
            this.Message = message;
        }
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
        public int CommulativeMonths { get; }
        public int ConsecutiveMonths { get; }
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
            this.CommulativeMonths = commulative_months;
            this.ConsecutiveMonths = consecutive_months;
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
        public int TotalMonthsSubscribed;
        public string Recipient;
        public string Message;
        public GiftEvent(string badge, string version, string gifter, string typeOfGift, int total_months_subscribed, string subplan, string recipient, string message)
        {
            this.Badge = badge;
            this.Version = version;
            this.Gifter = gifter;
            this.TypeOfGift = typeOfGift;
            this.TotalMonthsSubscribed = total_months_subscribed;
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

    public class RitualEvent
    {
        public string Username;
        public string Ritual;
        public string TypeOfRitual;
        public string EventMessage;
        public string Channel;
        public string UserMessage;
        public RitualEvent(string username, string ritual, string typeOfRitual
                            , string event_message, string channel, string user_message)
        {
            this.Username = username;
            this.Ritual = ritual;
            this.TypeOfRitual = typeOfRitual;
            this.EventMessage = event_message;
            this.Channel = channel;
            this.UserMessage = user_message;
        }
    }

}