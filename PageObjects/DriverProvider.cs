using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.IO;
using static DailyCheck.DebugLogger;

namespace DailyCheck.PageObjects
{
    class DriverProvider: IDisposable
    {
        private readonly string _browserDataDir = Path.Combine(AppContext.BaseDirectory, $"browserdata{DateTime.Now.Ticks:X}");
        public IWebDriver Driver { get; }

        public DriverProvider()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments($"user-data-dir={_browserDataDir}");
            options.AddArguments("headless");
            options.AddArguments("incognito");

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            ChromeDriver webDriver = new ChromeDriver(driverService, options);
            webDriver.Manage().Window.Maximize();
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            Driver = webDriver;
        }

        public void Dispose()
        {
            Driver.Quit();
            Driver.Dispose();
            DeleteBrowserData();
        }

        protected async void DeleteBrowserData()
        {
            do
            {
                try
                {
                    Log($"Deleting directory [{_browserDataDir}] using Directory.Delete");
                    Directory.Delete(_browserDataDir, true);
                }
                catch
                {
                    Log($"Deleting directory [{_browserDataDir}] using CMD.exe");
                    Process.Start("CMD.exe", $"/C RD /S /Q {_browserDataDir}").WaitForExit();
                    await Task.Delay(10);
                }
            } while (Directory.Exists(_browserDataDir));
            Log($"Directory [{_browserDataDir}] is deleted ✓");
        }
    }
}
