


namespace MightyPecoBot.Network

{
    interface IClientSocket
    {

        void Connect();
        bool IsConnected();
        void Send(string message);
        string Receive();

    }
}