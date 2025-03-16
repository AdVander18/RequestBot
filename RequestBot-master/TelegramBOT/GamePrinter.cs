using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrinterRepairMaster
{
    public partial class GamePrinter : Form
    {
        public GamePrinter()
        {
            InitializeComponent();
            SetupGame();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }
        private void SetupGame()
        {
            // Показываем инструкцию перед началом игры
            MessageBox.Show("Добро пожаловать в Printer Repair Master!\n" +
                            "Ваша задача — починить принтер, пройдя несколько этапов:\n" +
                            "1. Удалите пятна с помощью губки.\n" +
                            "2. Настройте шестерёнки на правильные значения.\n" +
                            "3. Удерживайте кнопку 3 секунды.\n" +
                            "4. Выполните последовательность нажатий клавиш (QTE).\n" +
                            "Удачи!", "Инструкция", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Остальная инициализация игры
            this.Text = "Printer Repair Master";
            this.ClientSize = new Size(800, 600);
            this.DoubleBuffered = true;

            // Инициализация компонентов
            InitializeProgressBar();
            InitializeLivesLabel();
            InitializeHintLabel();
            InitializePrinterImage(); // 1. Сначала создаём PictureBox принтера
            InitializeSponge();       // 2. Затем губку
            AddDirtSpots();           // 3. Теперь можно создавать пятна
            InitializeGears();
            InitializeHoldButton();
            InitializeTimers();

            // Начальные настройки
            ShuffleQteSequence();
            StartNextStage();
        }

        private void InitializeSponge()
        {
            // Убираем локальное объявление PictureBox
            pbSponge = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(50, 500),
                BackColor = Color.Yellow,
                Visible = false
            };
            pbSponge.MouseDown += pbSponge_MouseDown;
            pbSponge.MouseMove += pbSponge_MouseMove;
            this.Controls.Add(pbSponge);
        }
        private ProgressBar repairProgress;
        private Label lblLives;
        private Label lblHint;
        private PictureBox pbPrinter;
        private NumericUpDown gear1;
        private NumericUpDown gear2;
        private NumericUpDown gear3;
        private Button btnHold;

        // Игровые переменные
        private Random rnd = new Random();
        private int lives = 3;
        private int gearAttemptsLeft = 2;
        private List<Keys> qteSequence = new List<Keys>();
        private int currentQteIndex = 0;
        private Timer holdTimer = new Timer();
        private List<PictureBox> dirtSpots = new List<PictureBox>();
        private bool[] completedStages = new bool[4];
        private bool[] correctGears = new bool[3];
        private bool qteInputEnabled = true;
        private Timer qteCooldownTimer = new Timer { Interval = 300 };
        private int[] correctGearValues = new int[3];
        private Point spongeOffset;
        private DateTime holdStartTime;

        private void AddDirtSpots()
        {
            string dirtImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "dirt.png");
            if (!File.Exists(dirtImagePath))
            {
                MessageBox.Show("Файл 'dirt.png' не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int numberOfDirtSpots = rnd.Next(3, 6); // Генерируем от 3 до 5 пятен
            for (int i = 0; i < numberOfDirtSpots; i++)
            {
                PictureBox dirtSpot = new PictureBox
                {
                    Size = new Size(30, 30),
                    ImageLocation = dirtImagePath,
                    BackColor = Color.Transparent,
                    Location = new Point(
                        rnd.Next(pbPrinter.Left + 10, pbPrinter.Right - 40), // Отступ слева и справа
                        rnd.Next(pbPrinter.Top + 10, pbPrinter.Bottom - 40)), // Отступ сверху и снизу
                    Tag = "dirt"
                };
                this.Controls.Add(dirtSpot);
                dirtSpot.BringToFront(); // Перемещаем пятно поверх других элементов
                dirtSpots.Add(dirtSpot);
            }
        }

        private void ShuffleQteSequence()
        {
            // Очищаем предыдущую последовательность QTE
            qteSequence.Clear();

            // Генерируем новую случайную последовательность из 3-5 клавиш
            int sequenceLength = rnd.Next(3, 6); // Длина последовательности: от 3 до 5
            for (int i = 0; i < sequenceLength; i++)
            {
                // Добавляем случайные клавиши (например, A, S, D)
                Keys randomKey = (Keys)rnd.Next((int)Keys.A, (int)Keys.Z + 1);
                qteSequence.Add(randomKey);
            }

            // Обнуляем индекс для начала новой последовательности
            currentQteIndex = 0;

            // Выводим подсказку о первой клавише в последовательности
            lblHint.Text = $"Нажми: {qteSequence[currentQteIndex]}";
        }

        private void StartNextStage()
        {
            // Проверяем, сколько этапов уже завершено
            int completedStagesCount = 0;
            for (int i = 0; i < completedStages.Length; i++)
            {
                if (completedStages[i]) completedStagesCount++;
            }

            // Если все этапы завершены, показываем победу
            if (completedStagesCount == completedStages.Length)
            {
                MessageBox.Show("Поздравляю! Ты починил принтер!", "ЁЁЁУ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Закрываем игру после победы
                return;
            }

            // Определяем текущий этап
            int currentStage = Array.IndexOf(completedStages, false);

            // Обновляем подсказку о текущем этапе
            lblHint.Text = $"Этап {currentStage + 1}: Следуй инструкциям!";

            // Логика для каждого этапа
            switch (currentStage)
            {
                case 0: // Этап 1: Удаление пятен с принтера
                    pbSponge.Visible = true;
                    lblHint.Text = "Используйте губку, чтобы удалить пятна!";
                    break;

                case 1: // Этап 2: Настройка шестерёнок
                    GenerateCorrectGearValues();
                    correctGears = new bool[3];
                    gear1.Value = 1;
                    gear2.Value = 1;
                    gear3.Value = 1;
                    gear1.ForeColor = Color.Black;
                    gear2.ForeColor = Color.Black;
                    gear3.ForeColor = Color.Black;
                    gear1.Enabled = true;
                    gear2.Enabled = true;
                    gear3.Enabled = true;
                    lblHint.Text = "Настройте передачи на правильные значения!";
                    gearAttemptsLeft = 2; // Сброс попыток при входе в этап
                    break;

                case 2: // Этап 3: Удержание кнопки
                    btnHold.Enabled = true;
                    lblHint.Text = "Удерживайте кнопку 3 секунды!";
                    break;

                case 3: // Этап 4: Выполнение QTE
                    ShuffleQteSequence();
                    lblHint.Text = $"Нажми: {qteSequence[currentQteIndex]}";
                    break;
            }
        }

        private void CheckQteInput(Keys pressedKey)
        {
            if (!qteInputEnabled) return;

            // Блокируем ввод на время анимации
            qteInputEnabled = false;
            qteCooldownTimer.Start();

            if (pressedKey == qteSequence[currentQteIndex])
            {
                currentQteIndex++;
                lblHint.Text = $"Правильно! Осталось: {qteSequence.Count - currentQteIndex}";

                if (currentQteIndex >= qteSequence.Count)
                {
                    completedStages[3] = true;
                    StartNextStage();
                }
                else
                {
                    lblHint.Text += $"\nСледующая клавиша: {qteSequence[currentQteIndex]}";
                }
            }
            else
            {
                lives--;
                lblLives.Text = $"Жизни: {lives}";
                lblHint.Text = $"Ошибка! Ожидалось: {qteSequence[currentQteIndex]}";

                if (lives <= 0)
                {
                    MessageBox.Show("Game Over!");
                    this.Close();
                }
            }
        }

        private void InitializeProgressBar()
        {
            repairProgress = new ProgressBar
            {
                Location = new Point(50, 20),
                Size = new Size(700, 30),
                Style = ProgressBarStyle.Continuous,
                Maximum = 100
            };
            this.Controls.Add(repairProgress);
        }

        private void InitializeLivesLabel()
        {
            lblLives = new Label
            {
                Text = $"Lives: {lives}",
                Location = new Point(650, 70),
                Font = new Font("Arial", 14),
                AutoSize = true
            };
            this.Controls.Add(lblLives);
        }

        private void InitializeHintLabel()
        {
            lblHint = new Label
            {
                Text = "Start repairing!",
                Location = new Point(50, 70),
                Font = new Font("Arial", 12),
                AutoSize = true
            };
            this.Controls.Add(lblHint);
        }

        private void InitializePrinterImage()
        {
            string printerImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "printer.png");
            if (!File.Exists(printerImagePath))
            {
                MessageBox.Show("Файл 'printer.png' не найден!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            pbPrinter = new PictureBox
            {
                Size = new Size(400, 400),
                Location = new Point(300, 100),
                BackColor = Color.LightGray,
                ImageLocation = printerImagePath,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            this.Controls.Add(pbPrinter);
        }

        private void InitializeGears()
        {
            gear1 = new NumericUpDown
            {
                Tag = 0, // Используем Tag для идентификации шестерёнки
                Location = new Point(50, 150),
                Size = new Size(60, 20),
                Minimum = 1,
                Maximum = 3,
                Enabled = false
            };
            gear1.ValueChanged += Gear_ValueChanged;

            gear2 = new NumericUpDown
            {
                Tag = 1,
                Location = new Point(50, 200),
                Size = new Size(60, 20),
                Minimum = 1,
                Maximum = 3,
                Enabled = false
            };
            gear2.ValueChanged += Gear_ValueChanged;

            gear3 = new NumericUpDown
            {
                Tag = 2,
                Location = new Point(50, 250),
                Size = new Size(60, 20),
                Minimum = 1,
                Maximum = 3,
                Enabled = false
            };
            gear3.ValueChanged += Gear_ValueChanged;

            this.Controls.Add(gear1);
            this.Controls.Add(gear2);
            this.Controls.Add(gear3);
        }
        private void Gear_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown currentGear = (NumericUpDown)sender;
            int gearIndex = (int)currentGear.Tag;

            if (currentGear.Value == correctGearValues[gearIndex])
            {
                currentGear.ForeColor = Color.Green;
                currentGear.Enabled = false;
                correctGears[gearIndex] = true;
                lblHint.Text = $"Шестерня {gearIndex + 1} настроена правильно!";

                if (correctGears.All(x => x))
                {
                    completedStages[1] = true;
                    StartNextStage();
                }
            }
            else
            {
                currentGear.ForeColor = Color.Red;
                gearAttemptsLeft--;

                if (gearAttemptsLeft > 0)
                {
                    lblHint.Text = $"Неправильно! Осталось {gearAttemptsLeft} попыток.";
                }
                else
                {
                    lives--;
                    lblLives.Text = $"Жизни: {lives}";
                    lblHint.Text = "Попытки исчерпаны! Убавляется жизнь.";
                    gearAttemptsLeft = 2; // Сброс попыток после потери жизни

                    if (lives <= 0)
                    {
                        MessageBox.Show("Game Over!", "Проиграл", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Close();
                    }
                }
            }
        }

        private void GenerateCorrectGearValues()
        {
            for (int i = 0; i < correctGearValues.Length; i++)
            {
                correctGearValues[i] = rnd.Next(1, 4);
            }
        }

        private bool CheckGearValues()
        {
            // Сравниваем значения шестерёнок с правильными значениями
            if (gear1.Value == correctGearValues[0] &&
                gear2.Value == correctGearValues[1] &&
                gear3.Value == correctGearValues[2])
            {
                return true; // Все значения верны
            }
            return false; // Есть ошибки
        }

        private void BtnCheckGears_Click(object sender, EventArgs e)
        {
            // Проверяем значения шестерёнок
            if (CheckGearValues())
            {
                // Все значения верны
                completedStages[1] = true; // Отмечаем второй этап как завершённый
                StartNextStage();         // Переходим к следующему этапу
            }
            else
            {
                // Есть ошибки
                lives--; // Уменьшаем количество жизней
                lblLives.Text = $"Жизни: {lives}";
                if (lives <= 0)
                {
                    MessageBox.Show("Game Over!", "Проиграл", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                }
                else
                {
                    lblHint.Text = "Неверные значения! Попробуйте снова.";
                }
            }
        }

        private void InitializeHoldButton()
        {
            btnHold = new Button
            {
                Text = "Задержите",
                Location = new Point(50, 350),
                Size = new Size(120, 40),
                Enabled = false
            };
            btnHold.MouseDown += BtnHold_MouseDown;
            btnHold.MouseUp += BtnHold_MouseUp;
            this.Controls.Add(btnHold);
        }

        private void InitializeTimers()
        {
            qteCooldownTimer.Tick += (s, e) =>
            {
                qteInputEnabled = true;
                qteCooldownTimer.Stop();
            };
            holdTimer.Interval = 3000;
            holdTimer.Tick += HoldTimer_Tick;
        }

        private void GamePrinter_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // Форма будет получать события клавиш первой
            this.KeyDown += GamePrinter_KeyDown;
        }
        private void GamePrinter_KeyDown(object sender, KeyEventArgs e)
        {
            int currentStage = Array.IndexOf(completedStages, false);

            // Только для этапа QTE (case 3)
            if (currentStage == 3 && qteInputEnabled)
            {
                CheckQteInput(e.KeyCode);
            }
        }

        private void pbSponge_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Сохраняем смещение между позицией мыши и губкой
                spongeOffset = new Point(e.X, e.Y);
            }
        }

        private void pbSponge_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && pbSponge.Visible)
            {
                // Преобразуем глобальные координаты курсора в координаты относительно формы
                Point cursorPositionOnForm = this.PointToClient(Cursor.Position);

                // Вычисляем новую позицию губки относительно курсора
                Point newLocation = new Point(
                    cursorPositionOnForm.X - spongeOffset.X,
                    cursorPositionOnForm.Y - spongeOffset.Y
                );

                // Устанавливаем новую позицию губки
                pbSponge.Location = newLocation;
                pbSponge.BringToFront();
                // Проверяем пересечение губки с пятнами
                CheckDirtSpotCollision();
            }
        }

        private void CheckDirtSpotCollision()
        {
            // Проверяем, пересекается ли губка с каким-либо пятном
            for (int i = dirtSpots.Count - 1; i >= 0; i--)
            {
                PictureBox dirtSpot = dirtSpots[i];

                if (dirtSpot.Bounds.IntersectsWith(pbSponge.Bounds))
                {
                    // Если есть пересечение, удаляем пятно
                    dirtSpots.Remove(dirtSpot);
                    this.Controls.Remove(dirtSpot);
                    dirtSpot.Dispose();

                    // Проверяем, все ли пятна удалены
                    if (dirtSpots.Count == 0)
                    {
                        // Завершаем первый этап и переходим к следующему
                        completedStages[0] = true;
                        StartNextStage();
                    }

                    break; // Прекращаем проверку после удаления пятна
                }
            }
        }

        private void BtnHold_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && btnHold.Enabled)
            {
                holdStartTime = DateTime.Now; // Запоминаем время начала удержания
                lblHint.Text = "Удерживайте кнопку...";
            }
        }


        private void BtnHold_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && btnHold.Enabled)
            {
                TimeSpan holdDuration = DateTime.Now - holdStartTime; // Вычисляем длительность

                if (holdDuration.TotalSeconds >= 3)
                {
                    lblHint.Text = "Успешно! Этап пройден!";
                    completedStages[2] = true;
                    StartNextStage();
                }
                else
                {
                    lives--;
                    lblLives.Text = $"Жизни: {lives}";
                    lblHint.Text = $"Недостаточно долго! Нужно 3 сек. (Удержано: {holdDuration.TotalSeconds:F1} сек.)";

                    if (lives <= 0)
                    {
                        MessageBox.Show("Game Over!");
                        this.Close();
                    }
                }
            }
        }


        private void HoldTimer_Tick(object sender, EventArgs e)
        {
            holdTimer.Stop();
        }
    }
}