﻿using DailyCheck.PageObjects;
using DailyCheck.View;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static DailyCheck.PageObjects.LoginPageObject;

namespace DailyCheck
{
    public partial class RewardWindow : Window
    {
        private DriverProvider? _driverProvider;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private string login;
        private string password;

        public RewardWindow(string login, string password)
        {
            InitializeComponent();
            Loaded += OnLoaded;
            this.login = login;
            this.password = password;
            _driverProvider = null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            
            var TaskReward = GetReward();
            var TaskEvent = UpdateEventProgress(taskBefore: TaskReward);
            ValidateSettings();

            Task.Run(() =>
            {
                TaskEvent.Wait();
                _driverProvider?.Dispose();
            });

            ValidateSettings();
        }

        private async Task GetReward()
        {
            SynchronizationContext? syncContext = SynchronizationContext.Current;  // Possible NULL !!!
            ConsoleAP.Add("Инициализация Web-драйвера...",
                          "Инициализация Web-драйвера",
                          "Ошибка инициализации Web-драйвера",
                          AccessingElement.StateEnum.Loading);

            _driverProvider ??= new DriverProvider();

            if (_driverProvider == null)
            {
                ConsoleAP.LastFailure();
                return;
            }
            
            IReadOnlyDictionary<string, string> Attributes = new Dictionary<string, string>();
            bool isDone = false;           

            isDone = await Task.Run(() =>
            {
                var lpo = new LoginPageObject(_driverProvider.Driver, syncContext, ConsoleAP);
                bool isSignedIn = lpo.SignInAndGetReward(login, password);
                Attributes = lpo.Attributes;
                return isSignedIn;
            });


            if (isDone && Attributes.TryGetValue("Type", out var rewardType))
            {
                string rt = rewardType.ToString();
                if (rt != RewardType.None.ToString())
                    await ShowReward(Attributes);
            }
            else if (!isDone)
            {
                // Show Retry button
                var refresh = new AccessingButton();
                refresh.Click += (s, e) =>
                    {
                        ConsoleAP.ElementsPanel.Children.Remove(refresh);
                        _ = GetReward();
                    };
                ConsoleAP.ElementsPanel.Children.Add(refresh);
            }
        }

        private async Task UpdateEventProgress(Task taskBefore)
        {
            await taskBefore;
            SynchronizationContext? syncContext = SynchronizationContext.Current;  // Possible NULL !!!

            _driverProvider ??= new DriverProvider();

            await Task.Run(() =>
            {
                var webEvent = new WebEventJune2024PageObject(_driverProvider.Driver, syncContext, WebEventAccessingElement);
                webEvent.RunUpdater(delayInSeconds: 15).Wait();
            });

        }

        private async Task ShowReward(IReadOnlyDictionary<string, string> Attributes)
        {
            var token = _cancellationTokenSource.Token;

            await Task.Delay(500, token); // initial delay

            foreach (var x in new DescendingSQRT(200))
            {
                ConsoleAP.Opacity = x;
                await Task.Delay(8, token);
            }

            RewardView
                .SetBackGroundImage(Attributes.GetValueOrDefault("Background", ""))
                .SetContentImage(Attributes.GetValueOrDefault("Image", ""))
                .SetContentDescription(Attributes.GetValueOrDefault("Description", ""));
            ConsoleAP.Visibility = Visibility.Collapsed;
            RewardViewBorder.Opacity = 0.0;
            RewardViewBorder.Visibility = Visibility.Visible;

            Attributes.TryGetValue("Type", out var rewardType);
            if (rewardType == RewardType.New.ToString())
                RewardView.ToolTip = new ToolTip()
                {
                    Content = "Новая награда! Зайди в игру, чтобы получить её",
                };
            else if (rewardType == RewardType.Old.ToString())
                RewardView.ToolTip = new ToolTip()
                {
                    Content = "Награда уже получена",
                };

            foreach (var x in new AscendingSQRT(200))
            {
                RewardViewBorder.Opacity = x;
                await Task.Delay(8, token);
            }
        }
        private void ButtonCloseClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => Close();
        private void ButtonMinimizeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            _cancellationTokenSource.Cancel();
            _driverProvider?.Dispose();

            /*
            var children = new List<Process>();
            var mos = new ManagementObjectSearcher(String.Format($"Select * From Win32_Process Where ParentProcessID={process.Id}"));

            foreach (ManagementObject mo in mos.Get())
                Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
            */
            Debug.WriteLine("RewardWindow says goodbye");
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsDialog();
            bool? result = dialog.ShowDialog();
            ValidateSettings();
        }

        private void ValidateSettings()
        {
            string fileName = ((App)Application.Current).SettingsFile.Content;

            if (fileName == "" || File.Exists(fileName))
            {
                WrongSettings.Visibility = Visibility.Hidden;
            }
            else
            {
                WrongSettings.Visibility = Visibility.Visible;
            }
        }
    }
}
