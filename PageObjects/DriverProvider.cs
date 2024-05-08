using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.IO;
using static DailyCheck.DebugLogger;
using Path = System.IO.Path;

namespace DailyCheck.PageObjects
{
    class DriverProvider : IDisposable
    {
        private static readonly string ChromeBinaries = Path.Combine(AppContext.BaseDirectory, "ChromeBinaries");
        private static readonly string ChromeBinary = Path.Combine(ChromeBinaries, "chrome-headless-shell\\chrome-headless-shell.exe");
        private static readonly string DriverPath = Path.Combine(ChromeBinaries, "chromedriver");
        private static readonly string BrowserDataPrefix = "browserdata";

        private readonly string BrowserDataDir = Path.Combine(ChromeBinaries, $"{BrowserDataPrefix}{DateTime.Now.Ticks:X}");

        public IWebDriver Driver { get; }

        public DriverProvider()
        {
            ChromeOptions options = new();
            options.AddArguments($"user-data-dir={BrowserDataDir}"); // user-data-path
            options.AddArguments("headless=new");
            // options.AddArgument("--no-sandbox");  // for some issues like "... WebDriver timed out after 60 seconds"
            options.AddArguments("incognito");
            options.BinaryLocation = ChromeBinary;

            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService(DriverPath);
            driverService.HideCommandPromptWindow = true;

            Log(ChromeBinaries);
            Log(ChromeBinary);
            Log(DriverPath);
            Log(BrowserDataDir);

            try
            {
                Driver = new ChromeDriver(driverService, options);
            }
            catch
            {
                Log("Default Driver");
                Driver = new ChromeDriver();
            }
        }

        public void Dispose()
        {
            Driver.Quit();
            Driver.Dispose();
            DeleteBrowserData();
        }

        private void DeleteBrowserData()
        {
            int errorLimit = 10;
            int tryoutsLimit = 30;

            while (Directory.Exists(BrowserDataDir))
                try
                {
                    TryToDeleteFolder(BrowserDataDir).Wait(TimeSpan.FromSeconds(1));
                    if (--tryoutsLimit == 0)
                    {
                        Log($"DeleteBrowserData. Directory [{BrowserDataDir}] can not be deleted 🗙");
                        return;
                    }
                }
                catch
                {
                    if (--errorLimit == 0)
                    {
                        Log($"DeleteBrowserData. Directory [{BrowserDataDir}] can not be deleted 🗙");
                        return;
                    }
                }
            Log($"DeleteBrowserData. Directory [{BrowserDataDir}] is deleted ✓");
        }

        private static async Task TryToDeleteFolder(string path)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch
            {
                try
                {
                    await Task.Delay(10);
                    Process.Start("CMD.exe", $"/C RD /S /Q {path}").WaitForExit();
                }
                catch { /* ignore the exceptions */ }
            }
        }

        public static void CleanOldData()
        {
            static string[] GetDirectoriesOrEmpty(string p)
            {
                try { return Directory.GetDirectories(p); }
                catch { return []; }
            }

            Task.Run(() =>
            {
                GetDirectoriesOrEmpty(ChromeBinaries)
                    .AsParallel()
                    .Select(p => new DirectoryInfo(p))
                    .Where(di => di.Name.StartsWith(BrowserDataPrefix))
                    .ForAll(di => _ = TryToDeleteFolder(di.FullName));
            });
        }
    }
}
