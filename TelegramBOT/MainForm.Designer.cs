namespace TelegramBOT
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxMessages = new System.Windows.Forms.TextBox();
            this.btnOpenAllMessages = new System.Windows.Forms.Button();
            this.listViewUsers = new System.Windows.Forms.ListView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.йоуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.починиПринтерToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.змейкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.зигуратToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnOpenTasks = new System.Windows.Forms.Button();
            this.pbQrCode = new System.Windows.Forms.PictureBox();
            this.lbStartBot = new System.Windows.Forms.Label();
            this.btnEquipment = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbQrCode)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxMessages
            // 
            this.textBoxMessages.Location = new System.Drawing.Point(288, 27);
            this.textBoxMessages.Multiline = true;
            this.textBoxMessages.Name = "textBoxMessages";
            this.textBoxMessages.ReadOnly = true;
            this.textBoxMessages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxMessages.Size = new System.Drawing.Size(512, 423);
            this.textBoxMessages.TabIndex = 0;
            // 
            // btnOpenAllMessages
            // 
            this.btnOpenAllMessages.Location = new System.Drawing.Point(12, 27);
            this.btnOpenAllMessages.Name = "btnOpenAllMessages";
            this.btnOpenAllMessages.Size = new System.Drawing.Size(75, 42);
            this.btnOpenAllMessages.TabIndex = 1;
            this.btnOpenAllMessages.Text = "Все сообщения";
            this.btnOpenAllMessages.UseVisualStyleBackColor = true;
            this.btnOpenAllMessages.Click += new System.EventHandler(this.buttonOpenAllMessages_Click);
            // 
            // listViewUsers
            // 
            this.listViewUsers.HideSelection = false;
            this.listViewUsers.Location = new System.Drawing.Point(93, 27);
            this.listViewUsers.MultiSelect = false;
            this.listViewUsers.Name = "listViewUsers";
            this.listViewUsers.Size = new System.Drawing.Size(177, 155);
            this.listViewUsers.TabIndex = 2;
            this.listViewUsers.UseCompatibleStateImageBehavior = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.йоуToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // йоуToolStripMenuItem
            // 
            this.йоуToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.починиПринтерToolStripMenuItem,
            this.змейкаToolStripMenuItem,
            this.зигуратToolStripMenuItem});
            this.йоуToolStripMenuItem.Name = "йоуToolStripMenuItem";
            this.йоуToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.йоуToolStripMenuItem.Text = "Развлекуха";
            // 
            // починиПринтерToolStripMenuItem
            // 
            this.починиПринтерToolStripMenuItem.Name = "починиПринтерToolStripMenuItem";
            this.починиПринтерToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.починиПринтерToolStripMenuItem.Text = "Почини принтер";
            this.починиПринтерToolStripMenuItem.Click += new System.EventHandler(this.починиПринтерToolStripMenuItem_Click);
            // 
            // змейкаToolStripMenuItem
            // 
            this.змейкаToolStripMenuItem.Name = "змейкаToolStripMenuItem";
            this.змейкаToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.змейкаToolStripMenuItem.Text = "Змейка";
            this.змейкаToolStripMenuItem.Click += new System.EventHandler(this.змейкаToolStripMenuItem_Click);
            // 
            // зигуратToolStripMenuItem
            // 
            this.зигуратToolStripMenuItem.Name = "зигуратToolStripMenuItem";
            this.зигуратToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.зигуратToolStripMenuItem.Text = "Зигурат";
            this.зигуратToolStripMenuItem.Click += new System.EventHandler(this.зигуратToolStripMenuItem_Click);
            // 
            // btnOpenTasks
            // 
            this.btnOpenTasks.Location = new System.Drawing.Point(12, 75);
            this.btnOpenTasks.Name = "btnOpenTasks";
            this.btnOpenTasks.Size = new System.Drawing.Size(75, 42);
            this.btnOpenTasks.TabIndex = 4;
            this.btnOpenTasks.Text = "Задания";
            this.btnOpenTasks.UseVisualStyleBackColor = true;
            this.btnOpenTasks.Click += new System.EventHandler(this.buttonOpenTasks_Click);
            // 
            // pbQrCode
            // 
            this.pbQrCode.Image = global::TelegramBOT.Properties.Resources.qr;
            this.pbQrCode.Location = new System.Drawing.Point(12, 309);
            this.pbQrCode.Name = "pbQrCode";
            this.pbQrCode.Size = new System.Drawing.Size(129, 129);
            this.pbQrCode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbQrCode.TabIndex = 5;
            this.pbQrCode.TabStop = false;
            // 
            // lbStartBot
            // 
            this.lbStartBot.AutoSize = true;
            this.lbStartBot.Location = new System.Drawing.Point(12, 293);
            this.lbStartBot.Name = "lbStartBot";
            this.lbStartBot.Size = new System.Drawing.Size(129, 13);
            this.lbStartBot.TabIndex = 6;
            this.lbStartBot.Text = "Начните работу с ботом";
            // 
            // btnEquipment
            // 
            this.btnEquipment.Location = new System.Drawing.Point(2, 123);
            this.btnEquipment.Name = "btnEquipment";
            this.btnEquipment.Size = new System.Drawing.Size(90, 42);
            this.btnEquipment.TabIndex = 7;
            this.btnEquipment.Text = "Оборудование";
            this.btnEquipment.UseVisualStyleBackColor = true;
            this.btnEquipment.Click += new System.EventHandler(this.btnEquipment_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnEquipment);
            this.Controls.Add(this.lbStartBot);
            this.Controls.Add(this.pbQrCode);
            this.Controls.Add(this.btnOpenTasks);
            this.Controls.Add(this.listViewUsers);
            this.Controls.Add(this.btnOpenAllMessages);
            this.Controls.Add(this.textBoxMessages);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TaskManager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbQrCode)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxMessages;
        private System.Windows.Forms.Button btnOpenAllMessages;
        private System.Windows.Forms.ListView listViewUsers;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem йоуToolStripMenuItem;
        private System.Windows.Forms.Button btnOpenTasks;
        private System.Windows.Forms.ToolStripMenuItem починиПринтерToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem змейкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem зигуратToolStripMenuItem;
        private System.Windows.Forms.PictureBox pbQrCode;
        private System.Windows.Forms.Label lbStartBot;
        private System.Windows.Forms.Button btnEquipment;
    }
}

