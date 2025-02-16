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
    public partial class AddCabinetForm : Form
    {
        private ErrorProvider errorProvider; // Добавляем поле
        public Cabinet Cabinet { get; private set; } = new Cabinet();

        public AddCabinetForm()
        {
            InitializeComponent();
            // Инициализация ErrorProvider
            errorProvider = new ErrorProvider
            {
                BlinkStyle = ErrorBlinkStyle.NeverBlink,
                ContainerControl = this
            };

            // Валидация для поля номера кабинета
            txtNumber.Validating += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNumber.Text))
                {
                    errorProvider.SetError(txtNumber, "Введите номер кабинета");
                    e.Cancel = true;
                }
                else
                {
                    errorProvider.SetError(txtNumber, "");
                }
            };
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                Cabinet.Number = txtNumber.Text.Trim();
                Cabinet.Description = txtDescription.Text.Trim();
                DialogResult = DialogResult.OK;
            }
        }
    }
}
