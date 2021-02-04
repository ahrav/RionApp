namespace Rion.ViewModels
{
    public class ButtonViewModel : BaseViewModel
    {
        public string Text { get; set; }
        private bool _isEnabled;
        private Xamarin.Forms.Color _textColor;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                SetValue(ref _isEnabled, value);
                OnPropertyChanged();
            }
        }

        public Xamarin.Forms.Color TextColor
        {
            get => _textColor;
            set
            {
                SetValue(ref _textColor, value);
                OnPropertyChanged();
            }
        }
    }
}