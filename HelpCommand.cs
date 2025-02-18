public class HelpCommand : ITelegramCommand
{
    private readonly ITaskService _taskService;
    public HelpCommand(ITaskService taskService) => _taskService = taskService;

    public bool CanExecute(Update update) =>
        update.Message?.Text?.Trim().ToLower().StartsWith("help") ?? false;

    public async Task ExecuteAsync(Update update)
    {
        var (lastName, cabinet, description) = ParseHelpCommand(update.Message.Text);
        await _taskService.CreateTaskAsync(update.Message.From, update.Message.Chat.Id, lastName, cabinet, description);
        await _botClient.SendTextMessageAsync(update.Message.Chat.Id, "Окей, будет сделано 👍");
    }

    private (string, string, string) ParseHelpCommand(string text) { ... }
}