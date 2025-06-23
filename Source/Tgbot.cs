using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace TGBot.Source
{

    internal class Tgbot
    {
        private readonly TelegramBotClient _botClient;

        public Tgbot(string token)
        {
            _botClient = new TelegramBotClient(token);
        }
        public void Start()
        {
            Console.WriteLine("Бот запущено...");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // отримувати всі типи оновлень
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: default
            );
        }
        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message is { Text: { } messageText })
            {
                long chatId = update.Message.Chat.Id;

                Console.WriteLine($"Отримано повідомлення: {messageText}");

                // await bot.SendMessage
                await bot.SendMessage(
                    chatId: chatId,
                    text: $"Ти написав: {messageText}",
                    cancellationToken: token
                );
            }
        }
        private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken token)
        {
            var errorMessage = ex switch
            {
                ApiRequestException apiEx => $"Telegram API Error:\n[{apiEx.ErrorCode}] {apiEx.Message}",
                _ => ex.ToString()
            };

            Console.WriteLine($"❌ {errorMessage}");
            return Task.CompletedTask;
        }
    }
}
