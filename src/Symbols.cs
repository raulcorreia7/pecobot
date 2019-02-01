
namespace MightyPecoBot
{

    public class IRCSymbols
    {
        public static string PRIVMSG = "PRIVMSG";
        public static string NICK = "NICK";
        public static string JOIN = "JOIN";

        public static string PASS = "PASS";


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
            return FormatTwoParameters(JOIN, channel);
        }

        public static string FormatChannelMessage(string channel, string message)
        {
            return PRIVMSG + " #" + channel + " :" + message;
        }
    }
}