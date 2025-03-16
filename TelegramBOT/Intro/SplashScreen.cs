using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace TelegramBOT.Intro
{
    public partial class SplashScreen : Form
    {
        private System.Windows.Forms.Timer animationTimer;
        private float angle = 0;

        public SplashScreen()
        {
            InitializeComponent();

            InitializeForm();
            InitializeAnimation();
        }

        private void InitializeForm()
        {
            // Настройки формы
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.Size = new Size(400, 300);
            this.DoubleBuffered = true;

            // Элементы управления
            var logo = new PictureBox
            {
                Image = Properties.Resources.logo, // Добавьте свой логотип в ресурсы
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(100, 100),
                Location = new Point(150, 50)
            };

            var progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Size = new Size(300, 20),
                Location = new Point(50, 200),
                MarqueeAnimationSpeed = 30
            };

            var statusLabel = new Label
            {
                Text = "Загрузка...",
                AutoSize = false,
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 160)
            };

            Controls.Add(logo);
            Controls.Add(progressBar);
            Controls.Add(statusLabel);
        }

        private void InitializeAnimation()
        {
            animationTimer = new System.Windows.Forms.Timer { Interval = 50 };
            animationTimer.Tick += (s, e) =>
            {
                angle = (angle + 10) % 360;
                Invalidate();
            };
            animationTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Рисуем градиентный фон
            using (var brush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(0, 122, 204),
                Color.FromArgb(28, 28, 28),
                45f))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            // Рисуем анимированное кольцо
            var rect = new Rectangle(Width - 100, Height - 100, 80, 80);
            using (var pen = new Pen(Color.White, 4))
            using (var path = new GraphicsPath())
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                path.AddArc(rect, angle, 270);
                e.Graphics.DrawPath(pen, path);
            }
        }

        public void UpdateStatus(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(UpdateStatus), text);
                return;
            }

            if (Controls.Count > 2 && Controls[2] is Label statusLabel)
            {
                statusLabel.Text = text;
                statusLabel.Refresh();
            }
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Гарантируем видимость окна
            this.BringToFront();
            this.Activate();
            this.Focus();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true; // Откладываем закрытие для анимации
            StartClosingAnimation();
        }

        private async void StartClosingAnimation()
        {
            // Анимация прозрачности
            for (double opacity = 1.0; opacity > 0; opacity -= 0.05)
            {
                if (this.IsDisposed) break;
                this.Opacity = opacity;
                await Task.Delay(30);
            }

            // Корректно закрываем форму после анимации
            this.BeginInvoke(new Action(() =>
            {
                this.Close();
                this.Dispose();
            }));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            this.Dispose();
        }

        // Убираем лишние переопределения
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Visible = true;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            this.Refresh();
        }
    }
}
