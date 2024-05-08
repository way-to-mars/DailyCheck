using System.Data.SqlTypes;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace DailyCheck.View
{
    /// <summary>
    /// Логика взаимодействия для RewardPresenter.xaml
    /// </summary>
    public partial class RewardPresenter : UserControl
    {

        public const double WidgetWidth = 160;
        public const double WidgetHeight = 220;

        public RewardPresenter()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Width = WidgetWidth;
            this.Height = WidgetHeight;
        }

        public RewardPresenter SetBackGroundImage(string urlString)
        {
            if (!string.IsNullOrEmpty(urlString))
            {
                Uri resourceUri = new(urlString, UriKind.Absolute);
                ImageBackground.Source = new BitmapImage(resourceUri);
            }
            else
            {
                ImageBackground.Source = new BitmapImage(new Uri(@"D:\Code\CS\DailyCheck\img\empty_box.png", UriKind.Absolute));
            }
            return this;
        }

        public RewardPresenter SetContentImage(string urlString)
        {
            if (!string.IsNullOrEmpty(urlString))
            {
                Uri resourceUri = new(urlString, UriKind.Absolute);
                ImageContent.Source = new BitmapImage(resourceUri);
            }
            else
            {
                ImageContent.Source = new BitmapImage(new Uri(@"D:\Code\CS\DailyCheck\img\empty_prize.png", UriKind.Absolute));
            }
            return this;
        }

        public RewardPresenter SetContentDescription(string txt)
        {
            if (!string.IsNullOrEmpty(txt)) ContentDescription.Text = txt;
            else ContentDescription.Text = "";
            return this;
        }
    }
}
