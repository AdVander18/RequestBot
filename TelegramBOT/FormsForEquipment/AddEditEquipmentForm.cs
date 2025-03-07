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
        private readonly int _cabinetId;
        private readonly Equipment _existingEquipment;

        // Свойства для хранения данных
        public int CabinetId { get; private set; }
        public string Type { get; private set; }
        public string Model { get; private set; }
        public string OS { get; private set; }
        public int? ResponsibleEmployeeId { get; private set; }

        public AddEditEquipmentForm(Database db, int cabinetId, Equipment existingEquipment = null)
        {
            InitializeComponent();
            CabinetId = cabinetId;
            _database = db;
            _cabinetId = cabinetId;
            _existingEquipment = existingEquipment;

            // Настройка интерфейса
            ConfigureForm();
            LoadEquipmentTypes();
            InitializeResponsibleComboBox();
            LoadExistingData();
            UpdateOSFieldState();
        }

        private void ConfigureForm()
        {
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            cmbType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbType.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
        }

        private void LoadExistingData()
        {
            if (_existingEquipment != null)
            {
                cmbType.SelectedItem = _existingEquipment.Type;
                txtModel.Text = _existingEquipment.Model;
                txtOS.Text = _existingEquipment.OS;
                ResponsibleEmployeeId = _existingEquipment.ResponsibleEmployeeId;
            }
        }

        private void InitializeResponsibleComboBox()
        {
            try
            {
                var employeesInCabinet = _database.GetAllEmployees()
                    .Where(e => e.CabinetId == _cabinetId)
                    .ToList();

                // Добавляем элемент "Не назначено"
                var comboSource = new List<Employee> { new Employee { Id = -1, LastName = "Не назначено" } };
                comboSource.AddRange(employeesInCabinet);

                cmbMRP.DataSource = comboSource;
                cmbMRP.DisplayMember = "FullName";
                cmbMRP.ValueMember = "Id";

                // Установка выбранного значения
                cmbMRP.SelectedValue = _existingEquipment?.ResponsibleEmployeeId ?? -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateOSFieldState();
        }

        private void UpdateOSFieldState()
        {
            bool isComputer = cmbType.SelectedItem?.ToString() == "Компьютер";
            txtOS.Enabled = isComputer;
            if (!isComputer) txtOS.Text = string.Empty;
        }

        private void LoadEquipmentTypes()
        {
            var types = new HashSet<string>(_database.GetEquipmentTypes());
            types.UnionWith(new[] { "Компьютер", "Принтер", "Монитор" });
            cmbType.Items.Clear();
            cmbType.Items.AddRange(types.OrderBy(t => t).ToArray());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                var equipment = new Equipment
                {
                    Id = _existingEquipment?.Id ?? 0,
                    Type = Type,
                    Model = Model,
                    OS = OS,
                    CabinetId = _cabinetId,
                    ResponsibleEmployeeId = ResponsibleEmployeeId
                };

                if (_existingEquipment == null)
                {
                    _database.AddEquipment(equipment);
                }
                else
                {
                    _database.UpdateEquipment(equipment);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool ValidateInput()
        {
            // Проверка обязательных полей
            if (cmbType.SelectedItem == null || string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Заполните обязательные поля (Тип и Модель)!");
                return false;
            }

            // Обработка ответственного лица
            try
            {
                var selectedId = (int)cmbMRP.SelectedValue;
                ResponsibleEmployeeId = selectedId == -1 ? null : (int?)selectedId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выбора ответственного: {ex.Message}");
                return false;
            }

            // Сохранение остальных данных
            Type = cmbType.SelectedItem.ToString();
            Model = txtModel.Text.Trim();
            OS = txtOS.Text.Trim();

            return true;
        }

        private void lbMRP_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(lbMRP, "Материально ответственное лицо");
        }
    }
}