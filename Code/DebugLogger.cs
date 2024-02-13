using System.Diagnostics;

namespace DailyCheck
{
    internal class DebugLogger
    {
        public static void Log(string msg)
        {
            string time = DateTime.Now.ToString("hh:mm:ss.ffffff"); ;
            string threadName = $"id:{Environment.CurrentManagedThreadId}-'{Thread.CurrentThread.Name ?? ""}'";

            Debug.WriteLine($"{time} [{threadName}] {msg}");
        }
    }
}
