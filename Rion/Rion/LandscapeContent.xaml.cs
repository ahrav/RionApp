using Rion.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rion
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandscapeContent
    {

	    public LandscapeContent()
        {
	        DependencyService.Get<IScreenManager>().KeepScreenOn();
	        BindingContext = new MainViewModel(new PageService());
	        InitializeComponent();
        }
    }
}