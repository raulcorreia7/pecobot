using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace pecobot
{
    class Program
    {
        static void Main(string[] args)
        {

            Thread t;
            string url = "irc.chat.twitch.tv";//"irc://irc.chat.twitch.tv";
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
            mandarMensagem(sw, "WHO #frosticecold");
            mandarMensagem(sw, "JOIN #frosticecold");
            mandarMensagem(sw, "PRIVMSG #frosticecold :OLA PESSOAL");


            while (true) ;
            sw.Close();
            sr.Close();
            stream.Close();
            client.Close();
        }

        public static void OutputToConsole(StreamReader sr)
        {
            Console.WriteLine("Thread2");
            string s_data;
            //while ((s_data = sr.ReadLine()) != null)
            while (true)
            {
                s_data = sr.ReadLine();
                Console.WriteLine(s_data);
            }

            return;
        }

        public static void mandarMensagem(StreamWriter sw, string msg)
        {
            sw.Write(msg);
            sw.Flush();
        }
        static Byte[] convertString(string message)
        {
            return Encoding.ASCII.GetBytes(message);
        }
    }


}
