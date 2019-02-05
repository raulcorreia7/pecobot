

namespace MightyPecoBot.Parsing

{
    public class ChannelMessage
    {
        public string Channel;
        public string Username;
        public string Message;

        public string MessageID;

        public bool Mod;

        public ChannelMessage(string channel, string username, string message, string messageID, bool mod)
        {
            this.Channel = channel;
            this.Username = username;
            this.Message = message;
            if (!string.IsNullOrEmpty(messageID))
                this.MessageID = messageID;
            else
                this.MessageID ="NotParsed";
        this.Mod = mod;
        }
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