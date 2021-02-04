using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BACCommunicationAPI;
using BACCommunicationAPI.Abstractions.BACDevice;
using BACCommunicationAPI.Abstractions.Enumerations;
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