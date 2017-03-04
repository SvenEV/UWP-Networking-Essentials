using System;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels.StreamSockets;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace UwpNetworkingEssentials.Channels.Bluetooth
{
    /// <summary>
    /// (Work in progress, bluetooth connectivity is currently broken)
    /// </summary>
    public class BluetoothConnectionListener : ConnectionListenerBase<StreamSocketConnection>
    {
        private readonly StreamSocketListener _listener = new StreamSocketListener();
        private readonly IObjectSerializer _serializer;
        private readonly Guid _serviceUuid;
        private RfcommServiceProvider _serviceProvider;

        public BluetoothConnectionListener(Guid serviceUuid, IObjectSerializer serializer)
        {
            _serviceUuid = serviceUuid;
            _serializer = serializer;
            _listener.ConnectionReceived += OnConnectionReceived;
        }

        protected override Task DisposeCoreAsync()
        {
            _serviceProvider.StopAdvertising();
            _listener.Dispose();
            return Task.CompletedTask;
        }

        protected override async Task StartCoreAsync()
        {
            var serviceId = RfcommServiceId.FromUuid(_serviceUuid);
            _serviceProvider = await RfcommServiceProvider.CreateAsync(serviceId);

            _listener.ConnectionReceived += OnConnectionReceived;
            await _listener.BindServiceNameAsync(serviceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
            
            InitializeServiceSdpAttributes(_serviceProvider);
            _serviceProvider.StartAdvertising(_listener, true);
        }

        /// <summary>
        /// Initialize the Rfcomm service's SDP attributes.
        /// </summary>
        /// <param name="rfcommProvider">The Rfcomm service provider to initialize</param>
        private static void InitializeServiceSdpAttributes(RfcommServiceProvider rfcommProvider)
        {
            var sdpWriter = new DataWriter();

            // Write the Service Name Attribute.
            sdpWriter.WriteByte(BluetoothConnection.SdpServiceNameAttributeType);

            // The length of the UTF-8 encoded Service Name SDP Attribute.
            sdpWriter.WriteByte((byte)BluetoothConnection.SdpServiceName.Length);

            // The UTF-8 encoded Service Name value.
            sdpWriter.UnicodeEncoding = UnicodeEncoding.Utf8;
            sdpWriter.WriteString(BluetoothConnection.SdpServiceName);

            // Set the SDP Attribute on the RFCOMM Service Provider.
            rfcommProvider.SdpRawAttributes.Add(
                BluetoothConnection.SdpServiceNameAttributeId,
                sdpWriter.DetachBuffer());
        }

        private async void OnConnectionReceived(StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                var connection = await StreamSocketConnection
                    .AcceptConnectionAsync(args.Socket, _serializer)
                    .ContinueOnOtherContext();

                _connectionReceived.OnNext(connection);
            }
            catch
            {
                // Connection attempt failed
            }
        }
    }
}
