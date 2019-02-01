using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MightyPecoBot.Network;

namespace MightyPecoBot
{
    class Program
    {
        static volatile bool running = true;
        static void Main(string[] args)
        {
            Thread t;
            string url = "irc.chat.twitch.tv";
            int port = 6667;

            IClientSocket Socket = new TCPClientSocket(url, port);

            t = new Thread(() => OutputToConsole(Socket));
            t.IsBackground = true;
            t.Start();

            if (Socket.IsConnected()) Console.WriteLine("Connected!");

            string oath_token = System.IO.File.ReadAllText("oath.txt");

            mandarMensagem(Socket, "PASS " + oath_token);
            mandarMensagem(Socket, "NICK mightypecobot");
            mandarMensagem(Socket, "JOIN #frosticecold");
            mandarMensagem(Socket, "PRIVMSG #frosticecold :OLA PESSOAL");


            while (running)
            {
                string read = null;
                if ((read = Console.ReadLine()) != null)
                {
                    Console.WriteLine(read);
                    sendToChannel(Socket, read);
                }
            }

            Console.WriteLine("Exiting application...");
            t.Interrupt();
            if (!t.Join(250))
            {
                t.Abort();
            }

        }

        public static void OutputToConsole(IClientSocket socket)
        {

            try
            {
                Console.WriteLine("Output to console");
                string s_data;
                while (running)
                {
                    try
                    {
                        s_data = socket.Receive();
                        Console.WriteLine(s_data);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException || ex is ThreadInterruptedException)
                    Console.WriteLine("Receiving Thread Interrupted");
                else
                    Console.WriteLine("Oopsie!");
            }
            return;
        }

        public static void mandarMensagem(IClientSocket socket, string msg)
        {
            socket.Send(msg);
        }

        public static void sendToChannel(IClientSocket socket, string msg)
        {
            string data = "PRIVMSG #frosticecold :" + msg;
            mandarMensagem(socket, data);
        }
    }


}
