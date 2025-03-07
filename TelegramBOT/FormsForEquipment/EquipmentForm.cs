using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TelegramBOT.FormsForEquipment;
using OfficeOpenXml;
using System.IO;
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
        private readonly string _connectionString;

        public EquipmentForm(Database db, string connectionString)
        {
            InitializeComponent();

            _database = db;
            _connectionString = connectionString;

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
            treeView.ContextMenuStrip = contextMenu; // Привязка меню к TreeView

            // 5. Настраиваем TreeView

            treeView.MouseDown += treeView_MouseDown; // Добавьте эту строку

            // 6. Загрузка данных
            this.FormClosing += EquipmentForm_FormClosing;
            LoadData();
        }

        private void EquipmentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Передаем фокус обратно на MainForm
            if (this.Owner != null)
            {
                this.Owner.Activate();
                this.Owner.Focus();
            }
        }

        private void LoadData()
        {
            TreeNode selectedNode = treeView.SelectedNode;
            int[] selectedPath = null;

            if (selectedNode != null && selectedNode.Parent != null) // Добавлена проверка
            {
                selectedPath = GetNodePath(selectedNode);
            }

            List<string> expandedPaths = GetAllExpandedNodePaths();

            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var cabinets = _database.GetAllCabinets();
            foreach (var cabinet in cabinets)
            {

                // Формируем текст узла с учетом описания
                string cabinetText = string.IsNullOrEmpty(cabinet.Description)
                    ? $"Кабинет {cabinet.Number}"
                    : $"Кабинет {cabinet.Number} ({cabinet.Description})";

                var node = new TreeNode(cabinetText)
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
                var empNode = new TreeNode("Сотрудники");
                foreach (var emp in cabinet.Employees)
                {
                    empNode.Nodes.Add(new TreeNode($"{emp.LastName} {emp.FirstName}")
                    {
                        Tag = emp,
                        ImageKey = "employee"
                    });
                }
                node.Nodes.Add(empNode);

                node.Nodes.Add(equipmentNode);
                node.Nodes.Add(employeesNode);
                treeView.Nodes.Add(node);
            }

            treeView.EndUpdate();

            RestoreExpandedNodes(expandedPaths);

            if (selectedPath != null) // Добавлена проверка
            {
                RestoreSelectedNode(selectedPath);
            }
        }

        private int[] GetNodePath(TreeNode node)
        {
            List<int> path = new List<int>();
            while (node != null && node.Parent != null) // Добавлена проверка на null
            {
                int index = node.Parent.Nodes.IndexOf(node);
                path.Insert(0, index);
                node = node.Parent;
            }
            return path.ToArray();
        }

        // Обновленный метод восстановления узла
        private void RestoreSelectedNode(int[] path)
        {
            if (path == null || path.Length == 0) return;

            try
            {
                TreeNode currentNode = null;
                for (int i = 0; i < path.Length; i++)
                {
                    if (i == 0)
                    {
                        // Проверка индекса для корневых узлов
                        if (path[i] < treeView.Nodes.Count)
                        {
                            currentNode = treeView.Nodes[path[i]];
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        // Проверка индекса для дочерних узлов
                        if (currentNode != null &&
                            path[i] < currentNode.Nodes.Count)
                        {
                            currentNode = currentNode.Nodes[path[i]];
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (currentNode != null)
                {
                    treeView.SelectedNode = currentNode;
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки при необходимости
                Console.WriteLine($"Ошибка восстановления узла: {ex.Message}");
            }
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
            var selectedNode = treeView.SelectedNode;
            if (selectedNode == null) return;

            string message = "";
            string title = "Подтверждение удаления";
            DialogResult result;

            // Звуковое предупреждение (опционально)
            System.Media.SystemSounds.Exclamation.Play();                                                                                                                                                               

            if (selectedNode.Tag is Equipment equipment)
            {
                message = $"Удалить оборудование: {equipment.Type} {equipment.Model}?";
                result = MessageBox.Show(
                    text: message,
                    caption: title,
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Warning,
                    defaultButton: MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    _database.DeleteEquipment(equipment.Id);
                    LoadData();
                }
            }
            else if (selectedNode.Tag is Employee employee)
            {
                message = $"Удалить сотрудника: {employee.LastName} {employee.FirstName}?";
                result = MessageBox.Show(
                    text: message,
                    caption: title,
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _database.DeleteEmployee(employee.Id);
                    LoadData();
                }
            }
            else if (selectedNode.Tag is Cabinet cabinet)
            {
                bool hasData = cabinet.Equipment.Any() || cabinet.Employees.Any();
                message = $"Удалить кабинет {cabinet.Number}?";

                if (hasData)
                {
                    message += "\n\nБудут удалены:";
                    if (cabinet.Equipment.Any()) message += "\n- Все оборудование";
                    if (cabinet.Employees.Any()) message += "\n- Все сотрудники";
                }

                result = MessageBox.Show(
                    text: message,
                    caption: title,
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Warning,
                    defaultButton: MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    _database.DeleteCabinet(cabinet.Id);
                    LoadData();
                }
            }
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode node = treeView.GetNodeAt(e.X, e.Y);
                if (node != null)
                {
                    treeView.SelectedNode = node;
                }
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

        // Для добавления оборудования:
        private void btnAddEquipment_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Cabinet cabinet)
            {
                using (var form = new AddEditEquipmentForm(_database, cabinet.Id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadData(); // Только обновляем интерфейс
                    }
                }
            }
        }

        // Для редактирования оборудования:
        private void btnEditEquipment_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Equipment equipment)
            {
                using (var form = new AddEditEquipmentForm(_database, equipment.CabinetId, equipment))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadData(); // Только обновляем интерфейс
                    }
                }
            }
        }

        private void btnAddEmployee_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Cabinet cabinet)
            {
                var form = new AddEditEmployeeForm(_database)
                {
                    StartPosition = FormStartPosition.CenterParent // Центрирование относительно родителя
                };

                if (form.ShowDialog(this) == DialogResult.OK) // Передаём текущую форму как владельца
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

                    this.Activate(); // Возвращаем фокус после закрытия диалога
                }

            }
        }

        private void btnEditEmployee_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode?.Tag is Employee employee)
            {
                var form = new AddEditEmployeeForm(_database, employee)
                {
                    StartPosition = FormStartPosition.CenterParent // Центрирование относительно родителя
                };

                if (form.ShowDialog(this) == DialogResult.OK) // Передаём текущую форму как владельца
                {
                    employee.FirstName = form.FirstName;
                    employee.LastName = form.LastName;
                    employee.Position = form.Position;
                    employee.Username = form.Username;
                    _database.UpdateEmployee(employee);
                    LoadData();

                    this.Activate(); // Возвращаем фокус после закрытия диалога
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

        public Employee GetEmployeeById(int id)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SQLiteCommand("SELECT * FROM Employees WHERE Id = @id", connection);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.Read() ? new Employee
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        // ... остальные поля
                    } : null;
                }
            }
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
            if (e.Node?.Tag is Cabinet cabinetNode && e.Node.Text == "Оборудование")
            {
                dataGridViewDetails.Columns.Add("Responsible", "Ответственный");
                foreach (var eq in cabinetNode.Equipment)
                {
                    var responsible = eq.ResponsibleEmployeeId.HasValue
                        ? _database.GetEmployeeById(eq.ResponsibleEmployeeId.Value)?.FullName
                        : "Не назначен";
                    dataGridViewDetails.Rows.Add(eq.Type, eq.Model, eq.OS, responsible);
                }
            }

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

            if (selectedNode.Tag is Equipment equipment)
            {
                using (var form = new AddEditEquipmentForm(_database, equipment.CabinetId, equipment))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadData(); // Только обновляем интерфейс, сохранение уже сделано в форме
                    }
                }
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
                // Для нового оборудования:
                if (selectedNode.Text == "Оборудование")
                {
                    using (var form = new AddEditEquipmentForm(_database, cabinet.Id))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadData(); // Только обновляем интерфейс, сохранение уже сделано в форме
                        }
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

        private class EquipmentExport
        {
            public string Type { get; set; }
            public string Model { get; set; }
            public string OS { get; set; }
            public string Cabinet { get; set; }
            public int? ResponsibleEmployeeId { get; set; } // Добавьте это поле
            public string Responsible { get; set; }
        }

        private class EmployeeExport
        {
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string Position { get; set; }
            public string Cabinet { get; set; }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var equipmentList = new List<EquipmentExport>();
                var employeeList = new List<EmployeeExport>();

                // Сбор данных из TreeView
                foreach (TreeNode cabinetNode in treeView.Nodes)
                {
                    if (cabinetNode.Tag is Cabinet cabinet)
                    {
                        string cabinetInfo = string.IsNullOrEmpty(cabinet.Description)
                            ? $"Кабинет {cabinet.Number}"
                            : $"Кабинет {cabinet.Number} ({cabinet.Description})";

                        foreach (TreeNode sectionNode in cabinetNode.Nodes)
                        {
                            if (sectionNode.Text == "Оборудование")
                            {
                                foreach (TreeNode eqNode in sectionNode.Nodes)
                                {
                                    if (eqNode.Tag is Equipment eq)
                                    {
                                        equipmentList.Add(new EquipmentExport
                                        {
                                            Type = eq.Type,
                                            Model = eq.Model,
                                            OS = eq.OS,
                                            Cabinet = cabinetInfo,
                                            ResponsibleEmployeeId = eq.ResponsibleEmployeeId
                                        });
                                    }
                                }
                            }
                            else if (sectionNode.Text == "Сотрудники")
                            {
                                foreach (TreeNode empNode in sectionNode.Nodes)
                                {
                                    if (empNode.Tag is Employee emp)
                                    {
                                        employeeList.Add(new EmployeeExport
                                        {
                                            LastName = emp.LastName,
                                            FirstName = emp.FirstName,
                                            Position = emp.Position,
                                            Cabinet = cabinetInfo
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                // Создание Excel файла
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    // Лист "Оборудование"
                    ExcelWorksheet equipmentSheet = excelPackage.Workbook.Worksheets.Add("Оборудование");
                    equipmentSheet.Cells[1, 1].Value = "Тип";
                    equipmentSheet.Cells[1, 2].Value = "Модель";
                    equipmentSheet.Cells[1, 3].Value = "ОС";
                    equipmentSheet.Cells[1, 4].Value = "Кабинет";
                    equipmentSheet.Cells[1, 5].Value = "Ответственный";

                    int row = 2;
                    foreach (var eq in equipmentList)
                    {
                        var responsible = eq.ResponsibleEmployeeId.HasValue
                            ? _database.GetEmployeeById(eq.ResponsibleEmployeeId.Value)?.FullName
                            : "Не назначен";

                        equipmentSheet.Cells[row, 1].Value = eq.Type;
                        equipmentSheet.Cells[row, 2].Value = eq.Model;
                        equipmentSheet.Cells[row, 3].Value = eq.OS;
                        equipmentSheet.Cells[row, 4].Value = eq.Cabinet;
                        equipmentSheet.Cells[row, 5].Value = responsible;
                        row++;
                    }

                    // Лист "Сотрудники"
                    ExcelWorksheet employeeSheet = excelPackage.Workbook.Worksheets.Add("Сотрудники");
                    employeeSheet.Cells[1, 1].Value = "Фамилия";
                    employeeSheet.Cells[1, 2].Value = "Имя";
                    employeeSheet.Cells[1, 3].Value = "Должность";
                    employeeSheet.Cells[1, 4].Value = "Кабинет";

                    row = 2;
                    foreach (var emp in employeeList)
                    {
                        employeeSheet.Cells[row, 1].Value = emp.LastName;
                        employeeSheet.Cells[row, 2].Value = emp.FirstName;
                        employeeSheet.Cells[row, 3].Value = emp.Position;
                        employeeSheet.Cells[row, 4].Value = emp.Cabinet;
                        row++;
                    }

                    ExcelWorksheet tasksSheet = excelPackage.Workbook.Worksheets.Add("Задачи");
                    tasksSheet.Cells[1, 1].Value = "Кабинет";
                    tasksSheet.Cells[1, 2].Value = "Количество задач";
                    var taskStats = _database.GetTasksPerCabinet();
                    row = 2;
                    foreach (var entry in taskStats)
                    {
                        tasksSheet.Cells[row, 1].Value = entry.Key;
                        tasksSheet.Cells[row, 2].Value = entry.Value;
                        row++;
                    }

                    // Авто-ширина столбцов
                    equipmentSheet.Cells[equipmentSheet.Dimension.Address].AutoFitColumns();
                    employeeSheet.Cells[employeeSheet.Dimension.Address].AutoFitColumns();

                    // Сохранение файла
                    string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Оборудование в кабинетах.xlsx");
                    FileInfo excelFile = new FileInfo(savePath);

                    if (excelFile.Exists)
                    {
                        if (MessageBox.Show("Файл Оборудование в кабинетах.xlsx уже существует. Перезаписать?",
                            "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        {
                            return;
                        }
                        excelFile.Delete(); // Удаляем существующий файл перед сохранением
                    }

                    excelPackage.SaveAs(excelFile);
                    MessageBox.Show($"Данные успешно экспортированы в файл: {savePath}",
                        "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Отчёты или диаграммы

        private DataGridView CreateEquipmentGrid()
        {
            var grid = new DataGridView { Dock = DockStyle.Fill };
            grid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "Тип", DataPropertyName = "Type" },
                new DataGridViewTextBoxColumn { HeaderText = "Модель", DataPropertyName = "Model" },
                new DataGridViewTextBoxColumn { HeaderText = "ОС", DataPropertyName = "OS" },
                new DataGridViewTextBoxColumn { HeaderText = "Кабинет", DataPropertyName = "Cabinet" }
            );

            var cabinets = _database.GetAllCabinets();
            var equipmentList = cabinets.SelectMany(c => c.Equipment).ToList();
            grid.DataSource = equipmentList;

            return grid;
        }
    }
}
