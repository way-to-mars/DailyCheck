using System.IO;

namespace DailyCheck
{
    public class UserProfileFile
    {
        private static string FullName(string playerName)
        {
            string workingPath;
            string fileName = $"{playerName}.bin";

            try
            {
                workingPath = Directory.GetCurrentDirectory();
            }
            catch (Exception)
            {
                workingPath = Environment.CurrentDirectory;  // no backslash at the end
            }
            try
            {
                return Path.Combine(workingPath, fileName);
            }
            catch
            {
                return $"{workingPath}\\{fileName}";
            }
        }
        public static string Save(string login, string password, string playerName)
        {
            string fileName = FullName(playerName);
            try
            {
                FileIO.Encrypt(login, password, out var encryptedBytes);
                using var file = File.OpenWrite(fileName);
                file.Write(encryptedBytes);
            }
            catch
            {
                return "";
            }
            if (File.Exists(fileName)) return fileName;
            else return "";
        }

        public static bool Load(string fullName, out string login, out string password)
        {
            try
            {
                using var file = File.OpenRead(fullName);
                byte[] encryptedBytes = new byte[file.Length];
                file.Read(encryptedBytes, 0, encryptedBytes.Length);
                FileIO.Decrypt(encryptedBytes, out login, out password);
                return true;
            }
            catch
            {
                login = "";
                password = "";
                return false;
            }
        }
    }
}
