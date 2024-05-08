using DailyCheck.Code;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace DailyCheck.View
{
    public partial class SettingsDialog : Window
    {
        private SettingsFile SettingsFile => ((App)Application.Current).SettingsFile;

        public SettingsDialog()
        {
            InitializeComponent();
            PathTextBox.Text = SettingsFile.Content;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            string _old = SettingsFile.Content;
            string _new = PathTextBox.Text;
            if (_old != _new)
            {
                SettingsFile.Content = PathTextBox.Text;
                DebugLogger.Log($"SettingsFile.Content {_old} => {_new}");
            }
            else
            {
                DebugLogger.Log($"SettingsFile.Content {_old} remains {_new}");
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
            Close();
        }

        private void ButtonClose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            PathTextBox.Clear();
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Приложение|*.exe;*.com;*.bat;*.cmd|Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
                PathTextBox.Text = openFileDialog.FileName;
        }
    }   
}
