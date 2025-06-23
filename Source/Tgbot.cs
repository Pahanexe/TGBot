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
            var chatident = update.Message?.Chat.Id ?? 0;
            //update.Message is { Text: { } messageText }
            if (update.Type==UpdateType.Message && update.Message is not null)
            {

                var message = update.Message;

                switch (message.Type)
                {
                    case MessageType.Text:
                        await Text_Message_Handler(bot, update, token);
                        break;
                    case MessageType.Photo:
                        await Photo_Message_Handler(bot, update, token);
                        break;
                }

                update.Message.GetType();
                long chatId = update.Message.Chat.Id;

                //Console.WriteLine($"Отримано повідомлення: {messageText}");
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

        private void Command_Handler()
        {

        }
        private async Task Text_Message_Handler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            long chatId = update.Message.Chat.Id;
            Message message = update.Message;
            string messageText = message.Text;

            

            // await bot.SendMessage
            await bot.SendMessage(
                chatId: chatId,
                text: $"Ти написав: {messageText}",
                cancellationToken: token
            );
            Console.WriteLine($"Отримано повідомлення: {messageText}");
            //return Task.CompletedTask;
        }
        private async Task Photo_Message_Handler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            var message = update.Message!; 
            var chatId = message.Chat.Id;

            // 1. Отримуємо найбільше фото (найвища якість)
            var photo = message.Photo!.Last();
            var fileId = photo.FileId;

            // 2. Отримуємо інформацію про файл на сервері Telegram
            
            var file = await bot.GetFile(fileId, cancellationToken: token);

            // 3. Визначаємо папку для збереження
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // 4. Формуємо повний шлях для збереження файлу
            string filePath = Path.Combine(folderPath, $"{file.FileUniqueId}.jpg");

            // 5. Скачуємо файл з Telegram і зберігаємо
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await bot.DownloadFile(file.FilePath!, fileStream, cancellationToken: token);
            }

            // 6. Повідомлення користувачу
            await bot.SendMessage(chatId, "✅ Фото збережено локально!", cancellationToken: token);

            Console.WriteLine($"📁 Зображення збережено: {filePath}");
        }
    }
}
