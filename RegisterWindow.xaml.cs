using DailyCheck.PageObjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using IWshRuntimeLibrary;
using System.IO;
using System.Diagnostics;
using Windows.ApplicationModel.VoiceCommands;

namespace DailyCheck
{
    public partial class RegisterWindow : Window
    {
        private DriverProvider? _driverProvider = null;
        const string ExecutableName = "DailyCheck.exe";

        public RegisterWindow()
        {
            InitializeComponent();
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

            if (login.Length == 0)
            {
                LoginBox.Focus();
                return;
            }
                                
            if (!login.IsValidEmailAddress())
            {
                LoginBox.Focus();

                TextBlock popupText = new()
                {
                    Text = "Неверный формат email",
                    Padding = new Thickness(10, 2, 10, 2),
                    Background = Brushes.Wheat,
                    Foreground = Brushes.Red
                    
                };

                Popup popup = new()
                {
                    Child = popupText,
                    Placement = PlacementMode.Center,
                    PlacementTarget = LoginBox,
                    StaysOpen = false,
                    IsOpen = true
                };

                return;
            }

            if (password.Length == 0)
            {
                PasswordBox1.Focus();
                return;
            }

            LoginForm.Visibility = Visibility.Collapsed;
            UpdateLayout();
            await Task.Delay(1);

            CancellationTokenSource tokenSource = new();
            _ = ImageAnimation(tokenSource.Token);

            SynchronizationContext syncContext = SynchronizationContext.Current!;  // Possible NULL !!!

            bool isLogged = false;
            IReadOnlyDictionary<string, string> Attributes = new Dictionary<string, string>();
            using (DriverProvider driverProvider = new())
            {
                _driverProvider = driverProvider;
                isLogged = await Task.Run(() =>
                {
                    var lpo = new LoginPageObject(driverProvider.Driver, syncContext, null);
                    bool isSignedIn = lpo.SignInAndGetName(login, password,
                                            (msg) => { syncContext?.Send(new SendOrPostCallback((s) => { NotificationHolder.Text = msg; }), null); }
                                            ).Result;
                    Attributes = lpo.Attributes;
                    return isSignedIn;
                });

                
                tokenSource.Cancel();
                _driverProvider = null;
            }
            if (!isLogged)
            {
                NotificationHolder.Text = Attributes["Error"];
                LoginForm.Visibility = Visibility.Visible;
            }
            else
            {
                _ = SayHello(Attributes["PlayerName"]);
                PlayerNameHolder.Text = Attributes["PlayerName"];
                SaveForm.Visibility = Visibility.Visible;
            }
        }

        private async Task ImageAnimation(CancellationToken token)
        {
            static double fun(int x)
            {
                double arg = x;
                double const1 = 50;
                double const2 = 220;
                return (1 - Math.Cos(arg / const1)) * const2 / (const2 + arg);
            }

            var originalHeight = MainGrid.RowDefinitions[1].ActualHeight;
            double topMargin = 23; // (ImageGrid.Height - MainGrid.RowDefinitions[1].Height.Value) / 2;

            MainGrid.RowDefinitions[1].Height = new GridLength(330);
            ImageGrid.VerticalAlignment = VerticalAlignment.Top;
            int indexer = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    indexer++;
                    ImageGrid.Margin = new Thickness(0, topMargin + 145 * fun(indexer), 0, 0);
                    await Task.Delay(10, token);
                }
                catch { /* Do nothing */ }
            }

            MainGrid.RowDefinitions[1].Height = new GridLength(originalHeight);
            ImageGrid.Margin = new Thickness(0);
            ImageGrid.VerticalAlignment = VerticalAlignment.Center;

            return;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            _driverProvider?.Dispose();
        }

        private async Task SayHello(string name)
        {
            var message = $"Привет, {name}!".ToCharArray();
            int length = message.Length;

            for (int i = 1; i <= length; i++)
            {
                NotificationHolder.Text = new string(message[..i]);
                await Task.Delay(100);
            }

            await Task.Delay(200);

            foreach (var x in new DescendingSQRT(100))
            {
                NotificationHolder.Opacity = x;
                await Task.Delay(25);
            }

            return;
        }

        private async void Button_CreateLink(object sender, RoutedEventArgs e)
        {
            Button it = (Button)sender;
            it.IsEnabled = false;
            string login = LoginBox.Text;
            string password = PasswordBox1.Password;
            string playerName = PlayerNameHolder.Text;
            string shortcutPath = "";
            if (playerName == "") playerName = "user";



            bool success = await Task.Run(() => {
                string profileFile = UserProfileFile.Save(login, password, playerName);

                if (profileFile == "")
                {
                    MessageBox.Show("Не удалось сохранить параметры входа игрока");
                    return false;
                }

                try
                {
                    WshShell WshShell = new WshShell();

                    string DesktopDir = (string)WshShell.SpecialFolders.Item("Desktop");
                    shortcutPath = Path.Combine(DesktopDir, $"{playerName}.lnk");

                    IWshShortcut shortcut = (IWshShortcut)WshShell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = Process.GetCurrentProcess().MainModule?.FileName ?? Environment.CurrentDirectory + "\\" + ExecutableName;
                    shortcut.WindowStyle = 1;
                    shortcut.Arguments = profileFile;
                    shortcut.Hotkey = "Ctrl+Shift+N";
                    shortcut.Description = "Daily Check";
                    shortcut.WorkingDirectory = Environment.CurrentDirectory;
                    shortcut.IconLocation = $"{shortcut.TargetPath}, 0";
                    shortcut.Save();
                    if (System.IO.File.Exists(shortcutPath)) return true;
                }
                catch { /* notify user in a code below */ }

                MessageBox.Show("Не удалось сохранить ярлык");
                return false;
            });

            if (success) {
                it.Content = "Пуск";
                it.Click -= Button_CreateLink;
                it.Click += (s, e) => {
                    ShellExecute(shortcutPath);
                    Close();
                };
            }
            it.IsEnabled = true;
        }

        private void ShellExecute(string fullPath) {
            Process process = new();
            process.StartInfo.FileName = fullPath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}