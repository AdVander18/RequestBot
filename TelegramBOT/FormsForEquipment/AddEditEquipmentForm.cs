using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static TelegramBOT.Database;

namespace TelegramBOT.FormsForEquipment
{
    public partial class AddEditEquipmentForm : Form
    {
        public int? ResponsibleEmployeeId { get; private set; }

        private readonly Database _database;
        private readonly int _cabinetId;
        private readonly Equipment _existingEquipment;

        public int CabinetId { get; private set; }
        public string Type { get; private set; }
        public string Model { get; private set; }
        public string OS { get; private set; }

        public AddEditEquipmentForm(Database db, int cabinetId, Equipment existingEquipment = null)
        {
            InitializeComponent();
            CabinetId = cabinetId;
            _database = db;
            _cabinetId = cabinetId;
            _existingEquipment = existingEquipment;

            // Настройка интерфейса
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            cmbType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbType.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;

            // Загрузка данных
            LoadEquipmentTypes();
            InitializeResponsibleComboBox(_cabinetId);
            LoadExistingData();
            UpdateOSFieldState();
        }

        private void LoadExistingData()
        {
            if (_existingEquipment != null)
            {
                cmbType.SelectedItem = _existingEquipment.Type;
                txtModel.Text = _existingEquipment.Model;
                txtOS.Text = _existingEquipment.OS;

                // Устанавливаем ответственного сотрудника
                if (_existingEquipment.ResponsibleEmployeeId.HasValue)
                {
                    cmbMRP.SelectedValue = _existingEquipment.ResponsibleEmployeeId.Value;
                }
            }
        }

        private void InitializeResponsibleComboBox(int cabinetId)
        {
            try
            {
                var employeesInCabinet = _database.GetAllEmployees()
                    .Where(e => e.CabinetId == cabinetId)
                    .ToList();

                // Добавляем элемент "Не назначено" с Id = -1
                var dummy = new Employee
                {
                    Id = -1,
                    FirstName = "Не назначено",
                    LastName = ""
                };

                var comboSource = new List<Employee> { dummy };
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
            types.UnionWith(new List<string> { "Компьютер", "Принтер", "Монитор" });
            cmbType.Items.Clear();
            cmbType.Items.AddRange(types.ToArray());
        }

        // В классе AddEditEquipmentForm
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                var equipment = new Equipment
                {
                    Id = _existingEquipment?.Id ?? 0,
                    Type = cmbType.SelectedItem.ToString(),
                    Model = txtModel.Text,
                    OS = txtOS.Text,
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

            // Обработка МОЛ
            try
            {
                if (cmbMRP.SelectedValue == null)
                {
                    ResponsibleEmployeeId = null;
                }
                else
                {
                    int selectedId = Convert.ToInt32(cmbMRP.SelectedValue);
                    if (selectedId == -1)
                    {
                        ResponsibleEmployeeId = null;
                    }
                    else
                    {
                        var employee = _database.GetEmployeeById(selectedId);
                        if (employee == null)
                        {
                            MessageBox.Show("Ошибка: выбранный сотрудник не найден!");
                            return false;
                        }
                        ResponsibleEmployeeId = selectedId;
                    }
                }
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

        //Material Responsible Person
        private void lbMRP_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(lbMRP, "Материально ответственное лицо");
        }
    }
}