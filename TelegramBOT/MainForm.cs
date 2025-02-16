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

namespace TelegramBOT
{
    
    public partial class MainForm : Form
    {
        private string client = ConfigurationManager.AppSettings["BotToken"];// токен в конфиге(App.Config)
        private TelegramBotClient botClient;
        private readonly Database _database;
        private string taskHelp = "help";
        private string taskDone = "Окей, будет сделано👍";
        private string currentUsername;
        public MainForm()
        {
            InitializeComponent();
            this.BackColor = System.Drawing.Color.White;
            _database = new Database("messages.db");
            LoadUserAccounts(); // Загрузка аккаунтов
            listView1.ItemSelectionChanged += ListView1_ItemSelectionChanged; // Подписка на событие
            AppendToTextBox("Запуск бота...");
            botClient = new TelegramBotClient(client);

            // Удаляем вебхук
            botClient.DeleteWebhookAsync().Wait();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.StartReceiving(Update, Error, receiverOptions);
            AppendToTextBox("Бот запущен и ожидает сообщений...");
            _database.MessageAdded += () =>
            {
                // Обновляем список при добавлении нового сообщения
                LoadUserAccounts();
            };
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
            // Добавляем проверку Invoke
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(DisplayUserMessages), username);
                return;
            }

            textBox1.Clear();
            var messages = _database.GetMessagesByUsername(username);
            foreach (var message in messages)
            {
                textBox1.AppendText(message + Environment.NewLine);
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
            listView1.View = View.Details;
            listView1.OwnerDraw = true;
            listView1.FullRowSelect = true;

            // Исправленный обработчик
            listView1.DrawItem += (s, args) =>
            {
                args.DrawBackground();

                Font font = listView1.Font;
                args.Graphics.DrawString(
                    args.Item.Text,
                    font,
                    SystemBrushes.ControlText,
                    args.Bounds
                );

                args.DrawFocusRectangle();
            };
            listView1.View = View.Details;
            // Добавление колонки
            listView1.Columns.Add("", listView1.Width - 4);

            // Убрать заголовок колонки
            listView1.HeaderStyle = ColumnHeaderStyle.None;
        }

        // Метод для безопасного обновления TextBox
        private void AppendToTextBox(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(AppendToTextBox), text);
            }
            else
            {
                textBox1.AppendText(text + Environment.NewLine);
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
            foreach (Form form in Application.OpenForms)
            {
                if (form is Tasks)
                {
                    break;
                }
            }
            Tasks tasksForm = new Tasks(_database);
            tasksForm.Show();
        }
        private void LoadUserAccounts()
        {
            // Используем Invoke для потокобезопасности
            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new Action(LoadUserAccounts));
                return;
            }

            listView1.Items.Clear();
            var usernames = _database.GetUniqueUsernames();

            foreach (var username in usernames)
            {
                // Проверяем, существует ли уже пользователь в списке
                if (!listView1.Items.Cast<ListViewItem>().Any(i => i.Text == username))
                {
                    listView1.Items.Add(new ListViewItem(username));
                }
            }
            if (listView1.Items.Count > 0)
            {
                listView1.Items[listView1.Items.Count - 1].EnsureVisible();
            }
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

        private void button3_Click(object sender, EventArgs e)
        {
            // Кнопка для перехода на форму склада предприятия, какое оборудование стоит в каком кабинете, кто работает
            var equipmentForm = new EquipmentForm(_database);
            equipmentForm.Show();
        }
    }


    public class Database
    {
        public event Action MessageAdded;
        private readonly string _connectionString;

        public Database(string dbPath)
        {
            _connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        public async Task AddTaskMessageAsync(User user, long chatId, string lastName, string cabinet, string description)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Сохраняем пользователя
                var userCommand = new SQLiteCommand(
                    @"INSERT OR REPLACE INTO Users 
            (Username, FirstName, LastName) 
            VALUES (@username, @firstName, @lastName)",
                    connection);
                userCommand.Parameters.AddWithValue("@username", user.Username ?? "");
                userCommand.Parameters.AddWithValue("@firstName", user.FirstName ?? "");
                userCommand.Parameters.AddWithValue("@lastName", lastName); // Используем фамилию из сообщения
                await userCommand.ExecuteNonQueryAsync();

                // Сохраняем задачу с дополнительными полями
                var messageCommand = new SQLiteCommand(
        @"INSERT INTO Messages 
        (Username, ChatId, MessageText, IsTask, Status, LastName, CabinetNumber) 
        VALUES (@username, @chatId, @messageText, 1, 'Pending', @lastName, @cabinet)",
        connection);

                messageCommand.Parameters.AddWithValue("@username", user.Username ?? "");
                messageCommand.Parameters.AddWithValue("@chatId", chatId);
                messageCommand.Parameters.AddWithValue("@messageText", description);
                messageCommand.Parameters.AddWithValue("@lastName", lastName);
                messageCommand.Parameters.AddWithValue("@cabinet", cabinet);

                await messageCommand.ExecuteNonQueryAsync();
            }
            MessageAdded?.Invoke();
        }

        // Инициализация базы данных (создание таблицы, если её нет)
        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(@"
            CREATE TABLE IF NOT EXISTS Users (
                Username TEXT PRIMARY KEY,
                FirstName TEXT,
                LastName TEXT
            );

            CREATE TABLE IF NOT EXISTS Messages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                ChatId INTEGER NOT NULL,
                MessageText TEXT NOT NULL,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                IsTask BOOLEAN DEFAULT 0,
                Status TEXT DEFAULT 'None',
                LastName TEXT,
                CabinetNumber TEXT
            );

            CREATE TABLE IF NOT EXISTS Cabinets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Number TEXT NOT NULL UNIQUE,
                Description TEXT
            );

            CREATE TABLE IF NOT EXISTS Equipment (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Type TEXT NOT NULL,
                Model TEXT NOT NULL,
                OS TEXT,
                CabinetId INTEGER,
                FOREIGN KEY(CabinetId) REFERENCES Cabinets(Id)
            );

            CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Position TEXT,
                CabinetId INTEGER,
                Username TEXT,
                FOREIGN KEY(CabinetId) REFERENCES Cabinets(Id),
                FOREIGN KEY(Username) REFERENCES Users(Username)
            );", connection);
                command.ExecuteNonQuery();
            }
        }


        public async Task AddMessageAsync(string username, long chatId, string messageText)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    "INSERT INTO Messages (Username, ChatId, MessageText) VALUES (@username, @chatId, @messageText)",
                    connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@chatId", chatId);
                command.Parameters.AddWithValue("@messageText", messageText);

                await command.ExecuteNonQueryAsync();
            }
        }


        // Получение всех сообщений из базы данных
        public List<MessageData> GetAllMessages()
        {
            var messages = new List<MessageData>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT * FROM Messages", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            messages.Add(new MessageData
                            {
                                Username = reader["Username"].ToString(),
                                Text = reader["MessageText"].ToString(),
                                Timestamp = DateTime.Parse(reader["Timestamp"].ToString())
                            });
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        
                        // Заполняем список данными вместо вывода в TextBox
                        
                    }
                }
            }
            return messages;
        }
        public class MessageData
        {
            public string Username { get; set; }
            public string Text { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public List<string> GetUniqueUsernames()
        {
            var usernames = new List<string>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT DISTINCT Username FROM Messages", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usernames.Add(reader.GetString(0)); // Получаем имя пользователя
                    }
                }
            }

            return usernames;
        }

        public List<string> GetMessagesByUsername(string username)
        {
            var messages = new List<string>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT MessageText, Timestamp FROM Messages WHERE Username = @username", connection);
                command.Parameters.AddWithValue("@username", username);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string message = $"{reader["Timestamp"]}: {reader["MessageText"]}"; // Форматирование сообщения
                        messages.Add(message);
                    }
                }
            }

            return messages;
        }
        public bool CheckCabinetExists(string cabinetNumber)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "SELECT COUNT(*) FROM Cabinets WHERE Number = @number",
                    connection);
                cmd.Parameters.AddWithValue("@number", cabinetNumber);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public List<Cabinet> GetAllCabinets()
        {
            var cabinets = new List<Cabinet>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Получаем кабинеты
                var cmd = new SQLiteCommand("SELECT * FROM Cabinets", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cabinet = new Cabinet
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Number = reader["Number"].ToString(),
                            Description = reader["Description"].ToString(),
                            Equipment = new List<Equipment>(),
                            Employees = new List<Employee>()
                        };
                        cabinets.Add(cabinet);
                    }
                }

                // Заполняем оборудование
                foreach (var cabinet in cabinets)
                {
                    cmd = new SQLiteCommand(
                        "SELECT * FROM Equipment WHERE CabinetId = @id",
                        connection);
                    cmd.Parameters.AddWithValue("@id", cabinet.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cabinet.Equipment.Add(new Equipment
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Type = reader["Type"].ToString(),
                                Model = reader["Model"].ToString(),
                                OS = reader["OS"].ToString(),
                                CabinetId = cabinet.Id
                            });
                        }
                    }
                }

                // Заполняем сотрудников
                foreach (var cabinet in cabinets)
                {
                    cmd = new SQLiteCommand(
                        "SELECT * FROM Employees WHERE CabinetId = @id",
                        connection);
                    cmd.Parameters.AddWithValue("@id", cabinet.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cabinet.Employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Position = reader["Position"].ToString(),
                                CabinetId = cabinet.Id,
                                Username = reader["Username"].ToString()
                            });
                        }
                    }
                }
            }
            return cabinets;
        }

        public void AddCabinet(Cabinet cabinet)
        {
            if (string.IsNullOrWhiteSpace(cabinet.Number))
            {
                throw new ArgumentException("Номер кабинета не может быть пустым!");
            }

            // Проверка существования кабинета перед добавлением
            if (CheckCabinetExists(cabinet.Number))
            {
                throw new ArgumentException($"Кабинет с номером {cabinet.Number} уже существует!");
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "INSERT INTO Cabinets (Number, Description) VALUES (@num, @desc)",
                    connection);
                cmd.Parameters.AddWithValue("@num", cabinet.Number);
                cmd.Parameters.AddWithValue("@desc", cabinet.Description ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
        public class TaskData
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string CabinetNumber { get; set; }
            public string Username { get; set; }
            public string MessageText { get; set; }
            public string Status { get; set; }
            public DateTime Timestamp { get; set; } // Добавлено
        }

        public List<TaskData> GetAllTasks()
        {
            var tasks = new List<TaskData>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    @"SELECT M.Id,
                    M.Username,
                    M.MessageText,
                    M.Status, 
                    M.LastName,
                    M.CabinetNumber,
                    U.FirstName,
                    M.Timestamp
              FROM Messages M
              LEFT JOIN Users U ON M.Username = U.Username
              WHERE M.IsTask = 1", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskData
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"].ToString(),
                            MessageText = reader["MessageText"].ToString(),
                            Status = reader["Status"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            CabinetNumber = reader["CabinetNumber"].ToString(),
                            Timestamp = DateTime.Parse(reader["Timestamp"].ToString()) // Добавлено
                        });
                    }
                }
            }
            return tasks;
        }


        public void UpdateTaskStatus(int taskId, string status)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "UPDATE Messages SET Status = @status WHERE Id = @id",
                    connection);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@id", taskId);
                command.ExecuteNonQuery();
            }
        }

        // Обновим метод AddMessageAsync
        public async Task AddMessageAsync(User user, long chatId, string messageText)
        {
            bool isTask = messageText.Trim().ToLower().StartsWith("help");

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Сохраняем пользователя
                var userCommand = new SQLiteCommand(
                    @"INSERT OR REPLACE INTO Users 
            (Username, FirstName, LastName) 
            VALUES (@username, @firstName, @lastName)",
                    connection);
                userCommand.Parameters.AddWithValue("@username", user.Username ?? "");
                userCommand.Parameters.AddWithValue("@firstName", user.FirstName ?? "");
                userCommand.Parameters.AddWithValue("@lastName", user.LastName ?? "");
                await userCommand.ExecuteNonQueryAsync();

                // Сохраняем сообщение
                var messageCommand = new SQLiteCommand(
                    @"INSERT INTO Messages 
            (Username, ChatId, MessageText, IsTask, Status) 
            VALUES (@username, @chatId, @messageText, @isTask, @status)",
                    connection);

                messageCommand.Parameters.AddWithValue("@username", user.Username ?? "");
                messageCommand.Parameters.AddWithValue("@chatId", chatId);
                messageCommand.Parameters.AddWithValue("@messageText", messageText);
                messageCommand.Parameters.AddWithValue("@isTask", isTask);
                messageCommand.Parameters.AddWithValue("@status", isTask ? "Pending" : "None");

                await messageCommand.ExecuteNonQueryAsync();
            }
        }

        public void DeleteTask(int taskId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "DELETE FROM Messages WHERE Id = @id",
                    connection);
                command.Parameters.AddWithValue("@id", taskId);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateEquipment(Equipment equipment)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE Equipment SET 
                Type = @type, 
                Model = @model, 
                OS = @os 
            WHERE Id = @id", connection);

                cmd.Parameters.AddWithValue("@type", equipment.Type);
                cmd.Parameters.AddWithValue("@model", equipment.Model);
                cmd.Parameters.AddWithValue("@os", equipment.OS);
                cmd.Parameters.AddWithValue("@id", equipment.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteEquipment(int equipmentId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "DELETE FROM Equipment WHERE Id = @id", connection);
                cmd.Parameters.AddWithValue("@id", equipmentId);
                cmd.ExecuteNonQuery();
            }
        }

        // Данные для реализации кабинетов, сотрудников и тд

        public class Cabinet
        {
            public int Id { get; set; }
            public string Number { get; set; }
            public string Description { get; set; }
            public List<Equipment> Equipment { get; set; }
            public List<Employee> Employees { get; set; }
        }

        public class Equipment
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public string Model { get; set; }
            public string OS { get; set; }
            public int CabinetId { get; set; }
        }

        public class Employee
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Position { get; set; }
            public int CabinetId { get; set; }
            public string Username { get; set; }
        }

        public void AddEquipment(Equipment equipment)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "INSERT INTO Equipment (Type, Model, OS, CabinetId) " +
                    "VALUES (@type, @model, @os, @cid)", connection);
                cmd.Parameters.AddWithValue("@type", equipment.Type);
                cmd.Parameters.AddWithValue("@model", equipment.Model);
                cmd.Parameters.AddWithValue("@os", equipment.OS);
                cmd.Parameters.AddWithValue("@cid", equipment.CabinetId);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateCabinet(Cabinet cabinet)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE Cabinets 
            SET Number = @num, 
                Description = @desc 
            WHERE Id = @id",
                    connection);

                cmd.Parameters.AddWithValue("@num", cabinet.Number);
                cmd.Parameters.AddWithValue("@desc", cabinet.Description);
                cmd.Parameters.AddWithValue("@id", cabinet.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddEmployee(Employee employee)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "INSERT INTO Employees (FirstName, LastName, Position, CabinetId, Username) " +
                    "VALUES (@fn, @ln, @pos, @cid, @user)", connection);

                cmd.Parameters.AddWithValue("@fn", employee.FirstName);
                cmd.Parameters.AddWithValue("@ln", employee.LastName);
                cmd.Parameters.AddWithValue("@pos", employee.Position);
                cmd.Parameters.AddWithValue("@cid", employee.CabinetId);
                cmd.Parameters.AddWithValue("@user", employee.Username ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateEmployee(Employee employee)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    @"UPDATE Employees SET 
                FirstName = @fn,
                LastName = @ln,
                Position = @pos,
                Username = @user
            WHERE Id = @id", connection);

                cmd.Parameters.AddWithValue("@fn", employee.FirstName);
                cmd.Parameters.AddWithValue("@ln", employee.LastName);
                cmd.Parameters.AddWithValue("@pos", employee.Position);
                cmd.Parameters.AddWithValue("@user", employee.Username ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id", employee.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteEmployee(int employeeId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "DELETE FROM Employees WHERE Id = @id", connection);
                cmd.Parameters.AddWithValue("@id", employeeId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<string> GetAllUsernames()
        {
            var usernames = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT Username FROM Users", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usernames.Add(reader["Username"].ToString());
                    }
                }
            }
            return usernames;
        }
    }
}