

namespace MightyPecoBot.Parsing

{
    public class ChannelMessage
    {
        public string Channel;
        public string Username;
        public string Message;

        public ChannelMessage(string channel, string username, string message)
        {
            this.Channel = channel;
            this.Username = username;
            this.Message = message;
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