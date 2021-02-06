using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using BACCommunicationAPI;
using BACCommunicationAPI.Abstractions.BACDevice;
using BACCommunicationAPI.Abstractions.Enumerations;
using Xamarin.Forms;

namespace Rion.ViewModels
{
    public class MainViewModel : IDeviceManipulationPage
    {
        private readonly IPageService _pageService;
        private static ObservableCollection<string> List { get; set; } = new ObservableCollection<string>();
        private static IBacGenericDevice ConnectedDevice { get; set; }
        public ButtonViewModel ConnectButton { get; private set; }
        public LabelViewModel ConnectedDeviceLabel { get; private set; }
        public LabelViewModel VoltageLabel { get; private set; }
        private const int WheelSize = 11;
        private readonly FeedViewModel _model;
        public FeedViewModel Model => _model;
        
        public ICommand ConnectDeviceCommand { get; private set; }

        public MainViewModel(IPageService pageService)
        {
            Console.WriteLine($"Local device is {App.LocalDevice}");
            ConnectedDevice = App.LocalDevice;
            _pageService = pageService;
            _model = new FeedViewModel();
            
            
            if (App.LocalDevice == null)
            {
                ConnectButton = new ButtonViewModel {IsEnabled = true};
                ConnectDeviceCommand = new Command(ConnectController);
                ConnectedDeviceLabel = new LabelViewModel{LabelText = App.LocalDevice != null ? "Rion Thrust": "(Connect Device)"};
                VoltageLabel = new LabelViewModel {LabelText = ""};
                SetupCommunications();
            }
            HandleConnectionState();
        }

        private void SetupCommunications()
        {
            BacCommunication.CurrentRepository.BluetoothLeScanningChange += CurrentRepository_BluetoothLeScanningChange;
            Device.BeginInvokeOnMainThread(handleScanningChange);
            StartAutoConnectionRoutine();
        }

        private async void ConnectController()
        {
            if (!Application.Current.Properties.ContainsKey("device")) await _pageService.PushAsync(new ListOfDevices(this));
            else
            {
                StartAutoConnectionRoutine();
            }
        }
        
        private async void ReadSpeed()
        {
            if (ConnectedDevice == null)
            {
                Model.Speed = 0;
                return;
            }
            while (ConnectedDevice != null && ConnectedDevice.ConnectionState == BACConnectionState.CONNECTED)
            {
                try
                {
                    var rpm = await ConnectedDevice.Read(263);
                    Model.Speed = (rpm * WheelSize) / 336;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    Debug.WriteLine(exception);
                    UnsubscribeToDevice();
                    HandleConnectionState();
                }
            }
        }

        private async void ReadVoltage()
        {
            if (ConnectedDevice == null)
            {
                Model.Voltage = 00.0;
                VoltageLabel.LabelText = "";
                return;
            }

            while (ConnectedDevice != null && ConnectedDevice.ConnectionState == BACConnectionState.CONNECTED)
            {
                try
                {
                    Model.Voltage = await ConnectedDevice.Read(265) / (double) 32;
                    VoltageLabel.LabelText = Model.Voltage.ToString("F0");
                    Console.WriteLine(VoltageLabel.LabelText);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    Debug.WriteLine(exception);
                    UnsubscribeToDevice();
                    HandleConnectionState();
                }

            }
        }

        private async void ReadCurrent()
        {
            if (ConnectedDevice == null)
            {
                Model.Amps = 0.0;
                return;
            }

            while (ConnectedDevice != null && ConnectedDevice.ConnectionState == BACConnectionState.CONNECTED)
            {
                try
                {
                    Model.Amps = (await ConnectedDevice.Read(262) / (double)32) * 2.0;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    Debug.WriteLine(exception);
                    UnsubscribeToDevice();
                    HandleConnectionState();
                }
            }
        }

        private async void ReadTemp()
        {
            if (ConnectedDevice == null)
            {
                Model.Temp = 0.0;
                return;
            }

            while (ConnectedDevice != null && ConnectedDevice.ConnectionState == BACConnectionState.CONNECTED)
            {
                try
                {
                    Model.Temp = await ConnectedDevice.Read(259) * (9 / 5.0) + 32.0;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    Debug.WriteLine(exception);
                    UnsubscribeToDevice();
                    HandleConnectionState();
                }
            }
        }
        
        /// <summary>
        /// Unsubscribe from connection events
        /// </summary>
        private void UnsubscribeToDevice()
        {
            Model.Speed = 0;
            Model.Amps = 0.0;
            Model.Temp = 0.0;
            Model.Voltage = 0.0;
            VoltageLabel.LabelText = "";
            ConnectedDeviceLabel.LabelText = "(Connect Device)";
            if (ConnectedDevice == null) return;
            ConnectedDevice.ConnectionStateChanged -= Device_ConnectionStateChanged;
        }
        
        /// <summary>
        /// Event Handler for scanning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentRepository_BluetoothLeScanningChange(object sender, BACCommunicationAPI.Abstractions.BACDeviceRepository.ScanningBluetoothLEEventeArgs e)
        {
            Device.BeginInvokeOnMainThread(handleScanningChange);
        }

        private void ConnectDevice()
        {
            if (!Application.Current.Properties.ContainsKey("device")) return;
            List.Add(Application.Current.Properties["device"].ToString());
        }

        /// <summary>
        /// Start an auto-connection sequence
        /// </summary>
        private async void StartAutoConnectionRoutine()
        {
            Console.WriteLine("Started auto connect !!");
            ConnectDevice();
            if (ConnectedDevice != null)
            {
                await ConnectedDevice.Disconnect();
                ConnectedDevice = null;
            }

            Console.WriteLine($"list is: {List}");
            while (ConnectedDevice == null)
            {
                ConnectedDevice = await BacCommunication.CurrentRepository.StartBluetoothLeAutoConnection(List);
            }
            Console.WriteLine($"Device isss: {ConnectedDevice}");

            //do stuff with new device.
            App.LocalDevice = ConnectedDevice;
            Console.WriteLine("Started auto connect and have a device readyyy!!");
            HandleConnectionState();
            SubscribeToConnectedDevice();
        }

        /// <summary>
        /// Handle layout changes from scanning change
        /// </summary>
        private void handleScanningChange()
        {
            Console.WriteLine("Handling scanning state");
        }

        /// <summary>
        /// Subscribe to connection events
        /// </summary>
        /// <param name="device"></param>
        private void SubscribeToConnectedDevice()
        {
            ConnectedDevice.ConnectionStateChanged += Device_ConnectionStateChanged;
        }
        
        /// <summary>
        /// event handler for connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Device_ConnectionStateChanged(object sender, ConnectionStateChangeArgs e)
        {
            Device.BeginInvokeOnMainThread(HandleConnectionState);
        }

        /// <summary>
        /// Adjust layout based on connection state
        /// </summary>
        private void HandleConnectionState()
        {
            ReadSpeed();
            ReadVoltage();
            ReadCurrent();
            ReadTemp();
            if (ConnectedDevice == null) return;

            switch (ConnectedDevice.ConnectionState)
            {
                case BACConnectionState.CONNECTED:
                    Console.WriteLine("Running thisss");
                    ConnectButton.IsEnabled = false;
                    ConnectButton.TextColor = Color.LightGray;
                    ConnectedDeviceLabel.LabelText = "Rion Thrust";
                    break;
                case BACConnectionState.CONNECTING:
                    ConnectButton.IsEnabled = false;
                    ConnectButton.TextColor = Color.LightGray;
                    break;
                case BACConnectionState.DISCONNECTED:
                    Console.WriteLine("Disconnected!!!!!");
                    StartAutoConnectionRoutine();
                    ConnectButton.IsEnabled = true;
                    break;
                case BACConnectionState.DISCONNECTING:
                    ConnectButton.IsEnabled = false;
                    ConnectButton.TextColor = Color.LightGray;
                    break;
                case BACConnectionState.ERROR_STATE:
                    UnsubscribeToDevice();
                    break;
            }
        }

        public async void SubscribeAndHandleDeviceAsync(IBacGenericDevice device)
        {
            //check if we chose the same device
            if (ConnectedDevice == device) return;

            if (ConnectedDevice != null)
            {
                //unsubsribe from connection events. Devices could potentially persist.
                ConnectedDevice.ConnectionStateChanged -= Device_ConnectionStateChanged;

                //Disconnect if choosing new device.
                if (ConnectedDevice.ConnectionState != BACConnectionState.DISCONNECTED)
                {
                    await ConnectedDevice.Disconnect();
                }
            }

            ConnectedDevice = device;

            if (ConnectedDevice != null)
            {
                ConnectedDevice.ConnectionStateChanged += Device_ConnectionStateChanged;
                Application.Current.Properties["device"] = ConnectedDevice.Guid;
            }

            // HandleConnectionState();
        }
    }
}