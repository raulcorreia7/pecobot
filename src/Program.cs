using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MightyPecoBot.Network;
using MightyPecoBot.Parsing;
using static MightyPecoBot.BotLogger;

namespace MightyPecoBot
{
    class Program
    {
        /*
            Your username
         */
        const string USERNAME = "mightypecobot";

        /*
            Your Channel
         */
        const string CHANNEL = "frosticecold";

        static volatile bool running = true;
        static void Main(string[] args)
        {
            /*
                Select what log level you want
             */
            BotLogger.LogLevel = LOG_LEVEL.DEBUG;
            /*
                You need to provide your own token
             */
            string OATH_TOKEN = System.IO.File.ReadAllText("oath.txt");
            TwitchBot clientbot = new TwitchBot(USERNAME, CHANNEL);
            clientbot.Connect(OATH_TOKEN);
            clientbot.Debug();
            Thread.Sleep(500);

            while (clientbot.Running)
            {
                //Console.ReadKey();
                string data = Console.ReadLine();
                if (!string.IsNullOrEmpty(data))
                    clientbot.SendToIRC(data);
            }
        }

    }


}
