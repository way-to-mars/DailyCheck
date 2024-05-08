using DailyCheck.Code;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DailyCheck.View
{
    public partial class ButtonLGC : UserControl
    {
        private readonly string Title1 = "Поиск LGC...";
        private readonly string Title2 = "Запустить LGC";
        private SettingsFile SettingsFile => ((App)Application.Current).SettingsFile;

        public ButtonLGC()
        {
            CancellationTokenSource cts = new();

            InitializeComponent();
            this.DataContext = this;
            this.IsEnabled = false;
            ButtonText.Text = Title1;
            _ = AnimateSearching(cts.Token);
            _ = SearchLesta(cts);
        }

        private async Task AnimateSearching(CancellationToken token)
        {
            double t = 0;
            while (!token.IsCancellationRequested)
            {
                double y = Math.Sin(t);
                double x = Math.Cos(t);
                SearchIcon.Margin = new Thickness(4 - 4 * x, 4 - 4 * y, 0, 0);
                t = (t + 0.5) % (2 * Math.PI);
                await Task.Delay(100, token);
            }
        }

        private async Task SearchLesta(CancellationTokenSource cts)
        {
            List<string?> paths = [
                await SearchByClasses1(),
                await SearchByClasses2(),
                await SearchByMUI1(),
                await SearchByMUI2(),
                await SearchByUninstall(),
                ];            

            IEnumerable<string> approved = paths.FindAll(it => it is not null).Where(it => it!.EndsWith("lgc.exe\"")).Select(it => it!);
            cts.Cancel();

            if (approved.Any())
            {
                string mostOccuring = approved
                    .GroupBy(str => str)
                    .OrderByDescending(grp => grp.Count())
                    .Select(grp => grp.Key)
                    .First();
                OnLestaFound(mostOccuring);
            }
            else
            {
                OnLestaMissing();
            }            
        }

        private void OnLestaFound(string path)
        {
            ButtonText.Text = Title2;
            SearchIconHolder.Visibility = Visibility.Collapsed;
            this.IsEnabled = true;
            this.MainButton.Click += (s, e) =>
            {
                Process.Start(path, "");
                if (SettingsFile.Content != "") Process.Start(SettingsFile.Content);
            };
            MainButton.ToolTip = new ToolTip()
            {
                Content = $"Запустить Lesta Game Center {path}",
            };
        }

        private void OnLestaMissing()
        {
            ButtonText.Text = Title2;
            SearchIconHolder.Visibility = System.Windows.Visibility.Collapsed;
            MainButton.ToolTip = new ToolTip()
            {
                Content = "Не удалось найти Lesta Game Center",

            };
        }

        private static async Task<string?> SearchByClasses1()
        {
            // HKEY_CLASSES_ROOT\lgc\shell\open\commmand
            // "...lgc.exe" "%1"
            Task<string?> task = Task.Run(() =>
                {
                    try
                    {
                        RegistryKey? regKey = Registry.ClassesRoot.OpenSubKey(@"lgc\shell\open\command");
                        string? value = regKey?.GetValue("")?.ToString() ?? null;
                        string? path = value?.Split(" ")[0] ?? null;
                        return path;
                    }
                    catch { return null; }
                });
            await task;
            return task.Result.WithSingleQuotes();
        }
        private static async Task<string?> SearchByClasses2()
        {
            // HKEY_CURRENT_USER\SOFTWARE\Classes\lgc\shell\open\commmand
            // "...lgc.exe" "%1"
            Task<string?> task = Task.Run(() =>
            {
                try
                {
                    RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\lgc\shell\open\command");
                    string? value = regKey?.GetValue("")?.ToString() ?? null;
                    string? path = value?.Split(" ")[0] ?? null;
                    return path;
                }
                catch { return null; }
            });
            await task;
            return task.Result.WithSingleQuotes();
        }
        private static async Task<string?> SearchByMUI1()
        {
            // \HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache
            // D:\Games\Lesta\GameCenter\lgc.exe.FriendlyAppName  ---> Lesta Game Center
            Task<string?> task = Task.Run(() =>
            {
                try
                {
                    RegistryKey? regKey = Registry.ClassesRoot.OpenSubKey(@"Local Settings\Software\Microsoft\Windows\Shell\MuiCache");
                    var values = regKey?.GetValueNames() ?? Enumerable.Empty<string>();

                    var value = values.Where(name => name.EndsWith("lgc.exe.FriendlyAppName")).First();
                    return value[..value.IndexOf(".FriendlyAppName")];  
                }
                catch { return null; }
            });
            await task;
            return task.Result.WithSingleQuotes();
        }        
        private static async Task<string?> SearchByMUI2()
        {
            // \HKEY_CURRENT_USER\SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache
            // D:\Games\Lesta\GameCenter\lgc.exe.FriendlyAppName  ---> Lesta Game Center
            Task<string?> task = Task.Run(() =>
            {
                try
                {
                    RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache");
                    var values = regKey?.GetValueNames() ?? Enumerable.Empty<string>();

                    var value = values.Where(name => name.EndsWith("lgc.exe.FriendlyAppName")).First();
                    return value[..value.IndexOf(".FriendlyAppName")];
                }
                catch { return null; }
            });
            await task;
            return task.Result.WithSingleQuotes();
        }
        private static async Task<string?> SearchByUninstall()
        {
            // HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Lesta Game Center
            // DisplayIcon  ---> D:\Games\Lesta\GameCenter\lgc.exe,0
            Task<string?> task = Task.Run(() =>
            {
                try
                {
                    RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Lesta Game Center");
                    var value = regKey?.GetValue("DisplayIcon")?.ToString();
                    return value?[..value.IndexOf(",0")];
                }
                catch { return null; }
            });
            await task;
            return task.Result.WithSingleQuotes();
        }
    }

    public static partial class StringExtensions
    {
        public static string? WithSingleQuotes(this string? text)
        {
            return (text == null)
                ? null
                : $"\"{text.Trim('\"')}\"";
        }
    }

 }
