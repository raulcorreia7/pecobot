

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace MightyPecoBot.Network
{
    /**
        Class that handles TCP Connection
     */
    class TCPClientSocket : IClientSocket
    {
        public TcpClient Socket;
        public StreamReader StreamReader;
        public StreamWriter StreamWriter;

        public Stream Stream;

        public string URL { get; }
        public int PORT { get; }

        public bool IsEncrypted;

        public TCPClientSocket(string url, int port, bool encrypted)
        {
            Socket = new TcpClient();
            this.URL = url;
            this.PORT = port;
            this.IsEncrypted = encrypted;
        }

        ~TCPClientSocket()
        {
            if (Socket == null || Stream == null || StreamReader == null || StreamWriter == null) return;
            StreamWriter.Close();
            StreamReader.Close();
            Socket.Close();
        }

        public void Connect()
        {
            if (!Socket.Connected)
            {
                Socket.Connect(URL, PORT);
                if (IsEncrypted)
                {
                    SslStream st = new SslStream(Socket.GetStream());
                    st.AuthenticateAsClient(URL);
                    Stream = st;
                }
                else
                {
                    Stream = Socket.GetStream();
                }
                StreamReader = new StreamReader(Stream);
                StreamWriter = new StreamWriter(Stream);
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