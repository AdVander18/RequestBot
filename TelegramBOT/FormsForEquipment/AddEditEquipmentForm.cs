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
            var employeesInCabinet = _database.GetAllEmployees()
                .Where(e => e.CabinetId == cabinetId)
                .ToList();

            cmbMRP.DataSource = employeesInCabinet;
            cmbMRP.DisplayMember = "FullName";
            cmbMRP.ValueMember = "Id";

            // Для отладки: проверьте, что список сотрудников не пуст
            Console.WriteLine($"Загружено сотрудников: {employeesInCabinet.Count}");
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
            try
            {
                if (ValidateInput())
                {
                    // Явно устанавливаем результат диалога
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }



        private bool ValidateInput()
        {
            if (cmbType.SelectedItem == null || string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Заполните обязательные поля (Тип и Модель)!");
                return false;
            }

            // Проверка существования ответственного сотрудника
            if (cmbMRP.SelectedValue != null)
            {
                if (cmbMRP.SelectedValue is int selectedId)
                {
                    var employee = _database.GetEmployeeById(selectedId);
                    if (employee == null)
                    {
                        MessageBox.Show("Выбранный сотрудник не найден в базе данных!");
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка в выборе сотрудника!");
                    return false;
                }
            }

            Type = cmbType.SelectedItem.ToString();
            Model = txtModel.Text.Trim();
            OS = txtOS.Text.Trim();
            Console.WriteLine($"Selected Responsible ID: {cmbMRP.SelectedValue}");
            return true;
        }

        //Material Responsible Person
        private void lbMRP_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(lbMRP, "Материально ответственное лицо");
        }
    }
}