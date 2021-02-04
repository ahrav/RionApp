namespace Rion.ViewModels
{
    public class LabelViewModel : BaseViewModel
    {
        private string _labelText;
        private Xamarin.Forms.Color _labelColor;

        public string LabelText
        {
            get => _labelText;
            set
            {
                SetValue(ref _labelText, value);
                OnPropertyChanged();
            }
        }

        public Xamarin.Forms.Color LabelColor
        {
            get => _labelColor;
            set
            {
                SetValue(ref _labelColor, value);
                OnPropertyChanged();
            }
        }
    }
}