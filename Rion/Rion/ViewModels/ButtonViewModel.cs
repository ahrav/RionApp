namespace Rion.ViewModels
{
    public class ButtonViewModel : BaseViewModel
    {
        private string _text;
        private bool _isEnabled;
        private Xamarin.Forms.Color _textColor;

        public string Text
        {
            get => _text;
            set
            {
                SetValue(ref _text, value);
                OnPropertyChanged();
            }
        }

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