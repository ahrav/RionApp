using BACCommunicationAPI.Abstractions.BACDevice;

namespace Rion
{
    public interface IDeviceManipulationPage
    {
        void SubscribeAndHandleDeviceAsync(IBacGenericDevice device);
    }
}