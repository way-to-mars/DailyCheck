using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DailyCheck.PageObjects
{
    public class BasePageObject(IWebDriver webDriver)
    {
        public TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);
        public IWebDriver Driver { get; } = webDriver;
        public IWebElement? WaitEnabled(By by) => WaitEnabled(by, DefaultTimeout);
        public IWebElement? WaitEnabled(By by, TimeSpan timeOut)
        {
            IWebElement? result = null;
            try
            {
                new WebDriverWait(Driver, timeOut)
                    .Until(condition =>
                    {
                        try
                        {
                            var webElement = Driver.FindElement(by);
                            if (webElement.Enabled) result = webElement;
                            return webElement.Enabled & webElement.Displayed;
                        }
                        catch { return false; }
                    });
            }
            catch { /* Do nothing */ }
            return result;
        }
        public IWebElement? FirstOfTwo(By by1, By by2, out int resultNumber) => FirstOfTwo(by1, by2, DefaultTimeout, out resultNumber);
        public IWebElement? FirstOfTwo(By by1, By by2, TimeSpan timeOut, out int resultNumber)
        {
            IWebElement? resultElement = null;
            int _resultNumber = 0;
            try
            {
                new WebDriverWait(Driver, timeOut)
                    .Until(condition =>
                    {
                        try
                        {
                            resultElement = Driver.FindElement(by1);
                            _resultNumber = 1;
                            return true;
                        }
                        catch { /* Do nothing */ }
                        try
                        {
                            resultElement = Driver.FindElement(by2);
                            _resultNumber = 2;
                            return true;
                        }
                        catch { /* Do nothing */ }
                        return false;
                    });
            }
            catch { /* Do nothing */ }

            resultNumber = _resultNumber;
            return resultElement;
        }

        public void ClickJS(IWebElement? button) {
            if (button != null)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("arguments[0].click();", button);
            }
        }
    }

    public static class WebElementExtensions
    {
        public static string TextContent(this IWebElement? element) =>
            (element is null) ? string.Empty : element.GetAttribute("textContent");
    }
}
