using System;
using System.Collections.Generic;
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
            this.StartPosition = FormStartPosition.CenterScreen;
            _database = db;

            // Настройка автодополнения
            cmbType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbType.AutoCompleteSource = AutoCompleteSource.ListItems;

            // Загрузка типов оборудования
            LoadEquipmentTypes();

            // Заполнение данных при редактировании
            if (existingEquipment != null)
            {
                cmbType.SelectedItem = existingEquipment.Type;
                txtModel.Text = existingEquipment.Model;
                txtOS.Text = existingEquipment.OS;
            }
        }

        private void LoadEquipmentTypes()
        {
            var types = _database.GetEquipmentTypes();

            if (types.Count == 0)
            {
                types.AddRange(new[] { "Компьютер", "Принтер", "Монитор" });
            }

            cmbType.Items.AddRange(types.ToArray());
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