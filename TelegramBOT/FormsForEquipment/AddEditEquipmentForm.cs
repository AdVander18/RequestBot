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

namespace TelegramBOT.FormsForEquipment
{
    public partial class AddEditEquipmentForm : Form
    {
        public string Type { get; private set; }
        public string Model { get; private set; }
        public string OS { get; private set; }
        // Конструктор ДЛЯ ДОБАВЛЕНИЯ нового оборудования
        public AddEditEquipmentForm()
        {
            InitializeComponent();
        }

        // Конструктор ДЛЯ РЕДАКТИРОВАНИЯ существующего оборудования
        public AddEditEquipmentForm(Equipment equipment) : this()
        {
            // Заполняем поля данными из equipment
            txtType.Text = equipment.Type;
            txtModel.Text = equipment.Model;
            txtOS.Text = equipment.OS;
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtType.Text) || string.IsNullOrWhiteSpace(txtModel.Text))
            {
                MessageBox.Show("Поля 'Тип' и 'Модель' обязательны для заполнения!",
                                "Ошибка",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
