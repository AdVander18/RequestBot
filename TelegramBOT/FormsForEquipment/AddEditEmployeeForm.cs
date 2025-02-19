using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static TelegramBOT.Database;

namespace TelegramBOT.FormsForEquipment
{
    public partial class AddEditEmployeeForm : Form
    {
        private readonly Database _database;
        
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Position { get; private set; }
        public string Username { get; private set; }

        public AddEditEmployeeForm(Database db, Employee existingEmployee = null)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            _database = db;

            // Настройка автодополнения
            cmbUsername.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbUsername.AutoCompleteSource = AutoCompleteSource.ListItems;

            // Загрузка пользователей
            var usernames = _database.GetAllUsernames();
            cmbUsername.Items.AddRange(usernames.ToArray());

            // Заполнение данных при редактировании
            if (existingEmployee != null)
            {
                txtFirstName.Text = existingEmployee.FirstName;
                txtLastName.Text = existingEmployee.LastName;
                txtPosition.Text = existingEmployee.Position;
                cmbUsername.SelectedItem = existingEmployee.Username;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || 
                string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Заполните имя и фамилию!");
                return;
            }

            FirstName = txtFirstName.Text.Trim();
            LastName = txtLastName.Text.Trim();
            Position = txtPosition.Text.Trim();
            Username = cmbUsername.SelectedItem?.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}