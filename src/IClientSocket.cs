


namespace MightyPecoBot.Network

{
    interface IClientSocket
    {

        void Connect();
        void Send(string message);
        string Receive();

    }
}