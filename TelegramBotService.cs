using System;

    public class TelegramBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly Database _database;
        private readonly string _taskHelp = "help";
        private readonly string _taskDone = "Окей, будет сделано👍";

        public TelegramBotService(string botToken, Database database)
        {
            _botClient = new TelegramBotClient(botToken);
            _database = database;
        }

        public async Task StartAsync()
        {
            await _botClient.DeleteWebhookAsync();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);
        }

        private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            try
            {
                if (update.Message?.Text != null)
                {
                    var user = update.Message.From;
                    long chatId = update.Message.Chat.Id;
                    string messageText = update.Message.Text;

                    if (messageText.ToLower().StartsWith(_taskHelp))
                    {
                        await HandleHelpCommand(chatId, messageText, user);
                        return;
                    }

                    await _database.AddMessageAsync(user, chatId, messageText);

                    if (messageText.Contains("/start"))
                    {
                        await SendStartMessage(chatId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update Error: {ex.Message}");
            }
        }

        private async Task HandleHelpCommand(long chatId, string messageText, User user)
        {
            var pattern = @"^help\s+([^\d]+)\s+(\d+)\s+(.+)$";
            var match = Regex.Match(messageText, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string lastName = match.Groups[1].Value.Trim();
                string cabinetNumber = match.Groups[2].Value.Trim();
                string description = match.Groups[3].Value.Trim();

                var cabinetExists = _database.CheckCabinetExists(cabinetNumber);
                if (!cabinetExists)
                {
                    await _botClient.SendTextMessageAsync(chatId, "⚠️ Внимание: Такой кабинет не зарегистрирован в системе!");
                    return;
                }

                await _database.AddTaskMessageAsync(user, chatId, lastName, cabinetNumber, description);
                await _botClient.SendTextMessageAsync(chatId, _taskDone);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId,
                    "❌ Пожалуйста, укажите фамилию и номер кабинета в формате:\n" +
                    "help [Фамилия] [Кабинет] [Описание проблемы]\n" +
                    "Пример: help Иванов 404 Не печатает принтер");
            }
        }

        private async Task SendStartMessage(long chatId)
        {
            await _botClient.SendTextMessageAsync(chatId,
                "Здравствуйте! Для создания заявки используйте формат:\n" +
                "help [Фамилия] [Номер кабинета] [Описание проблемы]\n" +
                "Пример: help Иванов 404 Не работает принтер\n" +
                "Убедитесь, что указали фамилию и номер кабинета!");
        }

        private Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }