

using System.IO;
using System.Net.Sockets;

namespace MightyPecoBot.Network
{
    /**
        Class that handles TCP Connection
     */
    class TCPClientSocket : IClientSocket
    {
        public TcpClient Socket;
        public NetworkStream NetworkStream;
        public StreamReader StreamReader;
        public StreamWriter StreamWriter;

        public string URL { get; }
        public int PORT { get; }

        public TCPClientSocket(string url, int port)
        {
            Socket = new TcpClient(url, port);
            NetworkStream = Socket.GetStream();
            StreamReader = new StreamReader(NetworkStream);
            StreamWriter = new StreamWriter(NetworkStream);
            this.URL = url;
            this.PORT = port;
        }

        ~TCPClientSocket()
        {
            StreamWriter.Close();
            StreamReader.Close();
            NetworkStream.Close();
            Socket.Close();
        }

        public void Connect()
        {
            if (!Socket.Connected)
            {
                Socket.Connect(URL, PORT);
            }
        }

        public bool IsConnected() => Socket.Connected;
        public string Receive()
        {
            return StreamReader.ReadLine();
        }

        public void Send(string message)
        {
            StreamWriter.WriteLine(message);
            StreamWriter.Flush();
        }

    }
}