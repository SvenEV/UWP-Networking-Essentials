using System;
using System.Linq;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels.StreamSockets;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials.Channels.Bluetooth
{
    /// <summary>
    /// (Work in progress, bluetooth connectivity is currently broken)
    /// </summary>
    public static class BluetoothConnection
    {
        // The Id of the Service Name SDP attribute
        internal const ushort SdpServiceNameAttributeId = 0x100;

        // The SDP Type of the Service Name SDP attribute.
        // The first byte in the SDP Attribute encodes the SDP Attribute Type as follows :
        // -  the Attribute Type size in the least significant 3 bits,
        // -  the SDP Attribute Type value in the most significant 5 bits.
        internal const byte SdpServiceNameAttributeType = (4 << 3) | 5;

        // The value of the Service Name SDP attribute (max. 255 bytes)
        internal const string SdpServiceName = "UwpNetworkingEssentials Service";

        public static async Task<StreamSocketConnection> ConnectAsync(Guid serviceUuid, IObjectSerializer serializer)
        {
            // Find all paired instances of the Rfcomm service
            var serviceId = RfcommServiceId.FromUuid(serviceUuid);
            var deviceSelector = RfcommDeviceService.GetDeviceSelector(serviceId);
            var devices = await DeviceInformation.FindAllAsync(deviceSelector);

            if (devices.Count > 0)
            {
                var device = devices.First(); // TODO

                var deviceService = await RfcommDeviceService.FromIdAsync(device.Id);

                if (deviceService == null)
                {
                    // Access to the device is denied because the application was not granted access
                    return null;
                }

                var attributes = await deviceService.GetSdpRawAttributesAsync();
                IBuffer serviceNameAttributeBuffer;

                if (!attributes.TryGetValue(SdpServiceNameAttributeId, out serviceNameAttributeBuffer))
                {
                    // The service is not advertising the Service Name attribute (attribute id = 0x100).
                    // Please verify that you are running a BluetoothConnectionListener.
                    return null;
                }

                using (var attributeReader = DataReader.FromBuffer(serviceNameAttributeBuffer))
                {
                    var attributeType = attributeReader.ReadByte();

                    if (attributeType != SdpServiceNameAttributeType)
                    {
                        // The service is using an unexpected format for the Service Name attribute.
                        // Please verify that you are running a BluetoothConnectionListener.
                        return null;
                    }

                    var serviceNameLength = attributeReader.ReadByte();

                    // The Service Name attribute requires UTF-8 encoding.
                    attributeReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                    var serviceName = attributeReader.ReadString(serviceNameLength);

                    var socket = new StreamSocket();
                    await socket.ConnectAsync(deviceService.ConnectionHostName, deviceService.ConnectionServiceName);

                    var connection = await StreamSocketConnection.ConnectAsync(socket, serializer);
                    return connection;
                }

            }

            // No devices found
            return null;
        }
    }
}
