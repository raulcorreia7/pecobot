

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace MightyPecoBot.Network
{
    /**
        Class that handles TCP Connection
     */
    class TCPSSLClientSocket : IClientSocket
    {
        public TcpClient Socket;

        SslStream Sslstream;
        public StreamReader StreamReader;
        public StreamWriter StreamWriter;

        public string URL { get; }
        public int PORT { get; }

        public TCPSSLClientSocket(string url, int port)
        {
            Socket = new TcpClient();
            this.URL = url;
            this.PORT = port;
        }

        ~TCPSSLClientSocket()
        {
            if (Socket == null || this.Sslstream == null || StreamReader == null || StreamWriter == null) return;
            StreamWriter.Close();
            StreamReader.Close();
            Sslstream.Close();
            Socket.Close();
        }

        public void Connect()
        {
            if (!Socket.Connected)
            {
                Socket.Connect(URL, PORT);
                Sslstream = new SslStream(Socket.GetStream());
                Sslstream.AuthenticateAsClient(URL);
                StreamReader = new StreamReader(Sslstream);
                StreamWriter = new StreamWriter(Sslstream);
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
            StreamWriter.WriteLine(message);
            StreamWriter.Flush();
        }

    }
}