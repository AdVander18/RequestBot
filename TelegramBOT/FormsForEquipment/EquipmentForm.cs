using System;
using System.Collections.Generic;
using System.Linq;
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
        private DataGridView dataGridViewDetails; // Добавлен DataGridView

        public EquipmentForm(Database db)
        {
            InitializeComponent();

            _database = db;

            // Инициализация DataGridView
            dataGridViewDetails = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                ScrollBars = ScrollBars.Vertical,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Controls.Add(dataGridViewDetails);

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
            // Сохраняем текущее состояние
            TreeNode selectedNode = treeView.SelectedNode;
            // ОШИБКА ПРОИСХОДИТ ПОСЛЕ РАСКРЫТИЯ "ОБОРУДОВАНИЯ" И ОБНОВЛЕНИЯ ДАННЫХ ПО КНОПКУ ИСПРАВИТЬ int[] selectedPath = selectedNode?.FullPath.Split('\\').Select(x => int.Parse(x)).ToArray();
            List<string> expandedPaths = GetAllExpandedNodePaths();

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var cabinets = _database.GetAllCabinets();
            foreach (var cabinet in cabinets)
            {
                var node = new TreeNode($"Кабинет {cabinet.Number}")
                {
                    Tag = cabinet,
                    ImageKey = "cabinet"
                };

                var equipmentNode = new TreeNode("Оборудование");
                foreach (var eq in cabinet.Equipment)
                {
                    equipmentNode.Nodes.Add(new TreeNode($"{eq.Type} {eq.Model}") { Tag = eq });
                }

                var employeesNode = new TreeNode("Сотрудники");
                foreach (var emp in cabinet.Employees)
                {
                    employeesNode.Nodes.Add(new TreeNode($"{emp.LastName} {emp.FirstName}") { Tag = emp });
                }

                node.Nodes.Add(equipmentNode);
                node.Nodes.Add(employeesNode);
                treeView.Nodes.Add(node);
            }

            treeView.EndUpdate();

            // Восстанавливаем состояние
            RestoreExpandedNodes(expandedPaths);
            RestoreSelectedNode(selectedPath);
        }
        private List<string> GetAllExpandedNodePaths()
        {
            var paths = new List<string>();
            foreach (TreeNode node in treeView.Nodes)
            {
                GetExpandedPaths(node, paths);
            }
            return paths;
        }

        private void GetExpandedPaths(TreeNode node, List<string> paths)
        {
            if (node.IsExpanded)
            {
                paths.Add(node.FullPath);
            }
            foreach (TreeNode child in node.Nodes)
            {
                GetExpandedPaths(child, paths);
            }
        }

        private void RestoreExpandedNodes(List<string> paths)
        {
            foreach (string path in paths)
            {
                TreeNode node = FindNodeByPath(path);
                if (node != null)
                {
                    node.Expand();
                }
            }
        }

        private void RestoreSelectedNode(int[] path)
        {
            if (path == null || path.Length == 0) return;

            TreeNode currentNode = treeView.Nodes[path[0] - 1];
            for (int i = 1; i < path.Length; i++)
            {
                currentNode = currentNode.Nodes[path[i] - 1];
            }
            treeView.SelectedNode = currentNode;
            currentNode.EnsureVisible();
        }

        private TreeNode FindNodeByPath(string fullPath)
        {
            string[] parts = fullPath.Split('\\');
            TreeNode node = null;
            foreach (string part in parts)
            {
                node = node == null
                    ? treeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == part)
                    : node.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == part);

                if (node == null) break;
            }
            return node;
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
                    // Исправлено: передаем _database первым параметром
                    var form = new AddEditEquipmentForm(_database);
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
                // Исправлено: передаем _database и equipment
                var form = new AddEditEquipmentForm(_database, equipment);
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
                // Исправлено: передаем только _database
                var form = new AddEditEmployeeForm(_database);
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
                // Исправлено: передаем _database и employee
                var form = new AddEditEmployeeForm(_database, employee);
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
            //treeView.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
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
            bool isCabinet = e.Node?.Tag is Cabinet;
            bool isEquipment = e.Node?.Tag is Equipment;
            bool isEmployee = e.Node?.Tag is Employee;

            btnEdit.Enabled = isCabinet;
            btnEditEquipment.Enabled = isEquipment;
            btnEditEmployee.Enabled = isEmployee;

            // Очищаем DataGridView
            dataGridViewDetails.Rows.Clear();
            dataGridViewDetails.Columns.Clear();

            // Обработка отображения подробностей
            if (e.Node != null && e.Node.Level == 1) // Уровень 1: "Оборудование" или "Сотрудники"
            {
                var parentNode = e.Node.Parent;
                if (parentNode?.Tag is Cabinet cabinet)
                {
                    if (e.Node.Text == "Оборудование")
                    {
                        // Настройка колонок для оборудования
                        dataGridViewDetails.Columns.Add("Type", "Тип");
                        dataGridViewDetails.Columns.Add("Model", "Модель");
                        dataGridViewDetails.Columns.Add("OS", "Операционная система");

                        foreach (var eq in cabinet.Equipment)
                        {
                            dataGridViewDetails.Rows.Add(eq.Type, eq.Model, eq.OS);
                        }
                    }
                    else if (e.Node.Text == "Сотрудники")
                    {
                        // Настройка колонок для сотрудников
                        dataGridViewDetails.Columns.Add("LastName", "Фамилия");
                        dataGridViewDetails.Columns.Add("FirstName", "Имя");
                        dataGridViewDetails.Columns.Add("Position", "Должность");

                        foreach (var emp in cabinet.Employees)
                        {
                            dataGridViewDetails.Rows.Add(emp.LastName, emp.FirstName, emp.Position);
                        }
                    }
                }
            }
        }

        private void treeView_DoubleClick(object sender, EventArgs e)
        {
            var selectedNode = treeView.SelectedNode;
            if (selectedNode == null) return;

            // Обработка для конкретного оборудования
            if (selectedNode.Tag is Equipment equipment)
            {
                var form = new AddEditEquipmentForm(_database, equipment);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    equipment.Type = form.Type;
                    equipment.Model = form.Model;
                    equipment.OS = form.OS;
                    _database.UpdateEquipment(equipment);
                    LoadData();
                }
                return;
            }

            // Обработка для конкретного сотрудника
            if (selectedNode.Tag is Employee employee)
            {
                var form = new AddEditEmployeeForm(_database, employee);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    employee.FirstName = form.FirstName;
                    employee.LastName = form.LastName;
                    employee.Position = form.Position;
                    employee.Username = form.Username;
                    _database.UpdateEmployee(employee);
                    LoadData();
                }
                return;
            }

            // Обработка для родительских узлов "Оборудование"/"Сотрудники"
            if (selectedNode.Level == 1 && selectedNode.Parent?.Tag is Cabinet cabinet)
            {
                if (selectedNode.Text == "Оборудование")
                {
                    var form = new AddEditEquipmentForm(_database);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        var newEquipment = new Equipment
                        {
                            Type = form.Type,
                            Model = form.Model,
                            OS = form.OS,
                            CabinetId = cabinet.Id
                        };
                        _database.AddEquipment(newEquipment);
                        LoadData();
                    }
                }
                else if (selectedNode.Text == "Сотрудники")
                {
                    var form = new AddEditEmployeeForm(_database);
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        var newEmployee = new Employee
                        {
                            FirstName = form.FirstName,
                            LastName = form.LastName,
                            Position = form.Position,
                            Username = form.Username,
                            CabinetId = cabinet.Id
                        };
                        _database.AddEmployee(newEmployee);
                        LoadData();
                    }
                }
            }
        }
    }
}
