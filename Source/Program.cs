using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;



namespace TGBot.Source
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
            .Build();

            

            string token = configuration["TelegramBotToken"];
            var botClient = new TelegramBotClient(token);
        }
    }
}
