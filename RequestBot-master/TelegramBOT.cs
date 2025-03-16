using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

class Program
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly string botToken = "7299350943:AAGLJMmjGTquwrc1wnsAMJjWI8gntuRHr7A";
    private static readonly string appUrl = "http://localhost:5000/receive";

    static async Task Main(string[] args)
    {
        var botClient = new TelegramBotClient(botToken);

        botClient.StartReceiving(Update, Error);

        Console.WriteLine("Bot started...");
        Console.ReadLine();
    }

    private static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message != null)
        {
            var text = update.Message.Text;
            Console.WriteLine($"Received: {text}");

            // Отправляем текст в Windows Forms приложение
            var content = new StringContent($"{{ \"text\": \"{text}\" }}", System.Text.Encoding.UTF8, "application/json");
            await httpClient.PostAsync(appUrl, content);
        }
    }

    private static Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}