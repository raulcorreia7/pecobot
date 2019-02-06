

using System;

namespace MightyPecoBot.Network
{

    enum ConnectionType
    {
        TCP_UNENCRYPTED,
        TCP_SSL
    }
    static class SocketFactory
    {
        const string TCP_URL = "irc.chat.twitch.tv";
        const int PORT_TCP = 6667;
        const int PORT_TCP_SSL = 6697;
        public static IClientSocket createSocketConnection(ConnectionType connection)
        {
            switch (connection)
            {
                case ConnectionType.TCP_UNENCRYPTED:
                    return new TCPClientSocket(TCP_URL, PORT_TCP, false);
                case ConnectionType.TCP_SSL:
                    return new TCPClientSocket(TCP_URL, PORT_TCP_SSL, true);
                default:
                    throw new ArgumentException();

            }
        }


    }
}