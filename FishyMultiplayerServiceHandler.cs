using System.Threading.Tasks;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;

public class FishyMultiplayerServiceHandler : INetworkHandler
{
    private NetworkType _type;
    private NetworkRole _role;

    public Task StartAsync(NetworkConfiguration config)
    {
        Debug.Log($"Fishy Handler Start Info: Role: {config.Role}, Type: {config.Type}");
        _type = config.Type;
        _role = config.Role;

        switch (config.Type)
        {
            case NetworkType.Direct:
                Debug.Log($"Listen Address: {config.DirectNetworkListenAddress}, Publish Address: {config.DirectNetworkPublishAddress}");

                if (config.Role is NetworkRole.Server or NetworkRole.Host)
                {
                    var address = config.DirectNetworkListenAddress.ToFixedStringNoPort().ToString();
                    var port = config.DirectNetworkListenAddress.Port;
                    var ipType = config.DirectNetworkListenAddress.Family switch
                    {
                        Unity.Networking.Transport.NetworkFamily.Ipv4 => IPAddressType.IPv4,
                        Unity.Networking.Transport.NetworkFamily.Ipv6 => IPAddressType.IPv6,
                        Unity.Networking.Transport.NetworkFamily.Invalid => throw new System.InvalidOperationException("Invalid network family."),
                        Unity.Networking.Transport.NetworkFamily.Custom => throw new System.NotSupportedException("Custom network family is not supported."),
                        _ => throw new System.NotImplementedException(),
                    };

                    InstanceFinder.TransportManager.Transport.SetServerBindAddress(address, ipType);
                    InstanceFinder.TransportManager.Transport.SetPort(port);
                    Debug.Log($"Fishy Handler Start Info: Server bind address set to {address} with port {port} and IP type {ipType}.");
                }

                if (config.Role is NetworkRole.Client)
                {
                    var address = config.DirectNetworkPublishAddress.ToFixedStringNoPort().ToString();
                    var port = config.DirectNetworkPublishAddress.Port;

                    InstanceFinder.TransportManager.Transport.SetClientAddress(address);
                    InstanceFinder.TransportManager.Transport.SetPort(port);
                    Debug.Log($"Fishy Handler Start Info: Client address set to {address} with port {port}.");
                }

                if (config.Role is NetworkRole.Host)
                {
                    var address = "127.0.0.1";
                    var port = config.DirectNetworkListenAddress.Port;

                    InstanceFinder.TransportManager.Transport.SetClientAddress(address);
                    InstanceFinder.TransportManager.Transport.SetPort(port);
                    Debug.Log($"Fishy Handler Start Info: Host address set to {address} with port {port}.");
                }

                break;
            case NetworkType.Relay:
                var transport = InstanceFinder.TransportManager.Transport as UnityTransport;
                if (transport == null)
                {
                    return Task.FromException(new System.InvalidOperationException("Transport is not UnityTransport."));
                }

                // var relayData = config.Role is NetworkRole.Server or NetworkRole.Host
                //     ? config.RelayServerData
                //     : config.RelayClientData;
                transport.SetRelayServerData(config.RelayServerData);
                Debug.Log($"Relay: Server Data: {config.RelayServerData}, Client Data: {config.RelayClientData}");
                break;
            case NetworkType.DistributedAuthority:
                return Task.FromException(new System.NotSupportedException("Distributed Authority is not supported in FishNet."));
            default:
                return Task.FromException(new System.InvalidOperationException("Unknown network type."));
        }

        if (config.Role is NetworkRole.Server or NetworkRole.Host)
        {
            var serverResult = InstanceFinder.ServerManager.StartConnection();
            if (serverResult == false)
            {
                return Task.FromException(new System.InvalidOperationException("Failed to start server connection."));
            }

            Debug.Log($"Fishy Handler Start Info: Server started successfully.");
        }

        if (config.Role is NetworkRole.Client or NetworkRole.Host)
        {
            var clientResult = InstanceFinder.ClientManager.StartConnection();
            if (clientResult == false)
            {
                return Task.FromException(new System.InvalidOperationException("Failed to start client connection."));
            }

            Debug.Log($"Fishy Handler Start Info: Client started successfully.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        Debug.Log($"Fishy Handler Stop Info: Role: {_role}, Type: {_type}");

        if (_role is NetworkRole.Client or NetworkRole.Host)
        {
            var clientResult = InstanceFinder.ClientManager.StopConnection();
            if (clientResult == false)
            {
                return Task.FromException(new System.InvalidOperationException("Failed to stop client connection."));
            }

            Debug.Log($"Fishy Handler Stop Info: Client stopped successfully.");
        }

        if (_role is NetworkRole.Server or NetworkRole.Host)
        {
            var serverResult = InstanceFinder.ServerManager.StopConnection(true);
            if (serverResult == false)
            {
                return Task.FromException(new System.InvalidOperationException("Failed to stop server connection."));
            }

            Debug.Log($"Fishy Handler Stop Info: Server stopped successfully.");
        }

        return Task.CompletedTask;
    }
}

