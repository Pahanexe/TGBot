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
using static System.Net.Mime.MediaTypeNames;

namespace TGBot.Source
{

    internal class Tgbot
    {
        private readonly TelegramBotClient _botClient;
        private readonly MySQLdb _mydb;
        public Tgbot(string token, string conectionSTR)
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
            if (update.Type == UpdateType.Message && update.Message is not null)
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

            List<string> strarr = message.Text.Split(' ', '\n', ',', ';').Where(word => word.Length > 1).ToList();

            switch (strarr[0])
            {
                case "/start":
                    await bot.SendMessage(chatId, "Привіт! Я твій Telegram бот. Надішли мені фото з тегами у форматі #тег1 #тег2, або просто текстове повідомлення.");
                    break;
                case "/help":
                    await bot.SendMessage(chatId, "Щоб працювати із ботом, надішли зображення з підписом в якому містяться теги по типу #тег1 або #тег2. ");
                    await bot.SendMessage(chatId, "Потім із допомогою команди /get витягни всі картнки за тегом.");
                    break;


                case "/get":
                    if (strarr.Count < 2)
                    {
                        await bot.SendMessage(chatId, "Використай команду у форматі: /get #тег");
                        break;
                    }

                    string rawTag = strarr[1];
                    string tag = rawTag.TrimStart('#').ToLowerInvariant();

                    var images = await _mydb.FindImagesByTagAsync(tag);

                    if (images.Count == 0)
                    {
                        await bot.SendMessage(chatId, $"Нічого не знайдено за тегом: #{tag}");
                        break;
                    }

                    foreach (var (fileId, _) in images)
                    {
                        await bot.SendPhoto(chatId, fileId, cancellationToken: token);
                    }

                    break;
                default:
                    await bot.SendMessage(
                chatId: chatId,
                text: "Мені не знайома ця команда. Напиши /help щоб ознайомитись із можливостями бота.",
                cancellationToken: token
            );
                    break;

            }


            Console.WriteLine($"Отримано повідомлення: {messageText}");
        }
        private async Task Photo_Message_Handler(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            var message = update.Message!;
            var chatId = message.Chat.Id;

            if (string.IsNullOrWhiteSpace(message.Caption))
            {
                await _botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Будь ласка, надішли фото з підписом, який містить теги у форматі #тег1 #тег2");

                Console.WriteLine("Отримано фото без підпису.");
                return; // нічого не робимо далі
            }

            var photo = message.Photo!.Last();
            var fileId = photo.FileId;


            var file = await bot.GetFile(fileId, cancellationToken: token);


            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);


            string filePath = Path.Combine(folderPath, $"{file.FileUniqueId}.jpg");




            long imageid = await _mydb.SaveImageAsync(photo.FileId, folderPath);

            var tags = ExtractTagsFromCaption(message.Caption!);
            if (tags.Any())
            {
                await _mydb.AddTagsAsync(imageid, tags);

                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Фото збережено з тегами: {string.Join(", ", tags)}");
            }
            else
            {
                await _botClient.SendMessage(
                    message.Chat.Id,
                    $"Підпис не містить жодного тегу у форматі #тег. Спробуй ще раз.");
                return;
            }
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await bot.DownloadFile(file.FilePath!, fileStream, cancellationToken: token);
            }
            await bot.SendMessage(chatId, "✅ Фото збережено локально!", cancellationToken: token);

            Console.WriteLine($" Зображення збережено: {filePath}");
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
