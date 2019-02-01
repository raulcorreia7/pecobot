using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MightyPecoBot
{
    class Program
    {
        static volatile bool running = true;
        static void Main(string[] args)
        {
            Thread t;
            string url = null;
            url = "irc.chat.twitch.tv";
            int port = 6667;

            TcpClient client = new TcpClient(url, port);
            NetworkStream stream = client.GetStream();
            StreamReader sr = new StreamReader(stream);
            StreamWriter sw = new StreamWriter(stream);

            t = new Thread(() => OutputToConsole(sr));
            t.IsBackground = true;
            t.Start();

            bool connected = client.Connected;
            if (connected) Console.WriteLine("Connected!");
            bool flag = stream.CanRead & stream.CanWrite;
            if (!flag) return;


            Byte[] data = new Byte[256];
            String responseData = String.Empty;
            string oath_token = System.IO.File.ReadAllText("oath.txt");

            mandarMensagem(sw, "PASS " + oath_token);
            mandarMensagem(sw, "NICK mightypecobot");
            mandarMensagem(sw, "JOIN #frosticecold");
            mandarMensagem(sw, "PRIVMSG #frosticecold :OLA PESSOAL");


            while (running)
            {
                string read = null;
                if ((read = Console.ReadLine()) != null)
                {
                    Console.WriteLine(read);
                    sendToChannel(sw, read);
                }
            }


            if (!t.Join(250))
            {
                t.Abort();
            }
            sw.Close();
            sr.Close();
            stream.Close();
            client.Close();
        }

        public static void OutputToConsole(StreamReader sr)
        {

            try
            {
                Console.WriteLine("Output to console");
                string s_data;
                //while ((s_data = sr.ReadLine()) != null)
                while (running)
                {
                    try
                    {
                        s_data = sr.ReadLine();
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

        public static void mandarMensagem(StreamWriter sw, string msg)
        {
            sw.WriteLine(msg);
            sw.Flush();
        }

        public static void sendToChannel(StreamWriter sw, string msg)
        {
            string data = "PRIVMSG #frosticecold :" + msg;
            mandarMensagem(sw, data);
        }
        static Byte[] convertString(string message)
        {
            return Encoding.ASCII.GetBytes(message);
        }
    }


}
