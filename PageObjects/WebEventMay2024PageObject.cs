using DailyCheck.View;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Net.Http;
using System.Windows;
using static DailyCheck.DebugLogger;

namespace DailyCheck.PageObjects
{
    public class WebEventMay2024PageObject(IWebDriver driver, SynchronizationContext? uiContext, AccessingElement uiElement) : BasePageObject(driver)
    {
        private static readonly By loginInput = By.Id("id_login");
        private static readonly By passwordInput = By.Id("id_password");
        private static readonly By submitButton = By.XPath("//button[@type='submit']");
        //  private static readonly By submitButton = By.XPath("//span[@class='jsc-submit-button']");
        private static readonly By authenticationError = By.XPath("//p[@class='js-form-errors-content']");
        private static readonly By playerNameText = By.ClassName("cm-user-menu-link_cutted-text");
        private static readonly By todayItem = By.XPath("//div[@class='c_item c_default']");
        private static readonly By completedItems = By.XPath("//div[@class='c_item c_comlete']");
        private static readonly By itemBackground = By.XPath("./div[@class='c_item__bg']/img");
        private static readonly By itemContent = By.XPath(".//img[@class='c_item__icon']");
        private static readonly By itemTextSpan = By.XPath(".//span[@class='c_item__text']");
        private static readonly string eventURL = @"https://tanki.su/ru/web-event-may2024/";
        private readonly SynchronizationContext? UiContext = uiContext;
        private readonly AccessingElement uiElement = uiElement;

        private static readonly By taskNotAcceptedButton = By.XPath("//button[@class='Button_button__G7972 Button_button_disable__EblZF']");
        private static readonly By taskAcceptedButton = By.XPath("//button[@class='Button_button__G7972 Button_button_claimed__hjXRG']");

        public async Task Start(int delayInSeconds)
        {
            int _delay = delayInSeconds * 1000;
            Log("Start WebEvent");
            while (true)
            {
                NotifyUI("Проверка", "", "", AccessingElement.StateEnum.Loading);
                if (CheckUrl(eventURL).Result) break;
                Log("Страница события недоступна");                
                NotifyUI("", "", "ошибка", AccessingElement.StateEnum.Failure);
                await Task.Delay(_delay);
            }
            Driver.Navigate().GoToUrl(eventURL);
            Log("Открываем страницу события");
            NotifyUI("Загрузка", "", "", AccessingElement.StateEnum.Loading);
            while (true)
            {
                try
                {
                    Task.Delay(_delay).Wait();
                    NotifyUI("Обновляю", "", "", AccessingElement.StateEnum.Loading);
                    Driver.Navigate().Refresh();
                    var remains = Driver.FindElements(taskNotAcceptedButton);
                    var claimed = Driver.FindElements(taskAcceptedButton);
                    NotifyUI("", $"{claimed.Count()}/{remains.Count()}", "", AccessingElement.StateEnum.Success);
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    NotifyUI("", "", "ошибка", AccessingElement.StateEnum.Failure);
                }
            }
        }

        private void NotifyUI(string textLoading, string textSuccess, string textFailure, AccessingElement.StateEnum state)
        {
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    uiElement.TextLoading = textLoading;
                    uiElement.TextSuccess = textSuccess;
                    uiElement.TextFailure = textFailure;
                    uiElement.State = state;
                }),
                null
                );
        }

        private static async Task<bool> CheckUrl(string url)
        {
            HttpClient client = new() { Timeout = TimeSpan.FromSeconds(20) };
            try
            {
                var checkingResponse = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                Log($"GetAsync is finished for {url} with {checkingResponse.IsSuccessStatusCode}");
                if (checkingResponse.IsSuccessStatusCode)
                    return true;
            }
            catch { Log($"GetAsync has failed on {url}"); }
            return false;
        }


    }
}
