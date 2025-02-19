using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static TelegramBOT.Database;

namespace TelegramBOT.FormsForEquipment
{
    public partial class AddEditEquipmentForm : Form
    {
        private readonly Database _database;

        public string Type { get; private set; }
        public string Model { get; private set; }
        public string OS { get; private set; }

        public AddEditEquipmentForm(Database db, Equipment existingEquipment = null)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            _database = db;

            // Настройка автодополнения
            cmbType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbType.AutoCompleteSource = AutoCompleteSource.ListItems;

            // Подписываемся на событие изменения выбранного типа
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;

            // Загрузка типов оборудования
            LoadEquipmentTypes();

            // Заполнение данных при редактировании
            if (existingEquipment != null)
            {
                cmbType.SelectedItem = existingEquipment.Type;
                txtModel.Text = existingEquipment.Model;
                txtOS.Text = existingEquipment.OS;
            }

            // Обновляем состояние поля OS при старте формы
            UpdateOSFieldState();
        }

        // Обработчик изменения выбранного типа
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateOSFieldState();
        }

        // Метод для обновления состояния поля OS
        private void UpdateOSFieldState()
        {
            bool isComputer = cmbType.SelectedItem?.ToString() == "Компьютер";
            txtOS.Enabled = isComputer;

            // Очищаем поле, если выбран не компьютер
            if (!isComputer)
            {
                txtOS.Text = string.Empty;
            }
        }

        private void LoadEquipmentTypes()
        {
            // Получаем типы из базы данных
            var types = _database.GetEquipmentTypes();

            // Создаем список стандартных типов
            var defaultTypes = new List<string> { "Компьютер", "Принтер", "Монитор" };

            // Объединяем стандартные типы с полученными из базы, избегая дубликатов
            var allTypes = new HashSet<string>(types); // Используем HashSet для удобства проверки наличия
            foreach (var type in defaultTypes)
            {
                allTypes.Add(type); // Добавляем стандартные, если их ещё нет
            }

            // Очищаем ComboBox и заполняем объединенным списком
            cmbType.Items.Clear();
            cmbType.Items.AddRange(allTypes.ToArray());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbType.SelectedItem == null || string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Заполните обязательные поля (Тип и Модель)!");
                return;
            }

            Type = cmbType.SelectedItem.ToString();
            Model = txtModel.Text.Trim();
            OS = txtOS.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}