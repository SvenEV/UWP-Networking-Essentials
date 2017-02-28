# UWP Networking Essentials
Simple and lightweight networking (including RPC) for Universal Windows Platform apps.

Available on [NuGet: UWP Networking Essentials](https://www.nuget.org/packages/UwpNetworkingEssentials/).

## Features
* Easily connect UWP apps running on one or multiple devices
* Exchange arbitrary objects between server and clients (JSON serialized by default)
* A common API for messaging via StreamSockets and UWP app services (+ more channels in the future)
* Basic RPC (remote procedure calling) API
  * Supports parameters, return values and async methods
  * Method invocation using dynamic proxies

## Status
* Development is currently focused on interesting features and a convenient API design, NOT on security, performance or correctness
* Thus, it is currently not recommended to use this library in any really serious or mission-critical projects
* This being said, it might be useful and time-saving in small or personal projects

## Getting started
Please download the ChatSample app to understand how to use the RPC API.
Documentation is still scarce, sorry :(

Server example:

```csharp
// Start a server on port 1234
var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
var listener = new StreamSocketConnectionListener("1234", serializer);
await listener.StartAsync();
var server = new RpcServer(listener, serverRpcObject);

// Remotely invoke "clientRpcObject.DoStuffOnClient" on all connected clients
await server.Clients.DoStuffOnClient("Hello clients!");

// Remotely invoke "clientRpcObject.DoStuffOnClient" on a specific client
await server.Client(connectionId).DoStuffOnClient("Hello specific client!");
```

Client example:

```csharp
// Connect to a server
var serializer = new DefaultJsonSerializer(GetType().GetTypeInfo().Assembly);
var connection = await StreamSocketConnection.ConnectAsync("192.168.1.99", "1234", serializer);
var client = new RpcClient(connection, clientRpcObject);

// Remotely invoke "serverRpcObject.DoStuffOnServer" on the server
await client.Server.DoStuffOnServer("Hello server!");
```

So, what if you need your clients to do RPC via a UWP app service instead of StreamSockets?
Just replace
```csharp
var connection = await StreamSocketConnection.ConnectAsync("192.168.1.99", "1234", serializer);
```
with
```csharp
var connection = await ASConnection.ConnectLocallyAsync("AppServiceName", "PackageFamilyName", serializer);
```
