using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using TGBot.Source;



namespace TGBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appconfig.json", optional: false)
            .Build();


            var token = config["TelegramBotToken"];

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("appconfig.json is empty");
                return;
            }


            var bot = new Tgbot(token);
            bot.Start();


            Console.ReadLine();
        }

    }
}
