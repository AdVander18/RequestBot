using System;
using System.Windows.Forms;

namespace TelegramBOT
{
    public partial class AllMessages : Form
    {
        private readonly Database _database;

        public AllMessages(Database database)
        {
            InitializeComponent();
            _database = database;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        private void AllMessages_Load(object sender, EventArgs e)
        {
            var messages = _database.GetAllMessages();
            foreach (var msg in messages)
            {
                textBox1.AppendText($"{msg.Timestamp}: {msg.Username} - {msg.Text}{Environment.NewLine}");
            }

        }
    }
}