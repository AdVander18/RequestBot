using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TelegramBOT
{
    public partial class SnakeGame : Form
    {
        private const int CellSize = 32;
        private const int GridSize = 20;
        private readonly Timer gameTimer = new Timer();
        private Direction currentDirection = Direction.Right;
        private Direction nextDirection = Direction.Right;
        private List<Point> snake = new List<Point>();
        private Point food;
        private int score;
        private bool isGameOver;

        private enum Direction { Up, Down, Left, Right }

        public SnakeGame()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            InitializeComponent();
            InitializeGame();
            SetupTimer();
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void InitializeGame()
        {
            snake.Clear();
            snake.Add(new Point(5, 5));
            snake.Add(new Point(4, 5));
            snake.Add(new Point(3, 5));
            currentDirection = Direction.Right;
            nextDirection = Direction.Right;
            score = 0;
            isGameOver = false;
            GenerateFood();
        }

        private void SetupTimer()
        {
            gameTimer.Interval = 100;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        private void GenerateFood()
        {
            var random = new Random();
            do
            {
                food = new Point(
                    random.Next(0, GridSize),
                    random.Next(0, GridSize));
            } while (snake.Contains(food));
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isGameOver) return;

            currentDirection = nextDirection;
            MoveSnake();

            if (CheckCollision())
            {
                GameOver();
                return;
            }

            CheckFood();
            this.Invalidate();
        }

        private void MoveSnake()
        {
            for (int i = snake.Count - 1; i > 0; i--)
            {
                snake[i] = snake[i - 1];
            }

            var head = snake[0];
            switch (currentDirection)
            {
                case Direction.Up:
                    head.Y--;
                    break;
                case Direction.Down:
                    head.Y++;
                    break;
                case Direction.Left:
                    head.X--;
                    break;
                case Direction.Right:
                    head.X++;
                    break;
            }
            snake[0] = head;
        }

        private bool CheckCollision()
        {
            var head = snake[0];

            // Check walls collision
            if (head.X < 0 || head.X >= GridSize ||
                head.Y < 0 || head.Y >= GridSize)
                return true;

            // Check self collision
            for (int i = 1; i < snake.Count; i++)
            {
                if (head == snake[i])
                    return true;
            }

            return false;
        }

        private void CheckFood()
        {
            if (snake[0] == food)
            {
                snake.Add(new Point(-1, -1));
                score += 10;
                GenerateFood();
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();
            MessageBox.Show($"Game Over! Score: {score}\nPress OK to restart");
            InitializeGame();
            gameTimer.Start();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;

            switch (e.KeyCode)
            {
                case Keys.Up when currentDirection != Direction.Down:
                    nextDirection = Direction.Up;
                    break;
                case Keys.Down when currentDirection != Direction.Up:
                    nextDirection = Direction.Down;
                    break;
                case Keys.Left when currentDirection != Direction.Right:
                    nextDirection = Direction.Left;
                    break;
                case Keys.Right when currentDirection != Direction.Left:
                    nextDirection = Direction.Right;
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            DrawGrid(e.Graphics);
            DrawSnake(e.Graphics);
            DrawFood(e.Graphics);
            DrawScore(e.Graphics);
        }

        private void DrawGrid(Graphics g)
        {
            using (var pen = new Pen(Color.FromArgb(50, 50, 50)))
            {
                for (int i = 0; i <= GridSize; i++)
                {
                    g.DrawLine(pen, i * CellSize, 0, i * CellSize, GridSize * CellSize);
                    g.DrawLine(pen, 0, i * CellSize, GridSize * CellSize, i * CellSize);
                }
            }
        }

        private void DrawSnake(Graphics g)
        {
            for (int i = 0; i < snake.Count; i++)
            {
                var rect = new Rectangle(
                    snake[i].X * CellSize,
                    snake[i].Y * CellSize,
                    CellSize,
                    CellSize);

                using (var path = CreateRoundedRect(rect, 5))
                {
                    // Разделяем создание кистей для разных частей змеи
                    if (i == 0) // Head
                    {
                        using (var brush = new LinearGradientBrush(
                            rect,
                            Color.FromArgb(0, 100, 0),
                            Color.FromArgb(34, 139, 34),
                            LinearGradientMode.BackwardDiagonal))
                        {
                            g.FillPath(brush, path);
                            g.DrawPath(Pens.DarkGreen, path);
                        }
                    }
                    else if (i == snake.Count - 1) // Tail
                    {
                        using (var brush = new LinearGradientBrush(
                            rect,
                            Color.FromArgb(50, 205, 50),
                            Color.FromArgb(0, 128, 0),
                            LinearGradientMode.ForwardDiagonal))
                        {
                            g.FillPath(brush, path);
                            g.DrawPath(Pens.DarkGreen, path);
                        }
                    }
                    else // Body
                    {
                        using (var brush = new LinearGradientBrush(
                            rect,
                            Color.FromArgb(0, 128, 0),
                            Color.FromArgb(50, 205, 50),
                            LinearGradientMode.ForwardDiagonal))
                        {
                            g.FillPath(brush, path);
                            g.DrawPath(Pens.DarkGreen, path);
                        }
                    }
                }
            }
        }

        private void DrawFood(Graphics g)
        {
            var rect = new Rectangle(
                food.X * CellSize,
                food.Y * CellSize,
                CellSize,
                CellSize);

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (var brush = new LinearGradientBrush(
                    rect,
                    Color.Red,
                    Color.DarkRed,
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillPath(brush, path);
                    g.DrawPath(Pens.DarkRed, path);
                }
            }
        }

        private void DrawScore(Graphics g)
        {
            using (var font = new Font("Arial", 14, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            {
                g.DrawString($"Score: {score}", font, brush, 10, 10);
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int cornerRadius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.ClientSize = new Size(GridSize * CellSize, GridSize * CellSize);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // Останавливаем таймер при закрытии формы
            gameTimer.Stop();
        }

        private void SnakeGame_Load(object sender, EventArgs e)
        {

        }
    }
}
