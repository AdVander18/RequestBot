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
    public partial class AddEditEmployeeForm : Form
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Position { get; private set; }
        public string Username { get; private set; }

        public AddEditEmployeeForm(List<string> existingUsernames)
        {
            InitializeComponent();
            comboUsername.Items.AddRange(existingUsernames.ToArray());
        }

        public AddEditEmployeeForm(Employee employee, List<string> existingUsernames) : this(existingUsernames)
        {
            txtFirstName.Text = employee.FirstName;
            txtLastName.Text = employee.LastName;
            txtPosition.Text = employee.Position;
            comboUsername.Text = employee.Username;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FirstName = txtFirstName.Text;
            LastName = txtLastName.Text;
            Position = txtPosition.Text;
            Username = comboUsername.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
