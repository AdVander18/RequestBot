using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TelegramBOT
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.EnableVisualStyles();
            Application.ThreadException += (sender, e) => LogException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogException(e.ExceptionObject as Exception);

            // Создаем Splash Screen и главную форму
            var splash = new Intro.SplashScreen();
            MainForm mainForm = null;

            // Показываем Splash Screen
            splash.Show();

            // Запускаем фоновую загрузку
            Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i <= 100; i++)
                    {
                        splash.BeginInvoke((Action)(() =>
                            splash.UpdateStatus($"Загрузка: {i}%")));
                        Thread.Sleep(50);
                    }
                }
                finally
                {
                    // Закрытие Splash и показ MainForm в UI-потоке
                    splash.BeginInvoke((Action)(() =>
                    {
                        mainForm = new MainForm();
                        mainForm.FormClosed += (s, e) => Application.Exit(); // Обработчик закрытия
                        mainForm.Shown += (s, e) =>
                        {
                            if (!splash.IsDisposed)
                            {
                                splash.Close();
                                splash.Dispose();
                            }
                        };
                        mainForm.Show();
                    }));
                }
            });

            // Запускаем цикл сообщений
            Application.Run(mainForm);

        }
        private static void LogException(Exception ex)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyAppLog.txt");
            File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR: {ex?.ToString()}\n");
            MessageBox.Show($"Critical error: {ex?.Message}\nCheck log: {logPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}