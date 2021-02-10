using Rion.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PortraitContent : ContentView
    {
        public PortraitContent()
        {
            DependencyService.Get<IScreenManager>().KeepScreenOn();
            BindingContext = new MainViewModel(new PageService());
            InitializeComponent();
        }

    }
}