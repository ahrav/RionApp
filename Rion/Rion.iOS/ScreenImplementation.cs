using Rion.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ScreenImplementation))]
namespace Rion.iOS
{
    public class ScreenImplementation : IScreenManager
    {
        public void KeepScreenOn()
        {
            UIApplication.SharedApplication.IdleTimerDisabled = true;
        }
        
    }
}