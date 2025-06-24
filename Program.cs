using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
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
            var connStr = config.GetConnectionString("Default");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("appconfig.json is empty");
                return;
            }
            if (string.IsNullOrEmpty(connStr))
            {
                Console.WriteLine("appconfig.json is empty");
                return;
            }

            var bot = new Tgbot(token,connStr);
            bot.Start();


            Console.ReadLine();
        }

    }
}
