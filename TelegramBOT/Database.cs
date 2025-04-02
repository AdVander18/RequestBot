using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot.Types;

namespace TelegramBOT
{
    public class Database
    {
        public event Action MessageAdded;
        private readonly string _connectionString;

        public Database(string dbPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _connectionString = $"Data Source={dbPath};Version=3;";
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации БД: {ex.Message}");
                throw;
            }
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
            VALUES (@username, @chatId, @messageText, 1, 'Не завершено', @lastName, @cabinet)",
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
            try
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
                    IsFromAdmin BOOLEAN DEFAULT 0,
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
                    ResponsibleEmployeeId INTEGER,
                    FOREIGN KEY(CabinetId) REFERENCES Cabinets(Id),
                    FOREIGN KEY(ResponsibleEmployeeId) REFERENCES Employees(Id)
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
                    var migrateCmd = new SQLiteCommand(
                        @"UPDATE Messages 
                        SET IsFromAdmin = 0 
                        WHERE IsFromAdmin IS NULL",
                    connection);
                    migrateCmd.ExecuteNonQuery();
                    AddColumnIfNotExists(connection, "Messages", "IsFromAdmin", "BOOLEAN DEFAULT 0");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Ошибка инициализации базы данных: "+ex.Message);

            }
        }

        private void AddColumnIfNotExists(SQLiteConnection connection, string table, string column, string type)
        {
            try
            {
                var checkCmd = new SQLiteCommand(
                    $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='{column}'",
                    connection);
                var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

                if (!exists)
                {
                    var alterCmd = new SQLiteCommand(
                        $"ALTER TABLE {table} ADD COLUMN {column} {type}",
                        connection);
                    alterCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления столбца: {ex.Message}");
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
                        catch (Exception ex)
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
            public bool IsFromAdmin { get; set; } // Добавляем недостающее свойство
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

        public List<MessageData> GetMessagesByUsername(string username)
        {
            var messages = new List<MessageData>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    @"SELECT 
                MessageText, 
                Timestamp, 
                IsFromAdmin,
                Username
              FROM Messages 
              WHERE Username = @username",
                    connection);

                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        messages.Add(new MessageData
                        {
                            Username = reader["Username"].ToString(),
                            Text = reader["MessageText"].ToString(),
                            Timestamp = DateTime.Parse(reader["Timestamp"].ToString()),
                            IsFromAdmin = Convert.ToBoolean(reader["IsFromAdmin"])
                        });
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
                            Description = reader["Description"] != DBNull.Value
                                ? reader["Description"].ToString()
                                : null, // Безопасное чтение
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
                                CabinetId = cabinet.Id,
                                ResponsibleEmployeeId = reader["ResponsibleEmployeeId"] != DBNull.Value
                                    ? Convert.ToInt32(reader["ResponsibleEmployeeId"])
                                    : (int?)null
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
                messageCommand.Parameters.AddWithValue("@status", isTask ? "Не завершено" : "None");

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
            public int? ResponsibleEmployeeId { get; set; }
        }

        public class Employee
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Position { get; set; }
            public int CabinetId { get; set; }
            public string Username { get; set; }

            // Добавьте это свойство
            public string FullName => $"{LastName} {FirstName}";
            public override string ToString() => FullName;
        }

        public int AddEquipment(Equipment equipment)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
            INSERT INTO Equipment 
                (Type, Model, OS, CabinetId, ResponsibleEmployeeId) 
            VALUES 
                (@type, @model, @os, @cabinetId, @responsibleId);
            SELECT last_insert_rowid();", conn);

                // Параметры с явной обработкой NULL
                cmd.Parameters.AddWithValue("@type", equipment.Type);
                cmd.Parameters.AddWithValue("@model", equipment.Model);
                cmd.Parameters.AddWithValue("@os", equipment.OS ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@cabinetId", equipment.CabinetId);
                cmd.Parameters.AddWithValue("@responsibleId",
                    equipment.ResponsibleEmployeeId.HasValue && equipment.ResponsibleEmployeeId > 0
                        ? equipment.ResponsibleEmployeeId.Value
                        : (object)DBNull.Value);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void UpdateEquipment(Equipment equipment)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
            UPDATE Equipment 
            SET 
                Type = @type,
                Model = @model,
                OS = @os,
                ResponsibleEmployeeId = @responsibleId
            WHERE Id = @id", conn);

                // Параметры
                cmd.Parameters.AddWithValue("@type", equipment.Type);
                cmd.Parameters.AddWithValue("@model", equipment.Model);
                cmd.Parameters.AddWithValue("@os", equipment.OS ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@responsibleId",
                    equipment.ResponsibleEmployeeId.HasValue && equipment.ResponsibleEmployeeId > 0
                        ? equipment.ResponsibleEmployeeId.Value
                        : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id", equipment.Id);

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
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Обнулить ответственного в оборудовании
                        var updateCmd = new SQLiteCommand(
                            "UPDATE Equipment SET ResponsibleEmployeeId = NULL WHERE ResponsibleEmployeeId = @id",
                            connection);
                        updateCmd.Parameters.AddWithValue("@id", employeeId);
                        updateCmd.ExecuteNonQuery();

                        // Удалить сотрудника
                        var deleteCmd = new SQLiteCommand(
                            "DELETE FROM Employees WHERE Id = @id",
                            connection);
                        deleteCmd.Parameters.AddWithValue("@id", employeeId);
                        deleteCmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void DeleteCabinet(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Удаляем оборудование кабинета
                        var cmdEquipment = new SQLiteCommand(
                            "DELETE FROM Equipment WHERE CabinetId = @id",
                            connection);
                        cmdEquipment.Parameters.AddWithValue("@id", id);
                        cmdEquipment.ExecuteNonQuery();

                        // Удаляем сотрудников кабинета
                        var cmdEmployees = new SQLiteCommand(
                            "DELETE FROM Employees WHERE CabinetId = @id",
                            connection);
                        cmdEmployees.Parameters.AddWithValue("@id", id);
                        cmdEmployees.ExecuteNonQuery();

                        // Удаляем сам кабинет
                        var cmdCabinet = new SQLiteCommand(
                            "DELETE FROM Cabinets WHERE Id = @id",
                            connection);
                        cmdCabinet.Parameters.AddWithValue("@id", id);
                        cmdCabinet.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
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
        // В классе Database
        public List<string> GetEquipmentTypes()
        {
            var types = new List<string>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand(
                    "SELECT DISTINCT Type FROM Equipment",
                    connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        types.Add(reader["Type"].ToString());
                    }
                }
            }
            return types;
        }

        public Equipment GetEquipmentById(int id)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand(@"
            SELECT 
                Id, Type, Model, OS, CabinetId, 
                ResponsibleEmployeeId
            FROM Equipment 
            WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Equipment
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Type = reader["Type"].ToString(),
                            Model = reader["Model"].ToString(),
                            OS = reader["OS"] is DBNull ? null : reader["OS"].ToString(),
                            CabinetId = Convert.ToInt32(reader["CabinetId"]),
                            ResponsibleEmployeeId = reader["ResponsibleEmployeeId"] is DBNull
                                ? (int?)null
                                : Convert.ToInt32(reader["ResponsibleEmployeeId"]) // Чтение NULL
                        };
                    }
                    return null;
                }
            }
        }

        public List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT * FROM Employees", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Position = reader["Position"].ToString(),
                            Username = reader["Username"].ToString(),
                            CabinetId = Convert.ToInt32(reader["CabinetId"])
                        });
                    }
                }
            }
            return employees;
        }

        public Employee GetEmployeeById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT * FROM Employees WHERE Id = @id",
                    connection);
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Employee
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Position = reader["Position"].ToString(),
                            Username = reader["Username"].ToString(),
                            CabinetId = Convert.ToInt32(reader["CabinetId"])
                        };
                    }
                }
            }
            return null;
        }
        public string GetConnectionString()
        {
            return _connectionString;
        }

        // Коды для диаграмм или отчётов
        public Dictionary<string, int> GetTaskStatusStatistics(string statusFilter = null)
        {
            var stats = new Dictionary<string, int>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            SELECT Status, COUNT(*) 
            FROM Messages 
            WHERE IsTask = 1 
                AND (@statusFilter IS NULL 
                     OR @statusFilter = 'Все' 
                     OR Status = @statusFilter)
            GROUP BY Status";

                var cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@statusFilter",
                    string.IsNullOrEmpty(statusFilter) || statusFilter == "Все"
                        ? DBNull.Value
                        : (object)statusFilter);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stats.Add(reader.GetString(0), reader.GetInt32(1));
                    }
                }
            }
            return stats;
        }

        public Dictionary<string, int> GetTasksPerCabinet(string statusFilter = null)
        {
            var stats = new Dictionary<string, int>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            SELECT COALESCE(CabinetNumber, 'Не указан'), COUNT(*)
            FROM Messages 
            WHERE IsTask = 1 
                AND (@statusFilter IS NULL 
                     OR @statusFilter = 'Все' 
                     OR Status = @statusFilter)
            GROUP BY CabinetNumber";

                var cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@statusFilter",
                    string.IsNullOrEmpty(statusFilter) || statusFilter == "Все"
                        ? DBNull.Value
                        : (object)statusFilter);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cabinet = reader.GetString(0);
                        stats.Add(cabinet, reader.GetInt32(1));
                    }
                }
            }
            return stats;
        }

        public Dictionary<string, int> GetProblemCabinets()
        {
            var stats = new Dictionary<string, int>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SQLiteCommand(
                    @"SELECT CabinetNumber, COUNT(*) 
            FROM Messages 
            WHERE IsTask = 1 AND Status = 'Не завершено' 
            GROUP BY CabinetNumber 
            ORDER BY COUNT(*) DESC 
            LIMIT 5", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cabinet = reader.IsDBNull(0) ? "Не указан" : reader.GetString(0);
                        stats.Add(cabinet, reader.GetInt32(1));
                    }
                }
            }
            return stats;
        }

        public long GetChatIdByUsername(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT ChatId FROM Messages WHERE Username = @username ORDER BY Timestamp DESC LIMIT 1",
                    connection);
                command.Parameters.AddWithValue("@username", username);
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt64(result) : 0;
            }
        }

        public async Task AddOutgoingMessageAsync(string username, long chatId, string messageText)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(
                    @"INSERT INTO Messages 
                (Username, ChatId, MessageText, IsFromAdmin) 
              VALUES 
                (@username, @chatId, @messageText, 1)", connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@chatId", chatId);
                command.Parameters.AddWithValue("@messageText", messageText);

                await command.ExecuteNonQueryAsync();
            }
            MessageAdded?.Invoke();
        }
        public Dictionary<string, double> GetTasksPercentagePerCabinet(string statusFilter = null, string period = "month")
        {
            var stats = new Dictionary<string, double>();
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                var query = @"
            WITH TotalTasks AS (
                SELECT COUNT(*) as Total 
                FROM Messages 
                WHERE IsTask = 1 
                    AND (@statusFilter IS NULL 
                         OR @statusFilter = 'Все' 
                         OR Status = @statusFilter)
                    AND Timestamp >= CASE 
                        WHEN @period = 'month' THEN date('now', 'start of month')
                        WHEN @period = 'week' THEN date('now', '-7 days')
                        ELSE '0000-00-00'
                    END
            )
            SELECT 
                COALESCE(CabinetNumber, 'Не указан'), 
                CASE WHEN (SELECT Total FROM TotalTasks) > 0 
                     THEN (COUNT(*) * 100.0 / (SELECT Total FROM TotalTasks)) 
                     ELSE 0 
                END
            FROM Messages 
            WHERE IsTask = 1 
                AND (@statusFilter IS NULL 
                     OR @statusFilter = 'Все' 
                     OR Status = @statusFilter)
                AND Timestamp >= CASE 
                    WHEN @period = 'month' THEN date('now', 'start of month')
                    WHEN @period = 'week' THEN date('now', '-7 days')
                    ELSE '0000-00-00'
                END
            GROUP BY CabinetNumber";

                var cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@statusFilter",
                    string.IsNullOrEmpty(statusFilter) || statusFilter == "Все"
                        ? DBNull.Value
                        : (object)statusFilter);
                cmd.Parameters.AddWithValue("@period", period);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string cabinet = reader.GetString(0);
                        double percentage = Math.Round(reader.GetDouble(1), 1);
                        stats.Add(cabinet, percentage);
                    }
                }
            }
            return stats;
        }
    }
}