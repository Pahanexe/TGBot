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
        private readonly MySQLdb _mydb;
        public Tgbot(string token,string conectionSTR)
        {
            _botClient = new TelegramBotClient(token);
            _mydb = new MySQLdb(conectionSTR); 
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

        private async Task Text_Message_Handler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            long chatId = update.Message.Chat.Id;
            Message message = update.Message;
            string messageText = message.Text;

            await bot.SendMessage(
                chatId: chatId,
                text: $"Ти написав: {messageText}",
                cancellationToken: token
            );
            Console.WriteLine($"Отримано повідомлення: {messageText}");
        }
        private async Task Photo_Message_Handler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            var message = update.Message!; 
            var chatId = message.Chat.Id;

            
            var photo = message.Photo!.Last();
            var fileId = photo.FileId;

            
            var file = await bot.GetFile(fileId, cancellationToken: token);

            
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            
            string filePath = Path.Combine(folderPath, $"{file.FileUniqueId}.jpg");

            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await bot.DownloadFile(file.FilePath!, fileStream, cancellationToken: token);
            }
            long imageid = await _mydb.SaveImageAsync(photo.FileId, folderPath);

            await bot.SendMessage(chatId, "✅ Фото збережено локально!", cancellationToken: token);

            Console.WriteLine($"📁 Зображення збережено: {filePath}");
        }

        private List<string> ExtractTagsFromCaption(string caption)
        {
            return caption.Split(' ', '\n', ',', ';')
                .Where(word => word.StartsWith("#") && word.Length > 1)
                .Select(tag => tag.TrimStart('#').ToLowerInvariant())
                .Distinct()
                .ToList();
        }
    }
}
