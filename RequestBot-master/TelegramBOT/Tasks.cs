using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TelegramBOT
{
    public partial class Tasks : Form
    {
        private readonly Database _database;
        public Tasks(Database database)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            _database = database;

            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += ListBox1_DrawItem;
            listBox1.MouseDoubleClick += ListBox1_MouseDoubleClick;
            LoadTasks();
            ShowOldTasksWarning();
        }
        private void LoadTasks()
        {
            listBox1.Items.Clear();
            var tasks = _database.GetAllTasks();
            foreach (var task in tasks)
            {
                listBox1.Items.Add(task);
            }
        }

        private void ShowOldTasksWarning()
        {
            var oldTasks = _database.GetAllTasks()
                .Where(t => (DateTime.Now - t.Timestamp).TotalDays > 7)
                .ToList();

            if (oldTasks.Any())
            {
                MessageBox.Show(
                    $"Найдено {oldTasks.Count} задач старше 7 дней!",
                    "Внимание",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }



        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            var task = (Database.TaskData)listBox1.Items[e.Index];
            var daysOld = (DateTime.Now - task.Timestamp).TotalDays;

            // Цвет текста в зависимости от возраста
            Brush textBrush = daysOld > 7 ? Brushes.Red : Brushes.Black;

            // Рисуем кружок статуса
            var statusColor = task.Status == "Завершено" ? Color.Green : Color.Red;
            using (var brush = new SolidBrush(statusColor))
            {
                e.Graphics.FillEllipse(brush, e.Bounds.Left + 2, e.Bounds.Top + 2, 12, 12);
            }

            // Добавляем восклицательный знак для старых задач
            if (daysOld > 7)
            {
                e.Graphics.DrawString("!", e.Font, Brushes.Red, e.Bounds.Left + 16, e.Bounds.Top);
            }

            // Рисуем текст задания
            e.Graphics.DrawString(task.MessageText, e.Font, textBrush,
                new RectangleF(e.Bounds.Left + 30, e.Bounds.Top, e.Bounds.Width - 30, e.Bounds.Height));

            e.DrawFocusRectangle();
        }
        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                var task = (Database.TaskData)listBox1.Items[index];
                task.Status = task.Status == "Завершено" ? "Не завершено" : "Завершено";
                _database.UpdateTaskStatus(task.Id, task.Status);
                listBox1.Refresh();
                UpdateSelectedTaskInfo();
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedTaskInfo();
        }
        private void UpdateSelectedTaskInfo()
        {
            if (listBox1.SelectedItem != null)
            {
                var task = (Database.TaskData)listBox1.SelectedItem;
                var daysOld = (DateTime.Now - task.Timestamp).TotalDays;

                textBox1.Text = $"ID: {task.Id}" + Environment.NewLine +
                                $"Дата создания: {task.Timestamp:dd.MM.yyyy HH:mm}" + Environment.NewLine +
                                $"Возраст задачи: {Math.Floor(daysOld)} дней" + Environment.NewLine +
                                $"Фамилия: {task.LastName ?? "Не указано"}" + Environment.NewLine +
                                $"Имя: {task.FirstName ?? "Не указано"}" + Environment.NewLine +
                                $"Номер кабинета: {task.CabinetNumber ?? "Не указано"}" + Environment.NewLine +
                                $"Описание: {task.MessageText}" + Environment.NewLine +
                                $"Статус: {task.Status}" + Environment.NewLine +
                                (daysOld > 7 ? "⚠ ЗАДАЧА СТАРШЕ 7 ДНЕЙ ⚠" + Environment.NewLine : "");
            }
            else
            {
                textBox1.Clear();
            }
        }
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                // Спрашиваем пользователя, уверен ли он в удалении
                DialogResult result = MessageBox.Show("Вы точно хотите удалить эту запись?", "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // Если пользователь нажал "Yes", то удаляем запись
                if (result == DialogResult.Yes)
                {
                    var task = (Database.TaskData)listBox1.SelectedItem;
                    _database.DeleteTask(task.Id);
                    LoadTasks();
                    textBox1.Clear();
                }
            }
            else
            {
                // Если ничего не выбрано, показываем сообщение об ошибке
                MessageBox.Show("Выберите задачу для удаления!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTasks();
        }
        
        private void Tasks_Load(object sender, EventArgs e)
        {

        }

        private void btnRefresh_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(btnRefresh, "Обновить данные о задачах");
        }
    }
}
