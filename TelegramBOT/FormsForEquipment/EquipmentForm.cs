using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TelegramBOT.FormsForEquipment;
using static TelegramBOT.Database;

namespace TelegramBOT
{
    public partial class EquipmentForm : Form
    {
        private readonly Database _database;
        private Button btnAddEquipment;
        private Button btnEditEquipment;
        private Button btnAddEmployee;
        private Button btnEditEmployee;
        private ContextMenuStrip contextMenu;

        public EquipmentForm(Database db)
        {
            // 1. Сначала инициализируем компоненты дизайнера
            InitializeComponent();

            // 2. Затем работаем с остальными элементами
            _database = db;

            // 3. Настраиваем кнопки
            btnAddEquipment = new Button { Text = "Добавить оборудование", Top = 40 };
            btnAddEquipment.Click += btnAddEquipment_Click;
            Controls.Add(btnAddEquipment);

            btnEditEquipment = new Button { Text = "Изменить оборудование", Top = 70 };
            btnEditEquipment.Click += btnEditEquipment_Click;
            Controls.Add(btnEditEquipment);

            btnAddEmployee = new Button { Text = "Добавить сотрудника", Top = 100 };
            btnAddEmployee.Click += btnAddEmployee_Click;
            Controls.Add(btnAddEmployee);

            btnEditEmployee = new Button { Text = "Изменить сотрудника", Top = 130 };
            btnEditEmployee.Click += btnEditEmployee_Click;
            Controls.Add(btnEditEmployee);

            // 4. Инициализация контекстного меню
            contextMenu = new ContextMenuStrip();
            var deleteToolStripItem = new ToolStripMenuItem("Удалить");
            deleteToolStripItem.Click += DeleteItem_Click;
            contextMenu.Items.Add(deleteToolStripItem);

            // 5. Теперь treeView уже инициализирован
            treeView.ContextMenuStrip = contextMenu;

            // 6. Загрузка данных
            LoadData();
        }

        private void LoadData()
        {
            treeView.Nodes.Clear();
            var cabinets = _database.GetAllCabinets();

            foreach (var cabinet in cabinets)
            {
                var node = new TreeNode($"Кабинет {cabinet.Number} ({cabinet.Description})")
                {
                    Tag = cabinet,
                    ImageKey = "cabinet",
                    SelectedImageKey = "cabinet"
                };

                var eqNode = new TreeNode("Оборудование");
                foreach (var eq in cabinet.Equipment)
                {
                    eqNode.Nodes.Add(new TreeNode($"{eq.Type} {eq.Model}" +
                        (eq.OS != null ? $" ({eq.OS})" : ""))
                    {
                        Tag = eq,
                        ImageKey = "equipment"
                    });
                }

                var empNode = new TreeNode("Сотрудники");
                foreach (var emp in cabinet.Employees)
                {
                    empNode.Nodes.Add(new TreeNode(
                        $"{emp.LastName} {emp.FirstName} - {emp.Position}")
                    {
                        Tag = emp,
                        ImageKey = "user"
                    });
                }

                node.Nodes.Add(eqNode);
                node.Nodes.Add(empNode);
                treeView.Nodes.Add(node);
            }
            treeView.ExpandAll();
        }

        private void DeleteItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Equipment equipment)
            {
                _database.DeleteEquipment(equipment.Id);
                LoadData();
            }
            else if (treeView.SelectedNode?.Tag is Employee employee)
            {
                _database.DeleteEmployee(employee.Id);
                LoadData();
            }
        }



        private void AddNewCabinet()
        {
            var addCabinetForm = new AddCabinetForm();

            if (addCabinetForm.ShowDialog() == DialogResult.OK)
            {   
                try
                {
                    if (string.IsNullOrWhiteSpace(addCabinetForm.Cabinet.Number))
                    {
                        throw new ArgumentException("Номер кабинета обязателен для заполнения!");
                    }

                    _database.AddCabinet(addCabinetForm.Cabinet);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка добавления",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAddEquipment_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Cabinet cabinet)
            {
                try
                {
                    var form = new AddEditEquipmentForm();
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        if (string.IsNullOrWhiteSpace(form.Type) || string.IsNullOrWhiteSpace(form.Model))
                        {
                            throw new ArgumentException("Поля 'Тип' и 'Модель' обязательны для заполнения!");
                        }

                        var equipment = new Equipment
                        {
                            Type = form.Type,
                            Model = form.Model,
                            OS = form.OS,
                            CabinetId = cabinet.Id
                        };
                        _database.AddEquipment(equipment);
                        LoadData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnEditEquipment_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Equipment equipment)
            {
                var form = new AddEditEquipmentForm(equipment); // Используем конструктор с параметром
                if (form.ShowDialog() == DialogResult.OK)
                {
                    equipment.Type = form.Type;
                    equipment.Model = form.Model;
                    equipment.OS = form.OS;
                    _database.UpdateEquipment(equipment);
                    LoadData();
                }
            }
        }

        private void btnAddEmployee_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Cabinet cabinet)
            {
                var form = new AddEditEmployeeForm(_database.GetAllUsernames());
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var employee = new Employee
                    {
                        FirstName = form.FirstName,
                        LastName = form.LastName,
                        Position = form.Position,
                        Username = form.Username,
                        CabinetId = cabinet.Id
                    };
                    _database.AddEmployee(employee);
                    LoadData();
                }
            }
        }

        private void btnEditEmployee_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Employee employee)
            {
                var form = new AddEditEmployeeForm(employee, _database.GetAllUsernames());
                if (form.ShowDialog() == DialogResult.OK)
                {
                    employee.FirstName = form.FirstName;
                    employee.LastName = form.LastName;
                    employee.Position = form.Position;
                    employee.Username = form.Username;
                    _database.UpdateEmployee(employee);
                    LoadData();
                }
            }
        }

        private void EditSelected()
        {
            if (treeView.SelectedNode?.Tag is Cabinet cabinet)
            {
                using (var form = new EditCabinetForm(cabinet, _database))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _database.UpdateCabinet(form.Cabinet); // Убедитесь, что form.Cabinet возвращает объект типа Cabinet
                        LoadData();
                        MessageBox.Show("Изменения сохранены!");
                    }
                }
            }
        }

        private void EquipmentForm_Load(object sender, EventArgs e)
        {

        }

        private void btnAddCabinet_Click(object sender, EventArgs e)
        {
            AddNewCabinet();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool isEquipment = e.Node?.Tag is Equipment;
            bool isEmployee = e.Node?.Tag is Employee;

            btnEditEquipment.Enabled = isEquipment;
            btnEditEmployee.Enabled = isEmployee;
        }
    }
}
