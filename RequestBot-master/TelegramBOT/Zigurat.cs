using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace TelegramBOT
{
    public partial class Zigurat : Form
    {
        private const int gridWidth = 10;
        private const int gridHeight = 20;
        private int blockSize;
        private ZigguratBlock currentBlock;
        private Color[,] grid;
        private Timer gameTimer;
        private int score;
        private int currentLevel = 1;
        private int blocksToNextLevel = 5;

        public Zigurat()
        {
            InitializeComponent();
            InitializeGame();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        private void InitializeGame()
        {
            blockSize = ClientSize.Width / gridWidth;
            grid = new Color[gridWidth, gridHeight];
            score = 0;
            currentLevel = 1;
            blocksToNextLevel = 5;

            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            GenerateNewBlock();
        }

        private void GenerateNewBlock()
        {
            var shapes = new List<int[,]>
            {
                new int[,] { { 1, 1 }, { 1, 1 } },         // Квадрат
                new int[,] { { 1, 1, 1 }, { 0, 1, 0 } },   // T-образная
                new int[,] { { 1, 1, 0 }, { 0, 1, 1 } },   // Z-образная
                new int[,] { { 1 }, { 1 }, { 1 }, { 1 } }  // Палка
            };

            currentBlock = new ZigguratBlock
            {
                Position = new Point(gridWidth / 2 - 1, 0),
                LevelColor = GetLevelColor(),
                Shape = shapes[new Random().Next(shapes.Count)]
            };

            if (CheckCollision())
            {
                gameTimer.Stop();
                MessageBox.Show($"Игра окончена! Счет: {score}");
                SaveHighScore();
                InitializeGame();
            }
        }

        private Color GetLevelColor()
        {
            Color[] colors = { Color.Sienna, Color.Peru, Color.BurlyWood };
            return colors[(currentLevel - 1) % colors.Length];
        }

        private void GameLoop(object sender, EventArgs e)
        {
            MoveBlockDown();
        }

        private void MoveBlockDown()
        {
            currentBlock.Position = new Point(currentBlock.Position.X, currentBlock.Position.Y + 1);

            if (CheckCollision())
            {
                currentBlock.Position = new Point(currentBlock.Position.X, currentBlock.Position.Y - 1);
                MergeBlockToGrid();
                CheckCompletedRows();
                GenerateNewBlock();
            }

            Invalidate();
        }

        private bool CheckCollision()
        {
            for (int i = 0; i < currentBlock.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < currentBlock.Shape.GetLength(1); j++)
                {
                    if (currentBlock.Shape[i, j] == 1)
                    {
                        int x = currentBlock.Position.X + j;
                        int y = currentBlock.Position.Y + i;

                        if (x < 0 || x >= gridWidth || y >= gridHeight)
                            return true;

                        if (y >= 0 && grid[x, y] != Color.Empty)
                            return true;
                    }
                }
            }
            return false;
        }

        private void MergeBlockToGrid()
        {
            for (int i = 0; i < currentBlock.Shape.GetLength(0); i++)
            {
                for (int j = 0; j < currentBlock.Shape.GetLength(1); j++)
                {
                    if (currentBlock.Shape[i, j] == 1)
                    {
                        int x = currentBlock.Position.X + j;
                        int y = currentBlock.Position.Y + i;

                        if (y >= 0)
                            grid[x, y] = currentBlock.LevelColor;
                    }
                }
            }
        }

        private void CheckCompletedRows()
        {
            for (int y = gridHeight - 1; y >= 0; y--)
            {
                bool rowFull = true;
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[x, y] == Color.Empty)
                    {
                        rowFull = false;
                        break;
                    }
                }

                if (rowFull)
                {
                    score += 100;
                    blocksToNextLevel--;

                    if (blocksToNextLevel == 0)
                    {
                        currentLevel++;
                        blocksToNextLevel = currentLevel * 3;
                    }

                    for (int x = 0; x < gridWidth; x++)
                        grid[x, y] = Color.Empty;

                    ShiftRowsDown(y);
                    y++; // Повторная проверка той же Y-координаты
                }
            }
        }

        private void ShiftRowsDown(int startY)
{
    // Сдвигаем все строки выше удаленной вниз
    for (int y = startY; y > 0; y--)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            grid[x, y] = grid[x, y - 1];
        }
    }

    // Очищаем самую верхнюю строку
    for (int x = 0; x < gridWidth; x++)
    {
        grid[x, 0] = Color.Empty;
    }
}

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // Рисуем сетку
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (grid[x, y] != Color.Empty)
                    {
                        g.FillRectangle(new SolidBrush(grid[x, y]),
                            x * blockSize, y * blockSize,
                            blockSize - 1, blockSize - 1);
                    }
                }
            }

            // Рисуем текущий блок
            if (currentBlock != null)
            {
                for (int i = 0; i < currentBlock.Shape.GetLength(0); i++)
                {
                    for (int j = 0; j < currentBlock.Shape.GetLength(1); j++)
                    {
                        if (currentBlock.Shape[i, j] == 1)
                        {
                            g.FillRectangle(new SolidBrush(currentBlock.LevelColor),
                                (currentBlock.Position.X + j) * blockSize,
                                (currentBlock.Position.Y + i) * blockSize,
                                blockSize - 1, blockSize - 1);
                        }
                    }
                }
            }

            // Отображаем информацию
            g.DrawString($"Счет: {score}\nУровень: {currentLevel}",
                new Font("Arial", 14), Brushes.Gold, 10, 10);
        }

        private void SaveHighScore()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                if (File.Exists("scores.xml"))
                    doc.Load("scores.xml");
                else
                    doc.LoadXml("<Scores></Scores>");

                XmlElement newScore = doc.CreateElement("Score");
                newScore.InnerText = score.ToString();
                doc.DocumentElement.AppendChild(newScore);
                doc.Save("scores.xml");
            }
            catch { /* Обработка ошибок */ }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Point newPos = currentBlock.Position;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    newPos.X--;
                    break;
                case Keys.Right:
                    newPos.X++;
                    break;
                case Keys.Up:
                    RotateBlock();
                    break;
                case Keys.Down:
                    gameTimer.Interval = 100;
                    break;
            }

            if (e.KeyCode != Keys.Up)
            {
                Point oldPos = currentBlock.Position;
                currentBlock.Position = newPos;

                if (CheckCollision())
                    currentBlock.Position = oldPos;

                Invalidate();
            }
        }

        private void RotateBlock()
        {
            int[,] oldShape = currentBlock.Shape;
            int[,] newShape = new int[oldShape.GetLength(1), oldShape.GetLength(0)];

            for (int i = 0; i < newShape.GetLength(0); i++)
                for (int j = 0; j < newShape.GetLength(1); j++)
                    newShape[i, j] = oldShape[oldShape.GetLength(0) - j - 1, i];

            currentBlock.Shape = newShape;

            if (CheckCollision())
                currentBlock.Shape = oldShape;

            Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Down)
                gameTimer.Interval = 1000;
        }
    }

    public class ZigguratBlock
    {
        public Point Position { get; set; }
        public Color LevelColor { get; set; }
        public int[,] Shape { get; set; }
    
    }
}
