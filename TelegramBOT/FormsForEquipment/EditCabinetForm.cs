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
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            _database = database;
            Cabinet = cabinet;
        }

        private void EditCabinetForm_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Проверка на пустое поле
            if (string.IsNullOrWhiteSpace(txtNumber.Text))
            {
                MessageBox.Show("Укажите номер кабинета!");
                return;
            }

            // Проверка, что номер — целое число
            if (!int.TryParse(txtNumber.Text, out int number))
            {
                MessageBox.Show("Номер кабинета должен быть числом!");
                return;
            }

            // Проверка уникальности (если номер изменился)
            if (_database.CheckCabinetExists(txtNumber.Text) &&
                txtNumber.Text != Cabinet.Number)
            {
                MessageBox.Show("Кабинет с таким номером уже существует!");
                return;
            }

            // Сохранение данных
            Cabinet.Number = txtNumber.Text.Trim();
            Cabinet.Description = txtDesc.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
