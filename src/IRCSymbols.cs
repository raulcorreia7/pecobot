
namespace MightyPecoBot
{
    public static class IRCSymbols
    {

        public static class Colors
        {

            public const string BLUE = "Blue";
            public const string BLUE_VIOLET = "BlueViolet";
            public const string CADET_BLUE = "CadetBlue";
            public const string CHOCOLATE = "Chocolate";
            public const string CORAL = "CORAL";
            public const string DODGERBLUE = "DodgerBlue";
            public const string FIREBRICK = "Firebrick";
            public const string GOLDENROD = "GoldenRod";
            public const string GREEN = "GREEN";
            public const string HOTPINK = "HotPink";
            public const string ORANGERED = "OrangeRed";
            public const string RED = "Red";
            public const string SEAGREEN = "SeaGreen";
            public const string SPRINGGREEN = "SpringGreen";
            public const string YELLOWGREEN = "YellowGreen";
        }

        public static class Commands
        {
            public const string BAN = "/ban";
            public const string UNBAN = "/unban";

            public const string CLEAR = "/clear";

            public const string COLOR = "/color";

            public const string COMMERCIAL = "/commercial";
            public const string DELETE = "/delete";
            public const string DISCONNECT = "/delete";

            public const string EMOTE_ONLY_ON = "/emoteonly";

            public const string EMOTE_ONLY_OFF = "/emoteonlyoff";

            public const string FOLLOWERS_ON = "/followers";

            public const string FOLLOWERS_OFF = "/followersoff";

            public const string HELP = "/help";

            public const string HOST = "/host";

            public const string UNHOST = "/unhost";

            public const string MARKER = "/marker";

            public const string ME = "/me";

            public const string PROMOTE_TO_MOD = "/mod";

            public const string UNMOD = "/unmod";

            public const string LIST_MODS = "/mods";

            public const string R9KBETA_ON = "/r9kbeta";

            public const string R9KBETA_OFF = "/r9kbetaoff";

            public const string RAID = "/raid";

            public const string UNRAID = "/unraid";

            public const string SLOW_ON = "/slow";

            public const string SLOW_OFF = "/slowoff";

            public const string SUBSCRIBERS_ONLY = "/subscribers";

            public const string SUBSCRIBERS_OFF = "/subscribersoff";

            public const string TIMEOUT = "/timeout";

            public const string UNTIMEOUT = "/untimeout";

            public const string VIP = "/vip";

            public const string UNVIP = "/unvip";

            public const string VIPS = "/vips";

            public const string WHISPER = "/w";
        }

        public static class Badges
        {
            public static string ADMIN = "admin";
            public static string BROADCASTER = "broadcaster";
            public static string GLOBAL_MOD = "global_mod";
            public static string MODERATOR = "moderator";
            public static string SUBSCRIBER = "subscriber";
            public static string STAFF = "staff";
        }

        public static class CustomChannelCommands
        {
            public const string GITHUB = "!github";

        }
        public const string PRIVMSG = "PRIVMSG";
        public const string NICK = "NICK";
        public const string JOIN = "JOIN";

        public const string PASS = "PASS";

        public const string PING = "PING";

        public const string PART = "PART";

        public static string FormatTwoParameters(string p1, string p2)
        {
            return p1 + " " + p2;
        }
        public static string FormatOAuth(string oauth)
        {
            return FormatTwoParameters(PASS, oauth);
        }

        public static string FormatUsername(string username)
        {
            return FormatTwoParameters(NICK, username);
        }

        public static string FormatJoin(string channel)
        {
            return JOIN + " #" + channel;
        }

        public static string FormatChannelMessage(string channel, string message)
        {
            return PRIVMSG + " #" + channel + " :" + message;
        }


    }
}