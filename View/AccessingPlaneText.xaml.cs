using System.Windows.Controls;

namespace DailyCheck.View
{
    public partial class AccessingPlaneText : UserControl
    {
        public string Text
        {
            get => TextHolder.Text;
            set => TextHolder.Text = value;
        }

        public AccessingPlaneText()
        {
            InitializeComponent();
            DataContext = this;
        }

        public AccessingPlaneText(string text)
        {
            InitializeComponent();
            DataContext = this;
            Text = text;
        }
    }
}
