using System.IO;
using System.Text;

namespace DailyCheck.Code
{
    public class SettingsFile
    {
        private const string name = "settings.ini";
        private readonly Encoding encoding = Encoding.Unicode;
        private string? content;

        public string Content
        {
            get => (content == null) ? "" : content as string;
            set => ApplyContent(value);    
        }

        public SettingsFile()
        {
            content = ReadFile();
        }

        private static string FullName()
        {
            string workingPath;

            try
            {
                workingPath = System.IO.Directory.GetCurrentDirectory();
            }
            catch (Exception) 
            {
                workingPath = Environment.CurrentDirectory;  // no backslash at the end
            }
            try
            {
                return Path.Combine(workingPath, name);
            }
            catch
            {
                return $"{workingPath}\\{name}";
            }
        }

        private string? ReadFile()
        {
            try
            {
                using var file = new StreamReader(FullName(), encoding);
                return file.ReadLine();
            }
            catch
            {
                return null;
            }
        }

        private bool ApplyContent(string value)
        {
            try
            {
                using var file = new StreamWriter(FullName(), false, encoding);
                file.WriteLine(value);
                content = value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
