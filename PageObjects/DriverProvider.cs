using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using System.IO;
using static DailyCheck.DebugLogger;
using static DailyCheck.PageObjects.DriverProvider;
using Path = System.IO.Path;

namespace DailyCheck.PageObjects
{
    class DriverProvider : IDisposable
    {
        // All the version you can find here:
        // https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json
        public enum ChromiumVersions {
            Default = 0,
            V84 = 84,
            V120 = 120,
            V122 = 122,
            V123 = 123,
            V124 = 124,
            V125 = 125,
            V126 = 126,
        }
        private static readonly ChromiumVersions chromiumVersion = ChromiumVersions.V125;        

        private static readonly string ChromeBinaries = Path.Combine(AppContext.BaseDirectory, $"ChromeBinaries{chromiumVersion.ToPath()}");
        private static readonly string ChromeBinary = Path.Combine(ChromeBinaries, "chrome-headless-shell\\chrome-headless-shell.exe");
        private static readonly string DriverPath = Path.Combine(ChromeBinaries, "chromedriver");
        private static readonly string BrowserDataPrefix = "browserdata";

        private readonly string BrowserDataDir = Path.Combine(ChromeBinaries, $"{BrowserDataPrefix}{DateTime.Now.Ticks:X}");

        private Task<IWebDriver> _driverTask;
        public IWebDriver Driver {
            get
            {
                return _driverTask.Result;
            }
        }

        public DriverProvider()
        {
            // Driver = ChromeProvider();
            _driverTask = Task<IWebDriver>.Run(() => { 
                return FirefoxProvider();
            });
        }

        private IWebDriver ChromeProvider()
        {
            ChromeOptions options = new();
            options.AddArguments($"user-data-dir={BrowserDataDir}"); // user-data-path
            options.AddArguments("headless=new");
            options.AddArgument("--no-sandbox");  // for some issues like "... WebDriver timed out after 60 seconds"
            options.AddArguments("incognito");
            options.AddArguments("--disable-images");
            options.AddArguments("--disable-gpu");
            options.AddArguments("--disable-extensions");
            options.AddArguments("--disable-dev-shm-usage");
            options.AddArguments("--disable-software-rasterizer");
            options.AddArguments("--disable-blink-features=AutomationControlled");
            options.BinaryLocation = ChromeBinary;

            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService(DriverPath);
            driverService.HideCommandPromptWindow = true;

            Log(ChromeBinaries);
            Log(ChromeBinary);
            Log(DriverPath);
            Log(BrowserDataDir);

            try
            {
                return new ChromeDriver(driverService, options);
            }
            catch(Exception e)
            {                
                Log(e.Message);
                Log("Default Chrome Driver");
                return new ChromeDriver();
            }
        }

        private IWebDriver FirefoxProvider()
        {
            string FirefoxBinaries = Path.Combine(AppContext.BaseDirectory, $"FirefoxBinaries");
            string FirefoxBinary = Path.Combine(FirefoxBinaries, "firefox\\firefox.exe");
            string GeckoDriverPath = Path.Combine(FirefoxBinaries, "geckodriver");

            FirefoxOptions options = new();
            options.AddArguments("--headless");
            options.AddArguments("-safe-mode");
            options.AddArguments("-private");
            options.BinaryLocation=FirefoxBinary;

            FirefoxDriverService driverService = FirefoxDriverService.CreateDefaultService(GeckoDriverPath);
            driverService.HideCommandPromptWindow = true;

            try
            {
                return new FirefoxDriver(driverService, options);
            }
            catch (Exception e)
            {
                Log(e.Message);
                Log("Default Gecko Driver");
                return new FirefoxDriver();
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

    internal static class Extenders
    {
        public static string ToPath(this ChromiumVersions chromiumVersion)
        {
            if (chromiumVersion == ChromiumVersions.Default) return "";
            return $"\\{(int)chromiumVersion}.0";
        }
    }
}
