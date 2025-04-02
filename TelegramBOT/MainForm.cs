    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SQLite;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Telegram.Bot;
    using Telegram.Bot.Polling;
    using Telegram.Bot.Types;
    using Telegram.Bot.Types.Enums;
    using System.Configuration;
    using PrinterRepairMaster;
    using System.Drawing;
    using System.Text.RegularExpressions;
using TelegramBOT.Reports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;

    namespace TelegramBOT
    {
        public partial class MainForm : Form
        {
            private string client = ConfigurationManager.AppSettings["BotToken"];// токен в конфиге(App.Config)
            private TelegramBotClient botClient;
            private readonly Database _database;
            private string taskHelp = "help";
            private string taskDone = "Заявка принята в работу👍";
            private string currentUsername;

        public MainForm()
        {
            InitializeComponent(); // Только ОДИН вызов!
            this.Activated += (s, e) => this.Opacity = 1;
            //this.Shown += (s, e) => Console.WriteLine("MainForm shown!");
            //this.VisibleChanged += (s, e) => Console.WriteLine($"Visible: {this.Visible}");
            //this.WindowState = FormWindowState.Normal;
            // Подписка на событие нажатия клавиш в поле ввода
            tbSendMessage.KeyDown += tbSendMessage_KeyDown;

            // Проверка пути БД
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyApp",//База данных находится в папке %appdata%/MyApp
                "messages.db"
            );
            //MessageBox.Show($"База данных будет создана здесь: {dbPath}"); // Для отладки

            _database = new Database(dbPath);

            // Настройка ListView
            listViewUsers.View = View.Details;
            listViewUsers.Columns.Add("Пользователи", listViewUsers.Width - 25); // Ширина колонки
            listViewUsers.HeaderStyle = ColumnHeaderStyle.None;

            // Загрузка данных
            LoadUserAccounts();

            // Подписка на события
            listViewUsers.ItemSelectionChanged += ListView1_ItemSelectionChanged;

            // Запуск бота
            AppendToTextBox("Запуск бота...");
            try
            {
                botClient = new TelegramBotClient(client);
                botClient.DeleteWebhookAsync().Wait();
                var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
                botClient.StartReceiving(Update, Error, receiverOptions);
                AppendToTextBox("Бот запущен и ожидает сообщений...");
            }
            catch (Exception ex)
            {
                AppendToTextBox($"Ошибка запуска бота: {ex.Message}");
            }
        }

        private void listViewMethod()
        {
            listViewUsers.DrawItem += (s, args) =>
            {
                // Выбираем цвета в зависимости от состояния
                System.Drawing.Color backColor = args.Item.Selected
                    ? SystemColors.Highlight
                    : SystemColors.Window;

                System.Drawing.Color foreColor = args.Item.Selected
                    ? SystemColors.HighlightText
                    : SystemColors.ControlText;

                // Рисуем фон
                using (var backBrush = new SolidBrush(backColor))
                {
                    args.Graphics.FillRectangle(backBrush, args.Bounds);
                }

                // Рисуем текст
                TextRenderer.DrawText(
                    args.Graphics,
                    args.Item.Text,
                    args.Item.Font,
                    args.Bounds,
                    foreColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                // Рисуем фокус, если нужно
                if (args.Item.Focused && args.Item.Selected)
                {
                    ControlPaint.DrawFocusRectangle(args.Graphics, args.Bounds);
                }
            };
            // Настройка колонок
            listViewUsers.Columns.Add("", listViewUsers.Width - 4);
            listViewUsers.HeaderStyle = ColumnHeaderStyle.None;
        }

        private void LoadUserAccounts()
        {
            try
            {
                if (listViewUsers.InvokeRequired)
                {
                    listViewUsers.Invoke(new Action(LoadUserAccounts));
                    return;
                }

                listViewUsers.Items.Clear();
                var usernames = _database.GetUniqueUsernames();
                //MessageBox.Show($"Найдено пользователей: {usernames.Count}"); // Для отладки

                foreach (var username in usernames)
                {
                    listViewUsers.Items.Add(new ListViewItem(username));
                }
                listViewUsers.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            catch (Exception ex)
            {
                AppendToTextBox($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void ListView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
            {
                if (e.IsSelected) // Проверяем, выбран ли элемент
                {
                    currentUsername = e.Item.Text; // Сохраняем имя пользователя
                    DisplayUserMessages(currentUsername); // Отображаем сообщения этого пользователя
                }
            }

        private void DisplayUserMessages(string username)
        {
            if (textBoxMessages.InvokeRequired)
            {
                textBoxMessages.Invoke(new Action<string>(DisplayUserMessages), username);
                return;
            }

            textBoxMessages.Clear();
            var messages = _database.GetMessagesByUsername(username);
            foreach (var message in messages)
            {
                // Используем обновлённую структуру MessageData
                string prefix = message.IsFromAdmin ? "[Вы] " : "[Пользователь] ";
                string messageLine = $"{prefix}{message.Timestamp:g}: {message.Text}";
                textBoxMessages.AppendText(messageLine + Environment.NewLine);
            }
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
            {
                try
                {
                    if (update.Message?.Text != null)
                    {
                        var user = update.Message.From;
                        long chatId = update.Message.Chat.Id;
                        string messageText = update.Message.Text;

                        if (messageText.ToLower().StartsWith(taskHelp))
                        {
                            // Регулярное выражение для парсинга данных
                            var pattern = @"^help\s+([^\d]+)\s+(\d+)\s+(.+)$";
                            var match = Regex.Match(messageText, pattern, RegexOptions.IgnoreCase);

                            if (match.Success)
                            {
                                string lastName = match.Groups[1].Value.Trim();
                                string cabinetNumber = match.Groups[2].Value.Trim(); // Извлекаем номер кабинета
                                string description = match.Groups[3].Value.Trim();

                                // Проверка существования кабинета
                                var cabinetExists = _database.CheckCabinetExists(cabinetNumber);
                                if (!cabinetExists)
                                {
                                    await botClient.SendTextMessageAsync(chatId,
                                        "⚠️ Внимание: Такой кабинет не зарегистрирован в системе!");
                                    return; // Прерываем выполнение, если кабинет не найден
                                }

                                // Сохраняем задачу с дополнительными полями
                                await _database.AddTaskMessageAsync(user, chatId, lastName, cabinetNumber, description);
                                await botClient.SendTextMessageAsync(chatId, taskDone);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(chatId,
                                    "❌ Пожалуйста, укажите фамилию и номер кабинета в формате:\n" +
                                    "help [Фамилия] [Кабинет] [Описание проблемы]\n" +
                                    "Пример: help Иванов 404 Не печатает принтер");
                            }
                            return;
                        }

                        // Сохраняем сообщение с информацией о пользователе
                        await _database.AddMessageAsync(user, chatId, messageText);

                        // Обновляем UI
                        if (user.Username == currentUsername)
                        {
                            Invoke(new Action(() => DisplayUserMessages(currentUsername)));
                        }

                        if (messageText.ToLower().StartsWith("help"))
                        {
                            await botClient.SendTextMessageAsync(chatId, taskDone);
                        }

                        if (messageText.Contains("/start"))
                        {
                            await botClient.SendTextMessageAsync(chatId,
                                "Здравствуйте! Для создания заявки используйте формат:\n" +
                                "help [Фамилия] [Номер кабинета] [Описание проблемы]\n" +
                                "Пример: help Иванов 404 Не работает принтер\n" +
                                "Убедитесь, что указали фамилию и номер кабинета!");
                        }
                        this.Invoke((MethodInvoker)delegate
                        {
                            LoadUserAccounts();
                            if (user.Username == currentUsername)
                            {
                                DisplayUserMessages(currentUsername);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    AppendToTextBox($"Update Error: {ex.Message}");
                }
            }

            private async Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
            {
                AppendToTextBox($"Ошибка: {exception.Message}");
            }

            private void Form1_Load(object sender, EventArgs e)
            {
                LoadUserAccounts();
                listViewUsers.View = View.Details;
                listViewUsers.OwnerDraw = true;
                listViewUsers.FullRowSelect = true;

                // Исправленный обработчик
                listViewUsers.DrawItem += (s, args) =>
                {
                    args.DrawBackground();

                    Font font = listViewUsers.Font;
                    args.Graphics.DrawString(
                        args.Item.Text,
                        font,
                        SystemBrushes.ControlText,
                        args.Bounds
                    );

                    args.DrawFocusRectangle();
                };
            listViewMethod();
            listViewUsers.View = View.Details;
                // Добавление колонки
                listViewUsers.Columns.Add("", listViewUsers.Width - 4);

                // Убрать заголовок колонки
                listViewUsers.HeaderStyle = ColumnHeaderStyle.None;
            }

            // Метод для безопасного обновления TextBox
            private void AppendToTextBox(string text)
            {
                if (textBoxMessages.InvokeRequired)
                {
                    textBoxMessages.Invoke(new Action<string>(AppendToTextBox), text);
                }
                else
                {
                    textBoxMessages.AppendText(text + Environment.NewLine);
                }
            }

            private void buttonOpenAllMessages_Click(object sender, EventArgs e)
            {
                // Проверяем, есть ли уже открытая форма AllMessages
                foreach (Form form in Application.OpenForms)
                {
                    if (form is AllMessages)
                    {
                        // Если форма открыта, закрываем ее
                        form.Close();
                        break; // Выходим из цикла после закрытия
                    }
                }

                // Создаем и показываем новую форму AllMessages
                AllMessages allMessagesForm = new AllMessages(_database);
                allMessagesForm.Show(); // Теперь мы показываем новую форму
            }

            private void buttonOpenTasks_Click(object sender, EventArgs e)
            {
            Tasks tasksForm = new Tasks(_database);

                // Добавляем обработчик закрытия формы
                tasksForm.FormClosed += (s, ev) =>
            {
                       this.WindowState = FormWindowState.Normal;
                   this.Activate();
                  this.Focus();
                };

                // Открываем как немодальное окно с явным указанием владельца
                tasksForm.Show(this);
                tasksForm.Location = new Point(this.Left + 50, this.Top + 50); // Смещение для видимости
            }
            

            private void починиПринтерToolStripMenuItem_Click(object sender, EventArgs e)
            {
                GamePrinter gamePrinter = new GamePrinter();
                gamePrinter.Show();
            }

            private void зигуратToolStripMenuItem_Click(object sender, EventArgs e)
            {
                Zigurat zigurat = new Zigurat();
                zigurat.Show();
            }

            private void змейкаToolStripMenuItem_Click(object sender, EventArgs e)
            {
                SnakeGame snakeGame = new SnakeGame();
                snakeGame.Show();
            }

        private void btnEquipment_Click(object sender, EventArgs e)
        {
            // Получаем строку подключения из существующей базы данных
            string connectionString = _database.GetConnectionString();

            // Создаем форму с правильными параметрами
            var equipmentForm = new EquipmentForm(_database, connectionString);
            equipmentForm.Owner = this;
            equipmentForm.Show();
            this.Activate();
        }

        private void btnAnalytics_Click(object sender, EventArgs e)
        {
            var analyticsForm = new AnalyticsForm(_database);
            analyticsForm.Show();
        }

        private void pbQrCode_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(pbQrCode, "Отсканируйте QR-код С помощью вашего устройства");
        }

        private async void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentUsername))
            {
                MessageBox.Show("Выберите пользователя из списка!");
                return;
            }

            string message = tbSendMessage.Text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                MessageBox.Show("Введите текст сообщения!");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbSendMessage.Text.Trim()))
            {
                tbSendMessage.Focus();
                return; // Выходим без показа сообщения
            }
            btnSendMessage.Enabled = false;
            try
            {
                long chatId = _database.GetChatIdByUsername(currentUsername);
                if (chatId == 0)
                {
                    MessageBox.Show("Не удалось определить chat_id пользователя!");
                    return;
                }

                // Отправка сообщения через бота
                await botClient.SendTextMessageAsync(chatId, message);

                // Сохранение сообщения в БД
                await _database.AddOutgoingMessageAsync(currentUsername, chatId, message);

                // Обновление интерфейса
                tbSendMessage.Clear();
                DisplayUserMessages(currentUsername);
                tbSendMessage.Focus(); // Возвращаем фокус в поле ввода
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}");
            }
            finally
            {
                btnSendMessage.Enabled = true;
                tbSendMessage.Clear();
                tbSendMessage.Focus();
            }

        }

        private void tbSendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift && !e.Control)
            {
                e.SuppressKeyPress = true; // Блокируем системную обработку

                // Проверяем наличие текста перед отправкой
                if (!string.IsNullOrWhiteSpace(tbSendMessage.Text))
                {
                    btnSendMessage.PerformClick();
                }
                else
                {
                    // Очищаем поле и предотвращаем всплывающие сообщения
                    tbSendMessage.Clear();
                }
            }
        }
    }
}