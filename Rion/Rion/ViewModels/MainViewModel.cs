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
        private readonly StatsViewModel _stats;
        public FeedViewModel Model => _model;

        public StatsViewModel Stats => _stats;
        
        public ICommand ConnectDeviceCommand { get; private set; }
        
        public ICommand SwipeComand { get; set; }

        public MainViewModel(IPageService pageService)
        {
            ConnectedDevice = App.LocalDevice;
            _pageService = pageService;
            _model = new FeedViewModel();
            _stats = new StatsViewModel();
            
            
            if (App.LocalDevice == null)
            {
                ConnectButton = new ButtonViewModel {IsEnabled = true};
                ConnectDeviceCommand = new Command(ConnectController);
                ConnectedDeviceLabel = new LabelViewModel{LabelText = App.LocalDevice != null ? "Rion Thrust": "(Connect Device)"};
                VoltageLabel = new LabelViewModel {LabelText = "0.0", LabelColor = Color.DimGray};
                SetupCommunications();
            }
            HandleConnectionState();
        }

        private void SetupCommunications()
        {
            BacCommunication.CurrentRepository.BluetoothLeScanningChange += CurrentRepository_BluetoothLeScanningChange;
            Device.BeginInvokeOnMainThread(HandleScanningChange);
            ConnectDevice();
            StartAutoConnectionRoutine();
        }

        private async void ConnectController()
        {
            if (ConnectedDevice != null)
            {
                switch (ConnectedDevice.ConnectionState)
                {
                    case BACConnectionState.DISCONNECTED when !Application.Current.Properties.ContainsKey("device"):
                        await _pageService.PushAsync(new ListOfDevices(this));
                        break;
                    case BACConnectionState.DISCONNECTED:
                        StartAutoConnectionRoutine();
                        break;
                    case BACConnectionState.CONNECTED:
                        await ConnectedDevice.Disconnect();
                        List.Remove(Application.Current.Properties["device"].ToString());
                        ConnectedDevice = null;
                        break;
                    case BACConnectionState.CONNECTING:
                        break;
                    case BACConnectionState.DISCONNECTING:
                        break;
                    case BACConnectionState.ERROR_STATE:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                if (!Application.Current.Properties.ContainsKey("device")) await _pageService.PushAsync(new ListOfDevices(this));
                SetupCommunications();
            }
            
        }
        
        private async void ReadSpeed()
        {
            if (ConnectedDevice == null || ConnectedDevice.ConnectionState == BACConnectionState.DISCONNECTED)
            {
                Model.Speed = 0;
                return;
            }
            while (ConnectedDevice != null && ConnectedDevice.ConnectionState == BACConnectionState.CONNECTED)
            {
                try
                {
                    var speed = Math.Round(await ConnectedDevice.Read(260) / (float) 256 /1.609344, 0);
                    Model.Speed = speed;
                    if (speed > App.LifetimeSpeed)
                    {
                        Stats.LifetimeSpeed = speed;
                        App.LifetimeSpeed = speed;
                    }
                    if (speed > Stats.SessionSpeed) Stats.SessionSpeed = speed;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    UnsubscribeToDevice();
                    HandleConnectionState();
                }
            }
        }

        private async void ReadVoltage()
        {
            if (ConnectedDevice == null || ConnectedDevice.ConnectionState == BACConnectionState.DISCONNECTED)
            {
                Model.Voltage = 0.0;
                VoltageLabel.LabelText = "0.0";
                VoltageLabel.LabelColor = Color.DimGray;
                return;
            }

            while (ConnectedDevice != null && ConnectedDevice.ConnectionState == BACConnectionState.CONNECTED)
            {
                try
                {
                    Model.Voltage = await ConnectedDevice.Read(265) / (double) 32;
                    VoltageLabel.LabelText = Model.Voltage.ToString("F0");
                    VoltageLabel.LabelColor = Color.Black;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    UnsubscribeToDevice();
                }

            }
        }

        private async void ReadCurrent()
        {
            if (ConnectedDevice == null || ConnectedDevice.ConnectionState == BACConnectionState.DISCONNECTED)
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
                    UnsubscribeToDevice();
                }
            }
        }

        private async void ReadTemp()
        {
            if (ConnectedDevice == null || ConnectedDevice.ConnectionState == BACConnectionState.DISCONNECTED)
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
                    UnsubscribeToDevice();
                }
            }
        }
        
        /// <summary>
        /// Unsubscribe from connection events
        /// </summary>
        private void UnsubscribeToDevice()
        {
            Console.WriteLine("unsub!!");
            // Model.Speed = 0;
            // Model.Amps = 0.0;
            // Model.Temp = 0.0;
            // Model.Voltage = 0.0;
            // VoltageLabel.LabelText = "";
            // ConnectedDeviceLabel.LabelText = "(Connect Device)";
            // ConnectButton.Text = "Connect";
            // ConnectButton.IsEnabled = true;
            // if (ConnectedDevice == null) return;
            if (ConnectedDevice != null) ConnectedDevice.ConnectionStateChanged -= Device_ConnectionStateChanged;
            else
            {
                HandleConnectionState();
            }
        }
        
        /// <summary>
        /// Event Handler for scanning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentRepository_BluetoothLeScanningChange(object sender, BACCommunicationAPI.Abstractions.BACDeviceRepository.ScanningBluetoothLEEventeArgs e)
        {
            Device.BeginInvokeOnMainThread(HandleScanningChange);
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
            if (ConnectedDevice != null)
            {
                await ConnectedDevice.Disconnect();
                ConnectedDevice = null;
            }

            while (ConnectedDevice == null && List.Count > 0)
            {
                ConnectedDevice = await BacCommunication.CurrentRepository.StartBluetoothLeAutoConnection(List);
                if (ConnectedDevice != null)
                {
                    var wheelDiameter = await ConnectedDevice.Read(227);
                    if (Math.Abs(wheelDiameter - 279.4) > 0.1)
                    {
                        try
                        {
                            await ConnectedDevice.Write(227, (short) 279.4);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            UnsubscribeToDevice();
                            HandleConnectionState();
                        }
                    }

                    var gearRatio = await ConnectedDevice.Read(226) / 256;
                    if (gearRatio != 1)
                    {
                        try
                        {
                            await ConnectedDevice.Write(226, 1);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            UnsubscribeToDevice();
                            HandleConnectionState();
                        }
                    }
                }
            }
            if (ConnectedDevice == null) return;
            App.LocalDevice = ConnectedDevice;
            HandleConnectionState();
            SubscribeToConnectedDevice();
        }

        /// <summary>
        /// Handle layout changes from scanning change
        /// </summary>
        private void HandleScanningChange()
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

        private void Reset()
        {
            // Model.Speed = 0;
            // Model.Voltage = 0.0;
            // VoltageLabel.LabelText = "0.0";
            // Model.Amps = 0.0;
            // Model.Temp = 0.0;
            ConnectedDeviceLabel.LabelText = "(Connect Device)";
            ConnectButton.Text = "Connect";
            ConnectButton.IsEnabled = true;
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
            if (ConnectedDevice == null)
            {
                Reset();
                return;
            }

            switch (ConnectedDevice.ConnectionState)
            {
                case BACConnectionState.CONNECTED:
                    ConnectButton.IsEnabled = true;
                    ConnectButton.Text = "Disconnect";
                    ConnectedDeviceLabel.LabelText = "Rion Thrust";
                    Console.WriteLine($"run connected? {ConnectButton.Text} {ConnectButton.IsEnabled}");
                    break;
                case BACConnectionState.CONNECTING:
                    ConnectButton.IsEnabled = false;
                    ConnectButton.TextColor = Color.LightGray;
                    break;
                case BACConnectionState.DISCONNECTED:
                    if (List.Count > 0) StartAutoConnectionRoutine();
                    else
                    {
                        ConnectedDeviceLabel.LabelText = "(Connect Device)";
                        ConnectButton.Text = "Connect";
                        ConnectButton.IsEnabled = true;
                    }
                    Console.WriteLine($"run disconnected? {ConnectButton.Text} {ConnectButton.IsEnabled}");
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