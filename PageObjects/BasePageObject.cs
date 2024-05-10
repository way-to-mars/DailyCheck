using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Net.Http;
using static DailyCheck.DebugLogger;

namespace DailyCheck.PageObjects
{
    public class BasePageObject(IWebDriver webDriver)
    {
        protected TimeSpan DefaultTimeout { get; } = TimeSpan.FromSeconds(10);
        protected IWebDriver Driver { get; } = webDriver;
        protected IWebElement? WaitEnabled(By by) => WaitEnabled(by, DefaultTimeout);
        protected IWebElement? WaitEnabled(By by, TimeSpan timeOut)
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
        protected IWebElement? FirstOfTwo(By by1, By by2, out int resultNumber) => FirstOfTwo(by1, by2, DefaultTimeout, out resultNumber);
        protected IWebElement? FirstOfTwo(By by1, By by2, TimeSpan timeOut, out int resultNumber)
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

        protected void ClickJS(IWebElement? button)
        {
            if (button != null)
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
                js.ExecuteScript("arguments[0].click();", button);
            }
        }

        protected void RefreshJS()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("location.reload()");
        }

        protected static async Task<int> CheckUrl(string url)
        {
            HttpClient client = new() { Timeout = TimeSpan.FromSeconds(20) };
            try
            {
                var checkingResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                return (int)checkingResponse.StatusCode;
            }
            catch
            {
                Log($"GetAsync has failed on {url}");
                return 0;
            }
        }

        /// <summary>
        /// Checks your internet connection.
        /// Checks the list of reliable sites
        /// </summary>
        /// <returns>A tuple of integers. First item is the count of availble sites, Second - total count of sites checked</returns>
        protected static (int, int) SitesAble()
        {
            List<string> CheckList = [
                "https://ya.ru",
                "https://mail.ru",
                "https://rambler.ru",
                "https://gismeteo.ru",
                "https://habr.ru",
                "https://google.com",
                ];
            var tasks = CheckList.Select(CheckUrl);
            Task.WhenAll(tasks).Wait();
            int sitesChecked = CheckList.Count;
            int sitesAble = tasks.Where(x => x.Result.IsSuccessHttpResponse()).Count();
            return (sitesAble, CheckList.Count());
        }
    }

    public static class Extensions
    {
        public static string TextContent(this IWebElement? element) =>
            (element is null) ? string.Empty : element.GetAttribute("textContent");

        public static bool IsSuccessHttpResponse(this int response) { 
            return response >= 200 && response <= 299;
        }
    }


}
