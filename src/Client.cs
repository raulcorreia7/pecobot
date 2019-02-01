
namespace MightyPecoBot
{
    class TwitchBot
    {

        public string Username { get; }

        public const string URL = "irc.chat.twitch.tv";
        public const int PORT = 6667;
        public TwitchBot(string username, string oauth)
        {
            Username = username;
        }
    }
}