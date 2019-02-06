using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MightyPecoBot.Callbacks;
using MightyPecoBot.Parsing;

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

        static void Main(string[] args)
        {
            /*
                Select what log level you want
             */
            //BotLogger.LogLevel = LOG_LEVEL.DEBUG;
            /*
                You need to provide your own token
                https://twitchapps.com/tmi/ 
             */
            string OATH_TOKEN = System.IO.File.ReadAllText("oath.txt");
            TwitchBot clientbot = new TwitchBot(USERNAME, CHANNEL);
            clientbot.Connect(OATH_TOKEN);
            clientbot.Debug();

            while (clientbot.Running)
            {
                string data = Console.ReadLine();
                if (!string.IsNullOrEmpty(data))
                    clientbot.SendToIRC(data);
            }
        }

    }


}
