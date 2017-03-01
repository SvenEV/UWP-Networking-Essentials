using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.System.RemoteSystems;

namespace UwpNetworkingEssentials.Channels.AppServices
{
    public class ASConnection : ConnectionBase
    {
        private readonly AppServiceConnection _internalConnection;
        private readonly BackgroundTaskDeferral _internalConnectionDeferral;
        internal readonly IObjectSerializer _serializer;
        private DisconnectReason _disconnectReason = DisconnectReason.Unknown;

        public bool IsRemoteSystemConnection { get; }

        private ASConnection(string id, AppServiceConnection connection, BackgroundTaskDeferral connectionDeferral,
            bool isRemoteSystemConnection, IObjectSerializer serializer) : base(id)
        {
            _internalConnection = connection;
            _internalConnection.RequestReceived += OnRequestReceived;
            _internalConnection.ServiceClosed += OnUnderlyingConnectionClosed;
            _internalConnectionDeferral = connectionDeferral;
            _serializer = serializer;
            IsRemoteSystemConnection = isRemoteSystemConnection;
        }

        private async void OnUnderlyingConnectionClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            switch (args.Status)
            {
                case AppServiceClosedStatus.Completed:
                    _disconnectReason = DisconnectReason.RemotePeerDisconnected;
                    break;

                case AppServiceClosedStatus.Canceled:
                case AppServiceClosedStatus.ResourceLimitsExceeded:
                    _disconnectReason = DisconnectReason.UnexpectedDisconnect;
                    break;

                default:
                    _disconnectReason = DisconnectReason.Unknown;
                    break;
            }

            await DisposeAsync(); // TODO: Potentially dangerous 'await' in 'async void' method
        }

        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var request = new ASRequest(args.Request, args.GetDeferral(), this);

            _requestReceived.OnNext(request);
            await request.WaitForDeferralsAsync();

            // if no one responded to the message, respond with an empty message now
            if (!request.HasResponded)
                await request.SendResponseAsync(null);
        }

        public static async Task<ASConnection> AcceptConnectionAsync(AppServiceTriggerDetails e,
            BackgroundTaskDeferral connectionDeferral, IObjectSerializer serializer)
        {
            // Receive connection request, send connection response
            AppServiceRequest request = null;
            AppServiceDeferral deferral = null;

            var gotRequest = new SemaphoreSlim(0);
            e.AppServiceConnection.RequestReceived += OnRequestReceived;
            await gotRequest.WaitAsync();
            e.AppServiceConnection.RequestReceived -= OnRequestReceived;

            var message = serializer.DeserializeFromValueSet(request.Message);

            if (message is ConnectionRequestMessage connectionRequest)
            {
                // Accept connection request
                var connectionId = "AS_" + Guid.NewGuid();
                var connectionResponse = new ConnectionResponseMessage(connectionId);
                await request.SendResponseAsync(serializer.SerializeToValueSet(connectionResponse));
                deferral.Complete();
                return new ASConnection(
                    connectionId, e.AppServiceConnection, connectionDeferral,
                    e.IsRemoteSystemConnection, serializer);
            }
            else
            {
                // Wrong message received => reject connection
                var connectionResponse = new ConnectionResponseMessage(null);
                await request.SendResponseAsync(serializer.SerializeToValueSet(connectionResponse));
                deferral.Complete();
                connectionDeferral.Complete();
                e.AppServiceConnection.Dispose();
                return null;
            }

            void OnRequestReceived(AppServiceConnection _, AppServiceRequestReceivedEventArgs r)
            {
                request = r.Request;
                deferral = r.GetDeferral();
                gotRequest.Release();
            }
        }

        public static Task<ASConnectionConnectResult> ConnectLocallyAsync(string appServiceName,
            string packageFamilyName, IObjectSerializer serializer)
        {
            return ConnectInternalAsync(appServiceName, packageFamilyName, null, serializer);
        }

        public static Task<ASConnectionConnectResult> ConnectRemotelyAsync(string appServiceName,
            string packageFamilyName, RemoteSystem remoteSystem, IObjectSerializer serializer)
        {
            return ConnectInternalAsync(appServiceName, packageFamilyName, remoteSystem, serializer);
        }

        private static async Task<ASConnectionConnectResult> ConnectInternalAsync(string appServiceName,
            string packageFamilyName, RemoteSystem remoteSystem, IObjectSerializer serializer)
        {
            var connection = new AppServiceConnection
            {
                AppServiceName = appServiceName,
                PackageFamilyName = packageFamilyName
            };

            var isRemoteSystemConnection = (remoteSystem != null);

            var connectStatus = isRemoteSystemConnection
                ? await connection.OpenRemoteAsync(new RemoteSystemConnectionRequest(remoteSystem))
                : await connection.OpenAsync();

            if (connectStatus != AppServiceConnectionStatus.Success)
                return new ASConnectionConnectResult(connectStatus, AppServiceHandshakeStatus.Unknown, null);

            // Send connection request, receive connection response
            var request = new ConnectionRequestMessage();
            var response = await connection.SendMessageAsync(request, RequestOptions.Default, serializer);

            var success =
                response.Status == RequestStatus.Success &&
                response.Response is ConnectionResponseMessage responseObject &&
                responseObject.IsSuccessful;

            if (!success)
                return new ASConnectionConnectResult(
                    AppServiceConnectionStatus.Success, AppServiceHandshakeStatus.ConnectionRequestFailure, null);

            var asConnection = new ASConnection(
                ((ConnectionResponseMessage)response.Response).ConnectionId,
                connection, null, isRemoteSystemConnection, serializer);

            return new ASConnectionConnectResult(
                AppServiceConnectionStatus.Success,
                AppServiceHandshakeStatus.Success,
                asConnection);
        }

        protected override Task CloseCoreAsync()
        {
            _disconnectReason = DisconnectReason.LocalPeerDisconnected;
            return Task.CompletedTask;
        }

        protected override async Task<RequestResult> SendMessageCoreAsync(object message, RequestOptions options)
        {
            return await _internalConnection.SendMessageAsync(message, options, _serializer);
        }

        protected override void DisposeCore()
        {
            _disconnected.OnNext(new DisconnectEventArgs(this, _disconnectReason));
            _internalConnection.Dispose();
            _internalConnectionDeferral?.Complete();
        }
    }
}