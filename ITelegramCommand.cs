public interface ITelegramCommand
{
    bool CanExecute(Update update);
    Task ExecuteAsync(Update update);
}