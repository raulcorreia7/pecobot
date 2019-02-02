
namespace MightyPecoBot
{
    public class IRCSymbols
    {

        public class Colors
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

        public class Commands
        {

            public const string BAN = "/ban";
            public const string UNBAN = "/unban";

            public const string CLEAR = "/clear";

            public const string COLOR = "/color";

            public const string COMMERCIAL = "/commercial";
            public const string DELETE = "/delete";
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