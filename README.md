# MightyPecoBot
MightyPecoBot is a TwitchBot implemented from scratch.  
It uses dotnet core 2.0

## Installation

Clone the repository and copy the classes to your .Net Project
```bash
git clone https://github.com/raulcorreia7/pecobot
```

## Usage

```csharp
using MightyPecoBot;

class Program
    {
        //Your username
        const string USERNAME = "mightypecobot";
        //Your channel
        const string CHANNEL = "frosticecold";

        static void Main(string[] args)
        {
            //Select waht log level you want (optional = NORMAL)
            //BotLogger.LogLevel = LOG_LEVEL.DEBUG;

            //Read or provide your own OAUTH_TOKEN
            string OAUTH_TOKEN = System.IO.File.ReadAllText("oath.txt");
            //Create the bot
            TwitchBot clientbot = new TwitchBot(USERNAME, CHANNEL);
            //Connect
            clientbot.Connect(OAUTH_TOKEN);
            //Send debug hello world!
            clientbot.Debug();

            //Optional to read input from the console
            while (clientbot.Running)
            {
                string data = Console.ReadLine();
                if (!string.IsNullOrEmpty(data))
                    clientbot.SendToIRC(data);
            }
        }
    }

```

## Adding your own behaviour

```csharp
using MightyPecoBot.Callbacks;
using MightyPecoBot.Parsing;
public Program{
    static void Main(string[] args)
    {
        TwitchBot clientbot = new TwitchBot(USERNAME, CHANNEL);
        //Connect
        clientbot.Connect(YOUR_OWN_OAUTH_TOKEN);
        clientbot.OnChannelMessage((ChannelMessageEvent message) =>
            {   
                //You either use your default channel or the message responses channel
                clientbot.SendToChannel(message.Channel, "Hello this is your custom message!");
                return CallbackAction.CONTINUE;
            });
            clientbot.OnJoinChannel((UserActionUponChannel information) =>
            {
                clientbot.SendToChannel(information.Channel, "Hello " + information.Username + "!");
                return CallbackAction.CONTINUE;
            });
            clientbot.OnLeaveChannel((UserActionUponChannel information) =>
            {
                clientbot.SendToChannel(information.Channel, "Byebye " + information.Username);
                return CallbackAction.CONTINUE;
            });
            clientbot.OnRaidingEvent((RaidingEvent raidingevent) =>
            {
                clientbot.SendToChannel(raidingevent.RaidedChannel, "HELP! Were are being raided by: " + raidingevent.RaiderChannel);
                return CallbackAction.SKIP_OTHERS;
            });
            clientbot.OnSubGift((GiftEvent g)=>{
                clientbot.SendToChannel(clientbot.DefaultChannel,g.Message);
                return CallbackAction.CONTINUE;
            });
            clientbot.OnSubscribe((SubscriptionEvent subevent)=>{
                clientbot.SendToChannel(subevent.Channel,"WHOWHO! " + subevent.Username + " subscribed for: " + subevent.CommulativeMonths + " months.");
                return CallbackAction.SKIP_OTHERS;
            });

    }
}
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://choosealicense.com/licenses/mit/)