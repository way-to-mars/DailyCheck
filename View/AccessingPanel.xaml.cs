using System.Windows.Controls;

namespace DailyCheck.View
{
    public partial class ConsolePanel : UserControl
    {
        public ConsolePanel()
        {
            InitializeComponent();
        }

        public AccessingElement Add(string loading, string success, string failed, AccessingElement.StateEnum state)
        {
            AccessingElement element = new(
                textLoading: loading,
                textSuccess: success,
                textFailure: failed,
                state: state
                );           

            ElementsPanel.Children.Add(element);
            Scroller.ScrollToBottom();
            return element;
        }

        public void Notification(string message)
        {
            AccessingPlaneText element = new(message);
            ElementsPanel.Children.Add(element);
            Scroller.ScrollToBottom();
            DebugLogger.Log($"[Notification] {message}");
        }

        public void LastDone(string? message = null)
        {
            List<AccessingElement> list = ElementsPanel.ToList<AccessingElement>();
            if (list.Count != 0)
            {
                AccessingElement last = list.Last();                
                if (message != null) last.TextSuccess = message;
                DebugLogger.Log($"[Done] {last.TextSuccess}");
                last.State = AccessingElement.StateEnum.Success;
            }
        }

        public void LastFailure(string? message = null)
        {
            List<AccessingElement> list = ElementsPanel.ToList<AccessingElement>();
            if (list.Count != 0)
            {
                AccessingElement last = list.Last();                
                if (message != null) last.TextFailure = message;
                DebugLogger.Log($"[Failed] {last.TextFailure}");
                last.State = AccessingElement.StateEnum.Failure;
            }
        }
    }
}
