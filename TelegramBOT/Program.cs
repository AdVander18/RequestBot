using System;
using System.IO;
using System.Threading;
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

            // Создаем Splash Screen и его поток
            var splash = new Intro.SplashScreen();
            var splashThread = new Thread(() =>
            {
                Application.Run(splash);
            })
            {
                IsBackground = true
            };
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();

            // Имитация загрузки в главном потоке
            for (int i = 0; i <= 100; i++)
            {
                splash.UpdateStatus($"Загрузка: {i}%");
                Thread.Sleep(50);
            }

            // Закрываем Splash Screen
            splash.Invoke(new Action(() =>
            {
                splash.Close();
                splash.Dispose();
            }));

            // Дожидаемся завершения потока splash screen
            splashThread.Join();

            // Запускаем главную форму
            Application.Run(new MainForm());
        }
        private static void LogException(Exception ex)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyAppLog.txt");
            File.AppendAllText(logPath, $"[{DateTime.Now}] ERROR: {ex?.ToString()}\n");
            MessageBox.Show($"Critical error: {ex?.Message}\nCheck log: {logPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}