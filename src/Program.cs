using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MightyPecoBot.Network;
using static MightyPecoBot.BotLogger;

namespace MightyPecoBot
{
    class Program
    {
        static string USERNAME = "mightypecobot";
        static string CHANNEL = "frosticecold";

        static volatile bool running = true;
        static void Main(string[] args)
        {
            BotLogger.LogLevel = LOG_LEVEL.DEBUG;
            string OATH_TOKEN = System.IO.File.ReadAllText("oath.txt");
            TwitchBot clientbot = new TwitchBot(USERNAME, CHANNEL);
            clientbot.Connect(OATH_TOKEN);
            clientbot.Debug();
            Thread.Sleep(500);

            while (clientbot.Running)
            {
                Console.ReadLine();
            }
            // Thread thread;
            // thread = new Thread(() => OutputToConsole(Socket));
            // thread.IsBackground = true;
            // thread.Start();






            // while (running)
            // {
            //     string read = null;
            //     if ((read = Console.ReadLine()) != null)
            //     {
            //         Console.WriteLine(read);
            //         sendToChannel(Socket, read);
            //     }
            // }

            // Console.WriteLine("Exiting application...");
            // thread.Interrupt();
            // if (!thread.Join(250))
            // {
            //     thread.Abort();
            // }

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



    }


}
