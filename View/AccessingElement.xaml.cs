using System.Windows;
using System.Windows.Controls;

namespace DailyCheck.View
{
    /// <summary>
    /// Логика взаимодействия для AccessingElement.xaml
    /// </summary>
    public partial class AccessingElement : UserControl
    {
        public string TextLoading { get; set; }
        public string TextSuccess { get; set; }
        public string TextFailure { get; set; }

        public enum StateEnum
        {
            Loading,
            Success,
            Failure,
        }

        private StateEnum _state;
        public StateEnum State
        {
            get => _state;// ?? throw new InvalidOperationException("State is not initialized");
            set
            {
                _state = value;
                OnSetState(value);
            }
        }

        public void Update() => OnSetState(State);

        private void OnSetState(StateEnum value)
        {
            switch (value)
            {
                case StateEnum.Loading:
                    TextHolder.Text = TextLoading;
                    ViewLoading.Visibility = Visibility.Visible;
                    ViewSuccess.Visibility = Visibility.Collapsed;
                    ViewFailure.Visibility = Visibility.Collapsed;
                    break;

                case StateEnum.Success:
                    TextHolder.Text = TextSuccess;
                    ViewLoading.Visibility = Visibility.Collapsed;
                    ViewSuccess.Visibility = Visibility.Visible;
                    ViewFailure.Visibility = Visibility.Collapsed;
                    break;

                case StateEnum.Failure:
                    TextHolder.Text = TextFailure;
                    ViewLoading.Visibility = Visibility.Collapsed;
                    ViewSuccess.Visibility = Visibility.Collapsed;
                    ViewFailure.Visibility = Visibility.Visible;
                    break;

                default:
                    throw new InvalidCastException(nameof(value));
            }
        }

        // [Obsolete("Empty constructor is for Preview Mode only, please use a constructor with parameters instead.")]
        public AccessingElement()
        {
            InitializeComponent();
            this.DataContext = this;            
            TextLoading = TextSuccess = TextFailure = string.Empty;
            State = StateEnum.Loading;
        }

        public AccessingElement(string textLoading, string textSuccess, string textFailure, AccessingElement.StateEnum state)
        {
            InitializeComponent();
            this.DataContext = this;
            TextLoading = textLoading;
            TextSuccess = textSuccess;
            TextFailure = textFailure;
            State = state;
        }
    }
}
