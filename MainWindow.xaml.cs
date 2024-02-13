using DailyCheck.PageObjects;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static DailyCheck.DebugLogger;

namespace DailyCheck
{
    public partial class MainWindow : Window
    {
        private DriverProvider? _driverProvider = null;

        public MainWindow()
        {
            InitializeComponent();
            var uid = MachineId.Get();
        }

        private void ButtonCloseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Hide();
            _driverProvider?.Dispose();
            Close();
        }

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
        private void Grid_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                DragMove();
            }
        }
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBoxHint.Visibility = (PasswordBox1.Password.Length > 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void ButtonSubmit(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox1.Password;

            if (login.Length == 0 || !login.IsValidEmailAddress())
            {
                LoginBox.Focus();

                TextBlock popupText = new TextBlock();
                popupText.Text = "Неверный email или пароль.";
                popupText.Padding = new Thickness(10,2,10,2);
                popupText.Background = Brushes.Wheat;
                popupText.Foreground = Brushes.Red;

                Popup popup = new Popup();

                popup.Child = popupText;

                popup.Placement = PlacementMode.Center;
                popup.PlacementTarget = ImageGrid;
               // popup.HorizontalAlignment = HorizontalAlignment.Center;
               // popup.VerticalAlignment = VerticalAlignment.Center;
                popup.StaysOpen = false;
                popup.IsOpen = true;

                return;
            }


            Log("Button_Submit >> Start");

            LoginForm.Visibility = Visibility.Collapsed;
            UpdateLayout();
            await Task.Delay(1);

            CancellationTokenSource tokenSource = new();
            _ = ImageAnimation(tokenSource.Token);

            var sw = new Stopwatch();
            sw.Start();

            string playerName = "";
            using (DriverProvider driverProvider = new())
            {
                _driverProvider = driverProvider;
                await Task.Run(() =>
                {
                    playerName = new LoginPageObject(driverProvider.Driver)
                                        .SignIn(login, password);
                });

                sw.Stop();
                Log($"Button_Submit >> Login received in {sw.ElapsedMilliseconds} milliseconds");
                tokenSource.Cancel();

                if (playerName.Length == 0)
                {
                    Log("Can't get player name");
                    LoginForm.Visibility = Visibility.Visible;
                }
                else
                {
                    Log($"Player is {playerName}");
                    SubmitButton.Content = playerName;
                    LoginForm.Visibility = Visibility.Visible;                    
                }
                _driverProvider = null;
            }
        }

        private async Task ImageAnimation(CancellationToken token)
        {
            double fun(int x)
            {
                double arg = x;
                double const1 = 50;
                double const2 = 300;
                return (1 - Math.Cos(arg / const1)) * const2 / (const2 + arg);
            }

            var originalHeight = MainGrid.RowDefinitions[1].ActualHeight;
            MainGrid.RowDefinitions[1].Height = new GridLength(330);
            ImageGrid.VerticalAlignment = VerticalAlignment.Top;
            int indexer = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    indexer++;
                    ImageGrid.Margin = new Thickness(0, 200 * fun(indexer), 0, 0);
                    await Task.Delay(10, token);
                    Log(ImageGrid.Margin.ToString());
                }
                catch { /* Do nothing */ }
            }

            MainGrid.RowDefinitions[1].Height = new GridLength(originalHeight);
            ImageGrid.Margin = new Thickness(0);
            ImageGrid.VerticalAlignment = VerticalAlignment.Center;

            return;
        }
    }
}