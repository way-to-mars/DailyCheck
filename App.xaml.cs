using DailyCheck.Code;
using DailyCheck.PageObjects;
using System.Diagnostics;
using System.Windows;
using static DailyCheck.DebugLogger;
using static DailyCheck.FileIO;

namespace DailyCheck
{
    public partial class App : Application
    {
        public SettingsFile SettingsFile { get; } = new SettingsFile();

        public App()
        {            
            Startup += new StartupEventHandler(App_Startup);
        }

        async void App_Startup(object sender, StartupEventArgs e)
        {
            await Task.Run(() => {
                DriverProvider.CleanOldData();  // Delete old temp data in background thread
            });
            
            string argFile = GetFirstArgAsFilename();

            if (argFile.Length > 0)
            {
                Log($"Found a file in the first argument: {argFile}");
                if (UserProfileFile.Load(argFile, out string login, out string password))
                {
                    Log($"Loaded from argFile: login={login} password={password}");
                    MainWindow = new RewardWindow(login, password);
                }
                else
                {
                    MessageBox.Show($"Не удалось прочитать файл:\n{argFile}");
                    MainWindow = new RegisterWindow();
                }
            }
            else
            {
                Log($"There's no file in the first argument");
                MainWindow = new RegisterWindow();
            }

            MainWindow?.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Debug.WriteLine("App says goodbye");
        }
    }
}
