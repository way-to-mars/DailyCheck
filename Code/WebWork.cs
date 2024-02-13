using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.IO;
using static DailyCheck.DebugLogger;

namespace DailyCheck
{
    internal class WebWork(string login, string password) : IDisposable
    {
        private readonly ChromeDriver driver;
        private readonly string startUrl = @"https://tanki.su/auth/oid/new/?next=/ru/daily-check-in/";
        private readonly string browserDataDir = Path.Combine(AppContext.BaseDirectory, $"browserdata{DateTime.Now.Ticks:X}");
        private readonly By loginInput = By.Id("id_login");
        private readonly By passwordInput = By.Id("id_password");
        private readonly By submitButton = By.XPath("//button[@type='submit']");
        private readonly By playerNameText = By.ClassName("cm-user-menu-link_cutted-text");

        protected ConcurrentHashSet<Task> disposables = new();

        public async Task<string> GetPlayerName()
        {
            Log("GetPlayerName Started");

            string result = "";
            Task task;

            using (IWebDriver webDriver = GetDriver())
            {
                Log("GetPlayerName WebDriver received");
                task = Task.Run(() =>
                {
                    try { result = Login(webDriver); Log($"GetPlayerName. result = {result}"); }
                    catch (Exception ex) { Log($"GetPlayerName got exception:\n{ex}"); }
                });
                disposables.Add(task);
                await task;
            }
            Log("GetPlayerName WebDriver disposed");
            
            return result;
        }

        public void StopTasks()
        {
            foreach (Task task in disposables.ToHashSet())
            {
                Log($"task.id = {task.Id}");
                task.Dispose();
            }
        }

        private string Login(IWebDriver webDriver)
        {
            IWebElement FE(By by) => webDriver.FindElement(by);

            try
            {
                webDriver.Navigate().GoToUrl(startUrl);

                FE(loginInput).SendKeys(login);
                FE(passwordInput).SendKeys(password);
                FE(submitButton).Click();

                if (webDriver.NoSuchElement(playerNameText))
                {
                    if (webDriver.Url.StartsWith("https://lesta.ru/id/signin/"))
                    {
                        Log($"url = {webDriver.Url}");
                        Log("Неверный email или пароль.");
                        return "";
                    }
                    else
                    {
                        Log($"url = {webDriver.Url}");
                        Log("Другая ошибка");
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Login exception: {ex}");
            }

            string name;
            try { name = FE(playerNameText).GetAttribute("textContent"); }
            catch { name = ""; }
            return name;
        }

        private ChromeDriver GetDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments($"user-data-dir={browserDataDir}");
            options.AddArguments("headless");
            options.AddArguments("incognito");

            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            ChromeDriver webDriver = new ChromeDriver(driverService, options);
            webDriver.Manage().Window.Maximize();
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            return webDriver;
        }

        public void Dispose()
        {
            Log("Dispose WebWork");
            foreach (var d in disposables.ToHashSet())
                try
                {
                    Log($"Disposing {d}");
                    d?.Dispose();
                }
                catch { }
            DeleteBrowserData();
        }

        private void DeleteBrowserData()
        {
            do
            {
                try
                {
                    Log($"Deleting directory [{browserDataDir}] using Directory.Delete");
                    Directory.Delete(browserDataDir, true);
                }
                catch
                {
                    Log($"Deleting directory [{browserDataDir}] using CMD.exe");
                    Process.Start("CMD.exe", $"/C RD /S /Q {browserDataDir}").WaitForExit();
                   // await Task.Delay(10);
                }
            } while (Directory.Exists(browserDataDir));
            Log($"Directory [{browserDataDir}] is deleted ✓");
        }
    }

    public static class WebDriverExtensions
    {
        public static bool NoSuchElement(this IWebDriver driver, By by) => !driver.HasElement(by);

        public static bool HasElement(this IWebDriver driver, By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch { return false; }
        }
    }
}
