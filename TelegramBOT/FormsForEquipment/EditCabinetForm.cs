using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TelegramBOT.Database;

namespace TelegramBOT
{
    public partial class EditCabinetForm : Form
    {
        private readonly Database _database;
        public Cabinet Cabinet { get; private set; }

        public EditCabinetForm(Cabinet cabinet, Database database)
        {
            InitializeComponent();
            _database = database;
            Cabinet = cabinet;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Size = new Size(400, 300);
            this.Text = "Редактирование кабинета";

            // Номер кабинета
            var lblNumber = new Label
            {
                Text = "Номер кабинета:",
                Location = new Point(20, 20),
                Width = 120
            };
            var txtNumber = new TextBox
            {
                Text = Cabinet.Number,
                Location = new Point(150, 20),
                Width = 200
            };

            // Описание
            var lblDesc = new Label
            {
                Text = "Описание:",
                Location = new Point(20, 60),
                Width = 120
            };
            var txtDesc = new TextBox
            {
                Text = Cabinet.Description,
                Location = new Point(150, 60),
                Width = 200,
                Height = 100
            };

            // Кнопки
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Location = new Point(100, 180)
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(200, 180)
            };

            // Валидация
            btnSave.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNumber.Text))
                {
                    MessageBox.Show("Укажите номер кабинета!");
                    return;
                }

                // Проверка уникальности номера
                if (_database.CheckCabinetExists(txtNumber.Text) &&
                    txtNumber.Text != Cabinet.Number)
                {
                    MessageBox.Show("Кабинет с таким номером уже существует!");
                    return;
                }

                Cabinet.Number = txtNumber.Text.Trim();
                Cabinet.Description = txtDesc.Text.Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.AddRange(new Control[]
            {
            lblNumber, txtNumber,
            lblDesc, txtDesc,
            btnSave, btnCancel
            });
        }

        private void EditCabinetForm_Load(object sender, EventArgs e)
        {

        }
    }
}
