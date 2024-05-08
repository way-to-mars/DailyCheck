using System.Windows.Input;
using System.Windows.Controls;

namespace DailyCheck.View
{
    public partial class AccessingButton : UserControl
    {
        public event Action<object, MouseButtonEventArgs> Click = delegate { };

        public AccessingButton()
        {
            InitializeComponent();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Click.Invoke(sender, e);
        }
    }
}
