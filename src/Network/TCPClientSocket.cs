

using System;
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
            Socket = new TcpClient();
            this.URL = url;
            this.PORT = port;
        }

        ~TCPClientSocket()
        {
            if (NetworkStream == null || StreamReader == null || StreamWriter == null) return;
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
                NetworkStream = Socket.GetStream();
                StreamReader = new StreamReader(NetworkStream);
                StreamWriter = new StreamWriter(NetworkStream);
            }
        }

        public bool IsConnected() => Socket.Connected;
        public string Receive()
        {
            if (Socket.Connected)
            {
                //This has an exception, we need to treat it
                return StreamReader.ReadLine();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Send(string message)
        {
            if (Socket.Connected)
            {
                StreamWriter.WriteLine(message);
                StreamWriter.Flush();
            }
        }

    }
}