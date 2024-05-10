using DailyCheck.View;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Net.Http;
using System.Windows;
using static DailyCheck.DebugLogger;

namespace DailyCheck.PageObjects
{
    public class WebEventMay2024PageObject(IWebDriver driver, SynchronizationContext? uiContext, AccessingElement uiElement) : BasePageObject(driver)
    {

        private static readonly string eventURL = @"https://tanki.su/ru/web-event-may2024/";
        private readonly SynchronizationContext? UiContext = uiContext;
        private readonly AccessingElement uiElement = uiElement;

        private static readonly By taskNotAcceptedButton = By.XPath("//button[@class='Button_button__G7972 Button_button_disable__EblZF']");
        private static readonly By taskAcceptedButton = By.XPath("//button[@class='Button_button__G7972 Button_button_claimed__hjXRG']");
        private static readonly By taskCompletedButton = By.XPath("//button[@class='Button_button__G7972 Button_button_completed__eJppi']");
        private static readonly By taskTakeNewButton = By.XPath("//button[@class='Button_button__G7972 ']");

        public async Task RunUpdater(int delayInSeconds)
        {
            int delayMilliseconds = delayInSeconds * 1000;
            Log("RunUpdater WebEvent");
            while (true)
            {
                NotifyUI("Проверка", "", "", AccessingElement.StateEnum.Loading);

                int responce = CheckUrl(eventURL).Result;       // 0 - exception (no internet or god knows what else)
                if (responce >= 200 && responce <= 299) break;  // Ok
                if (responce == 404) {                          // Page not found
                    NotifyUI("", "", "завершено", AccessingElement.StateEnum.Failure);
                    SetHint("Страница события не существует. Вероятно ивент уже завершился.");
                    return;
                }
                
                Log("Страница события недоступна");                
                NotifyUI("", "", "ошибка", AccessingElement.StateEnum.Failure);
                SetHint($"Страница события недоступна. Код ошибки: {responce}");
                await Task.Delay(delayMilliseconds);
            }
            RemoveHint();
            Driver.Navigate().GoToUrl(eventURL);
            Log("Открываем страницу события");
            NotifyUI("Загрузка", "", "", AccessingElement.StateEnum.Loading);
            while (true)
            {
                try
                {         
                    var remains0 = Driver.FindElements(taskNotAcceptedButton);
                    var claimed0 = Driver.FindElements(taskAcceptedButton);
                    var completed0 = Driver.FindElements(taskCompletedButton);
                    var available0 = Driver.FindElements(taskTakeNewButton);
                    var remains = remains0.Count();
                    var claimed = claimed0.Count();
                    var completed = completed0.Count();
                    var available = available0.Count();
                    remains0 = null;
                    claimed0 = null;
                    completed0 = null;
                    available0 = null;
                    if (available > 0) {
                        ClickJS(WaitEnabled(taskTakeNewButton));
                        NotifyUI("", $"Новая задача", "", AccessingElement.StateEnum.Success);
                        SetHint($"Получена {completed + 1}-я задача");
                        Task.Delay(5 * 60 * 1000).Wait();       // extra delay; we just picked a new task
                    }
                    if (remains == 0 && completed == 15)
                    {
                        NotifyUI("", $"Выполнено", "", AccessingElement.StateEnum.Success);
                        SetHint("Все задачи выполнены");
                        return;
                    }
                    NotifyUI("", $"{completed + claimed} из 15", "", AccessingElement.StateEnum.Success);
                    SetHint($"Выполнено: {completed}, осталось: {remains + claimed}, выполняется: {claimed}, доступно: {available}");
                    if (completed == 14 && claimed == 1) {
                            Task.Delay(5 * 60 * 1000).Wait();    // extra delay; we don't need to pick a new task anymore
                    }
                    Task.Delay(delayMilliseconds).Wait();
                    NotifyUI("Обновляю", "", "", AccessingElement.StateEnum.Loading);
                    //Driver.Navigate().Refresh();
                    RefreshJS();
                   // Driver.Navigate().GoToUrl(eventURL);
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

        private void SetHint(string hint )
        {
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    uiElement.ToolTip = "Событие 'Крымская операция': " + hint;
                }),
                null
                );
        }

        private void RemoveHint()
        {
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    uiElement.ToolTip = null;
                }),
                null
                );
        }


    }
}
