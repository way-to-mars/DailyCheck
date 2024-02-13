using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using static DailyCheck.DebugLogger;

namespace DailyCheck.PageObjects
{
    public class LoginPageObject
    {
        private readonly By loginInput = By.Id("id_login");
        private readonly By passwordInput = By.Id("id_password");
        private readonly By submitButton = By.XPath("//button[@type='submit']");
        private readonly By playerNameText = By.ClassName("cm-user-menu-link_cutted-text");
        private readonly string startUrl = @"https://tanki.su/auth/oid/new/?next=/ru/daily-check-in/";

        private IWebDriver _driver;

        public LoginPageObject(IWebDriver driver)
        {
            _driver = driver;
        }

        public string SignIn(string login, string password)
        {
            IWebElement FE(By by) => _driver.FindElement(by);

            try
            {
                _driver.Navigate().GoToUrl(startUrl);

                FE(loginInput).SendKeys(login);
                FE(passwordInput).SendKeys(password);
                FE(submitButton).Click();

                if (_driver.NoSuchElement(playerNameText))
                {
                    if (_driver.Url.StartsWith("https://lesta.ru/id/signin/"))
                    {
                        Log($"url = {_driver.Url}");
                        Log("Неверный email или пароль.");
                        return "";
                    }
                    else
                    {
                        Log($"url = {_driver.Url}");
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
    }
}
