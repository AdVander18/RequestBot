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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.йоуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.починиПринтерToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.змейкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.зигуратToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button2 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(288, 27);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(512, 423);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 42);
            this.button1.TabIndex = 1;
            this.button1.Text = "Все сообщения";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonOpenAllMessages_Click);
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(93, 27);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(177, 155);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
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
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 75);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 42);
            this.button2.TabIndex = 4;
            this.button2.Text = "Задания";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.buttonOpenTasks_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::TelegramBOT.Properties.Resources.qr;
            this.pictureBox1.Location = new System.Drawing.Point(12, 309);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(129, 129);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 293);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Начните работу с ботом";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(2, 123);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(90, 42);
            this.button3.TabIndex = 7;
            this.button3.Text = "Оборудование";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TaskManager";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem йоуToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripMenuItem починиПринтерToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem змейкаToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem зигуратToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
    }
}

