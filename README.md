# UWP Networking Essentials
Simple and lightweight networking (including RPC) for Universal Windows Platform apps.

Available on [NuGet: UWP Networking Essentials](https://www.nuget.org/packages/UwpNetworkingEssentials/).

## Features
* Easily connect UWP apps running on multiple devices
* Exchange arbitrary objects between server and clients (JSON serialized by default)
* Built on top of StreamSockets
* Basic RPC (remote procedure calling) API
  * Supports parameters, return values and async methods
  * Method invocation using dynamic proxies

## Status
* Work in progress
* It's not (yet) bug-free
* It's not secure
* It's not designed for high performance
* Contributions welcome :)

## Getting started
Please download the ChatSample app to understand how to use the RPC API.

Server example:

```csharp
// Start a server on port 1234
var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
var server = await RpcServer.StartAsync("1234", serverRpcObject, serializer);

// Remotely invoke "clientRpcObject.DoStuffOnClient" on all connected clients
await server.Clients.DoStuffOnClient("Hello clients!");

// Remotely invoke "clientRpcObject.DoStuffOnClient" on a specific client
await server.Client(connectionId).DoStuffOnClient("Hello specific client!");
```

Client example:

```csharp
// Connect to a server
var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
var client = await RpcClient.ConnectAsync("192.168.178.99", "1234", clientRpcObject, serializer);

// Remotely invoke "serverRpcObject.DoStuffOnServer" on the server
await client.Server.DoStuffOnServer("Hello server!");
```
