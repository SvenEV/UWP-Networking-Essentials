﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Threading.Tasks;
using UwpNetworkingEssentials.Channels;
using UwpNetworkingEssentials.Rpc;

namespace UwpNetworkingEssentials.ChatSample.ViewModels
{
    public partial class ClientViewModel : ApplicationViewAwareViewModel, IRpcTarget
    {
        public async void AddMessage(string message, [RpcCaller]RpcConnection caller)
        {
            // RPC method called by the server to add a message that has been
            // sent by the server or another client to the message list
            await RunAsync(() =>
                Messages.Add($"{caller.Id} said: " + message));
        }

        public async void OnConnected(RpcConnection connection)
        {
            await RunAsync(() =>
                Messages.Add($"Connected to {connection.Id}"));
        }

        public async void OnDisconnected(RpcConnection connection, DisconnectEventArgs args)
        {
            await RunAsync(async () =>
            {
                Messages.Add($"Disconnected from {connection.Id} (Reason: {args.Reason})");
                await Task.Delay(2000);
                if (_frame.CanGoBack)
                    _frame.GoBack();
            });
        }

        public async void OnConnectionAttemptFailed(RpcConnectionAttemptFailedException exception)
        {
            await RunAsync(async () =>
            {
                Messages.Add($"Failed to connect to {exception.RemoteHostName}:{exception.RemotePort}");
                await Task.Delay(2000);
                if (_frame.CanGoBack)
                    _frame.GoBack();
            });
        }
    }
}
