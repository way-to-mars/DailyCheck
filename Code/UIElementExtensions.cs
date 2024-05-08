using System.Windows;
using System.Windows.Controls;

namespace DailyCheck
{
    public static class UIElementExtensions
    {
        public static List<T> ToList<T>(this StackPanel windowBody) where T : UIElement
        {
            List<T> result = [];
            foreach (UIElement item in windowBody.Children)
                if (item is T view) result.Add(view);
            return result;
        }
    }
}
