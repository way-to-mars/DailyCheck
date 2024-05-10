using DailyCheck.View;
using OpenQA.Selenium;
using System.Net.Http;
using static DailyCheck.DebugLogger;

namespace DailyCheck.PageObjects
{
    public class LoginPageObject(IWebDriver driver, SynchronizationContext? uiContext, ConsolePanel? consolePanel) : BasePageObject(driver)
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
        private static readonly string startUrl = @"https://tanki.su/auth/oid/new/?next=/ru/daily-check-in/";
        private static readonly string tankiDomen = @"https://tanki.su/";
        private readonly SynchronizationContext? UiContext = uiContext;
        private readonly ConsolePanel? ConsolePanel = consolePanel;
        public IReadOnlyDictionary<string, string> Attributes { get => new Dictionary<string, string>(_attributes); }
        public enum RewardType
        {
            None,
            New,
            Old,
        }

        private readonly Dictionary<string, string> _attributes = new()
        {
            {"PlayerName", ""},
            {"Background", ""},
            {"Image", ""},
            {"Description", ""},
            {"Type", RewardType.None.ToString() }
        };

        public async Task<bool> SignInAndGetReward(string login, string password)
        {
            await Task.Run(() => { }); // Empty action

            if (!EstablishConnection()) return false;

            if (TryToSignIn(login, password)) return GetReward();

            return false;
        }

        public async Task<bool> SignInAndGetName(string login, string password, Action<string> notificate)
        {
            await Task.Run(() => { }); // Empty action

            try
            {
                notificate("Загружаем сайт авторизации...");
                Driver.Navigate().GoToUrl(startUrl);
                await Task.Delay(1000);
            }
            catch
            {
                if (SitesAble().Item1 == 0)
                {
                    _attributes["Error"] = "Нет доступа к интернету";
                    return false;
                }

                var accessDomen = CheckUrl(tankiDomen);
                var accessStartUrl = CheckUrl(startUrl);
                if (!accessDomen.Result.IsSuccessHttpResponse())
                    _attributes["Error"] = "Сайт tanki.su не доступен";
                else if (!accessStartUrl.Result.IsSuccessHttpResponse())
                    _attributes["Error"] = "Сайт tanki.su доступен, но недоступна страница авторизации";
                else
                    _attributes["Error"] = "Интернет перегружен или нестабилен";
                return false;
            }

            try
            {
                notificate("Логинимся...");
                TimeSpan timeOut = TimeSpan.FromSeconds(30);
                List<Task<IWebElement?>> tasks =
                    [
                        Task.Run(async () => { await Task.Delay(10); return WaitEnabled(loginInput, timeOut); }),
                        Task.Run(async () => { await Task.Delay(10); return WaitEnabled(passwordInput, timeOut); }),
                        Task.Run(async () => { await Task.Delay(10); return WaitEnabled(submitButton, timeOut); }),
                    ];
                Task.WhenAll(tasks).Wait();

                if (tasks.Where(t => t.Result is null).Any())  // Check if there any nulls in results
                {
                    _attributes["Error"] = "Не удалось загрузить интерфейс входа в учетную запись";
                    return false;
                }

                tasks[0].Result?.SendKeys(login);
                tasks[1].Result?.SendKeys(password);
                ClickJS(tasks[2].Result);
            }
            catch (Exception ex)
            {
                _attributes["Error"] = $"Непредвиденная ошибка: {ex.Message}";
                return false;
            }

            IWebElement? clickResult = FirstOfTwo(playerNameText, authenticationError, TimeSpan.FromSeconds(30), out int resNumber);

            switch (resNumber)
            {
                case 0: _attributes["Error"] = "Не удалось залогиниться"; return false;
                case 1: break;
                case 2: _attributes["Error"] = "Неверный email или пароль"; return false;
                default: throw new InvalidOperationException("Unreachable code reached");
            }

            try
            {
                string? playerName = clickResult?.TextContent() ?? null;
                if (playerName != null)
                    _attributes["PlayerName"] = (string)playerName;
                else
                    notificate("Не удалось получить имя игрока");
            }
            catch { }

            return true;
        }

        private bool EstablishConnection()
        {
            try
            {
                ConsolePanelAdd("Загружаем сайт авторизации...",
                                "Загружаем сайт авторизации",
                                $"Не удалось загрузить страницу авторизации",
                                AccessingElement.StateEnum.Loading);
                Driver.Navigate().GoToUrl(startUrl);
                ConsolePanelLastDone();
                Log($"url = {Driver.Url}");
                return true;
            }
            catch
            {
                ConsolePanelLastFailure();
                ConsolePanelAdd("Проверка подключения к интернету...", "", "", AccessingElement.StateEnum.Loading);

                var (sitesAble, sitesChecked) = SitesAble();

                if (sitesAble == 0)
                {
                    ConsolePanelLastFailure("Нет доступа к интернету");
                    return false;
                }
                else
                {
                    ConsolePanelLastDone($"Есть доступ к интернету [{sitesAble} из {sitesChecked}]");
                }

                ConsolePanelAdd("Проверка доступа к tanki.su...", "", "", AccessingElement.StateEnum.Loading);

                var accessDomen = CheckUrl(tankiDomen);
                var accessStartUrl = CheckUrl(startUrl);

                if (!accessDomen.Result.IsSuccessHttpResponse())
                {
                    ConsolePanelLastFailure("Сайт tanki.su не доступен");
                    return false;
                }

                if (!accessStartUrl.Result.IsSuccessHttpResponse())
                {
                    ConsolePanelLastFailure("Сайт tanki.su доступен, но недоступна страница авторизации");
                    return false;
                }

                ConsolePanelLastDone("Интернет перегружен или нестабилен");
                return false;
            }
        }
        private bool TryToSignIn(string login, string password)
        {
            try
            {
                ConsolePanelAdd("Логинимся...", "Логинимся", "Не удалось залогиниться", AccessingElement.StateEnum.Loading);
                TimeSpan timeOut = TimeSpan.FromSeconds(30);
                List<Task<IWebElement?>> tasks =
                    [
                        Task.Run(async () => { await Task.Delay(10); return WaitEnabled(loginInput, timeOut); }),
                        Task.Run(async () => { await Task.Delay(10); return WaitEnabled(passwordInput, timeOut); }),
                        Task.Run(async () => { await Task.Delay(10); return WaitEnabled(submitButton, timeOut); }),
                    ];
                Task.WhenAll(tasks).Wait();

                if (tasks.Where(t => t.Result is null).Any())  // Check if there any null in results
                {
                    ConsolePanelLastFailure();
                    Notification("Не удалось загрузить интерфейс входа в учетную запись");
                    return false;
                }

                tasks[0].Result?.SendKeys(login);
                tasks[1].Result?.SendKeys(password);

                ClickJS(tasks[2].Result);
            }
            catch (Exception ex)
            {
                ConsolePanelLastFailure();
                Notification($"Непредвиденная ошибка: {ex.Message}");
                return false;
            }

            IWebElement? clickResult = FirstOfTwo(playerNameText, authenticationError, TimeSpan.FromSeconds(30), out int resNumber);

            switch (resNumber)
            {
                case 0: ConsolePanelLastFailure(); return false;
                case 1: ConsolePanelLastDone(); break;
                case 2: ConsolePanelLastFailure("Неверный email или пароль"); return false;
                default: throw new InvalidOperationException("Unreachable code reached");
            }

            try
            {
                string? playerName = clickResult?.TextContent() ?? null;
                if (playerName != null)
                {
                    Notification($"Имя игрока: {playerName}");
                    _attributes["PlayerName"] = playerName;
                }
                else
                {
                    Notification($"Не удалось получить имя игрока");
                }
            }
            catch { }

            return true;
        }
        private bool GetReward()
        {
            ConsolePanelAdd("Ищем награду....", "Награда найдена!", "Награда недоступна или получена ранее", AccessingElement.StateEnum.Loading);

            var rewardItem = WaitEnabled(todayItem, TimeSpan.FromSeconds(5));
            if (rewardItem is not null)
            {
                ConsolePanelLastDone();
                try
                {
                    _attributes["Background"] = rewardItem.FindElement(itemBackground)?.GetAttribute("src") ?? ""; ;
                    _attributes["Image"] = rewardItem.FindElement(itemContent)?.GetAttribute("src") ?? "";
                    _attributes["Description"] = rewardItem.FindElement(itemTextSpan)?.TextContent() ?? "";
                    _attributes["Type"] = RewardType.New.ToString();
                }
                catch
                {
                    Notification("Не удалось загрузить информацию о награде");
                }

                try
                {
                    string classBefore = rewardItem.GetAttribute("class"); // c_item c_default

                    rewardItem.Click();

                    // TODO
                    // Wait somehow until item is changed
                    Task.Delay(500).Wait();
                    // TO DO !!!
                    // Make it meaningful

                    string classAfter = rewardItem.GetAttribute("class"); // c_item c_comlete    

                    if (classAfter != classBefore) Notification("Награда получена");
                    else Notification($"Награда вероятно не получена. class_after={classAfter}");

                    return true;
                }
                catch
                {
                    Notification("Непредвиденная ошибка");
                    return false;
                }
            }
            else
            {
                ConsolePanelLastFailure();
                try
                {
                    var completedCollection = Driver.FindElements(by: completedItems);
                    if (completedCollection != null && completedCollection.Count > 0)
                    {
                        var lastItem = completedCollection.Last();

                        _attributes["Background"] = lastItem.FindElement(itemBackground)?.GetAttribute("src") ?? "";
                        _attributes["Image"] = lastItem.FindElement(itemContent)?.GetAttribute("src") ?? "";
                        _attributes["Description"] = lastItem.FindElement(itemTextSpan)?.TextContent() ?? "";
                        _attributes["Type"] = RewardType.Old.ToString();
                    }
                }
                catch
                {
                    Notification("Не удалось загрузить информацию о последней полученной награде");
                }
            }
            return true;
        }
        private AccessingElement? ConsolePanelAdd(string textLoading, string textSuccess, string textFailure, AccessingElement.StateEnum state)
        {
            AccessingElement? result = null;
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    result = ConsolePanel?.Add(textLoading, textSuccess, textFailure, state);
                }),
                null
                );
            return result;
        }
        private void ConsolePanelLastDone(string? successMessage = null)
        {
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    ConsolePanel?.LastDone(message: successMessage);
                }),
                null
                );
        }
        private void ConsolePanelLastFailure(string? errorMessage = null)
        {
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    ConsolePanel?.LastFailure(message: errorMessage);
                }),
                null
                );
        }
        private void Notification(string message)
        {
            UiContext?.Send(
                new SendOrPostCallback((s) =>
                {
                    ConsolePanel?.Notification(message);
                }),
                null
                );
        }

    }
}
