using BACCommunicationAPI.Abstractions.BACDevice;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Rion
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
            if (!Current.Properties.ContainsKey("lifetimeSpeed")) LifetimeSpeed = 0;
        }
        
        public static IBacGenericDevice LocalDevice { get; set; }

        public static double LifetimeSpeed
        {
            get => (double) Current.Properties["lifetimeSpeed"];
            set => Current.Properties["lifetimeSpeed"] = value;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}