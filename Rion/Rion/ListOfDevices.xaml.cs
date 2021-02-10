using System;
using System.Collections.ObjectModel;
using BACCommunicationAPI;
using BACCommunicationAPI.Abstractions.BACDevice;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Rion
{
   [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListOfDevices
    {
        private readonly IDeviceManipulationPage _page;

        private ObservableCollection<IBacGenericDevice> Items { get; set; } = new ObservableCollection<IBacGenericDevice>(BacCommunication.CurrentRepository.BacDevices);

        public ListOfDevices(IDeviceManipulationPage page)
        {
            InitializeComponent();

            _page = page;
        }

        /// <summary>
        /// Enable and Disable items based on Scanning State.
        /// Note: on iOS, the list doesn't always appear to be refreshing.
        /// </summary>
        private void HandleScanningState()
        {
            MyListView.IsRefreshing = BacCommunication.CurrentRepository.IsBluetoothLeScanning;
            ScanButton.IsEnabled = !BacCommunication.CurrentRepository.IsBluetoothLeScanning;
            StopScanButton.IsEnabled = BacCommunication.CurrentRepository.IsBluetoothLeScanning;

            //hack: this is added to show on iOS that it's still scanning.
            IsBusy = BacCommunication.CurrentRepository.IsBluetoothLeScanning;
        }

        /// <summary>
        /// On Appearing, subscribe to Repository change & scan events
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            Items = new ObservableCollection<IBacGenericDevice>(BacCommunication.CurrentRepository.BacDevices);

            BacCommunication.CurrentRepository.DeviceRepositoryChange += CurrentRepository_DeviceRepositoryChange;
            BacCommunication.CurrentRepository.BluetoothLeScanningChange += CurrentRepository_BluetoothLeScanningChange;

            MyListView.ItemsSource = Items;
            HandleScanningState();
        }

        /// <summary>
        /// On Disappearing, unsubscribe to Repository change & scan events
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            BacCommunication.CurrentRepository.DeviceRepositoryChange -= CurrentRepository_DeviceRepositoryChange;
            BacCommunication.CurrentRepository.BluetoothLeScanningChange -= CurrentRepository_BluetoothLeScanningChange;

            //stop scan if we are scanning
            BacCommunication.CurrentRepository.StopBluetoothLeScan();
        }

        /// <summary>
        /// Show list refresh action if scanning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentRepository_BluetoothLeScanningChange(object sender, BACCommunicationAPI.Abstractions.BACDeviceRepository.ScanningBluetoothLEEventeArgs e)
        {
            Device.BeginInvokeOnMainThread(HandleScanningState);
        }

        /// <summary>
        /// Change list if devices are found or lost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentRepository_DeviceRepositoryChange(object sender, BACCommunicationAPI.Abstractions.BACDeviceRepository.DeviceRepositoryChangeArgs e)
        {
            if (e.DevicesAdded != null)
            {
                foreach (var device in e.DevicesAdded)
                {
                    Items.Add(device);
                }
            }

            if (e.DevicesRemoved != null)
            {
                foreach (var device in e.DevicesRemoved)
                {
                    Items.Remove(device);
                }
            }
        }

        /// <summary>
        /// Send device to main Page on selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null || !(e.Item is IBacGenericDevice device))
                return;
            _page.SubscribeAndHandleDeviceAsync(device);
            await Navigation.PushAsync(new MainPage());
        }

        /// <summary>
        /// Perform a short scan on Pull-To-Refresh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MyListView_OnRefreshing(object sender, EventArgs e)
        {
            try
            {
                await BacCommunication.CurrentRepository.StartBluetoothLeScanWithTimeout(5.0);
            }
            catch (Exception ex)
            {
                HandleScanningState();
                await DisplayAlert("Error", "Exception starting scan: " + ex.Message, "Ok");
            }
        }

        /// <summary>
        /// Start an indefinite Scan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ScanButton_OnClicked(object sender, EventArgs e)
        {
            try
            {
                await BacCommunication.CurrentRepository.StartBluetoothLeScan();
            }
            catch (Exception ex)
            {
                HandleScanningState();
                await DisplayAlert("Error", "Exception starting scan: " + ex.Message, "Ok");
            }
        }

        /// <summary>
        /// Stop a scan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopScanButton_OnClicked(object sender, EventArgs e)
        {
            BacCommunication.CurrentRepository.StopBluetoothLeScan();
        }
    }
}