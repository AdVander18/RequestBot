using System;
using System.Threading;
using System.Windows.Forms;

namespace TelegramBOT
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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
    }
}