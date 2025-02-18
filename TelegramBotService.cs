public class TelegramBotService
{
    private readonly ITelegramCommand[] _commands;
    private readonly TelegramBotClient _botClient;
    private readonly IMessageService _messageService;

    public TelegramBotService(
        string botToken,
        IMessageService messageService,
        params ITelegramCommand[] commands)
    {
        _botClient = new TelegramBotClient(botToken);
        _messageService = messageService;
        _commands = commands;
    }

    public async Task StartAsync()
    {
        await _botClient.DeleteWebhookAsync();
        var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
        _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        if (update.Message?.Text == null) return;

        var command = _commands.FirstOrDefault(c => c.CanExecute(update));
        if (command != null)
        {
            await command.ExecuteAsync(update);
            await _messageService.AddMessageAsync(update.Message.From, update.Message.Chat.Id, update.Message.Text);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
    {
        // Логирование ошибки
        return Task.CompletedTask;
    }
}