using System.Threading.Tasks;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;

public class FishyMultiplayerServiceHandler : INetworkHandler
{
    public bool UseMultipassIndex = false;
    public int MultipassIndex = 0;

    private NetworkType _type;
    private NetworkRole _role;

    public Task StartAsync(NetworkConfiguration config)
    {
        Debug.Log($"Fishy Handler Start Info: Role: {config.Role}, Type: {config.Type}");
        _type = config.Type;
        _role = config.Role;


        if (InstanceFinder.ClientManager.Started)
        {
            InstanceFinder.ClientManager.StopConnection();
            Debug.LogWarning("Stopping existing client connection before starting a new one.");
        }
        if (InstanceFinder.ServerManager.AnyServerStarted())
        {
            InstanceFinder.ServerManager.StopConnection(true);
            Debug.LogWarning("Stopping existing server connection before starting a new one.");
        }


        // Set the transport based on the network type
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
                var transport = InstanceFinder.TransportManager.Transport.GetComponent<UnityTransport>();
                if (transport == null)
                {
                    return Task.FromException(new System.InvalidOperationException("Transport is not UnityTransport."));
                }

                var multipass = InstanceFinder.TransportManager.Transport as Multipass;
                if (multipass != null)
                {
                    multipass.SetClientTransport<UnityTransport>();
                    Debug.Log("Fishy Handler Start Info: Multipass transport set to UnityTransport for Relay.");
                }

                transport.SetRelayServerData(config.RelayServerData);
                Debug.Log($"Relay: Server Data: {config.RelayServerData.Endpoint}, Client Data: {config.RelayClientData.Endpoint}");
                break;
            case NetworkType.DistributedAuthority:
                return Task.FromException(new System.NotSupportedException("Distributed Authority is not supported in FishNet."));
            default:
                return Task.FromException(new System.InvalidOperationException("Unknown network type."));
        }


        // Start the server and client connections based on the role
        if (config.Role is NetworkRole.Server or NetworkRole.Host)
        {
            if (UseMultipassIndex)
            {
                var multipass = InstanceFinder.TransportManager.Transport as Multipass;
                if (multipass == null)
                {
                    return Task.FromException(new System.InvalidOperationException("Multipass transport is not set up correctly."));
                }

                if (MultipassIndex < 0 || MultipassIndex >= multipass.Transports.Count)
                {
                    return Task.FromException(new System.ArgumentOutOfRangeException(nameof(MultipassIndex), "Multipass index is out of range."));
                }

                multipass.StartConnection(true, MultipassIndex);
                Debug.Log($"Fishy Handler Start Info: Server started successfully on Multipass index {MultipassIndex}.");
            }
            else
            {
                var serverResult = InstanceFinder.ServerManager.StartConnection();
                if (serverResult == false)
                {
                    return Task.FromException(new System.InvalidOperationException("Failed to start server connection."));
                }

                Debug.Log($"Fishy Handler Start Info: Server started successfully.");
            }
        }

        if (config.Role is NetworkRole.Client or NetworkRole.Host)
        {
            if (UseMultipassIndex)
            {
                var multipass = InstanceFinder.TransportManager.Transport as Multipass;
                if (multipass == null)
                {
                    return Task.FromException(new System.InvalidOperationException("Multipass transport is not set up correctly."));
                }

                if (MultipassIndex < 0 || MultipassIndex >= multipass.Transports.Count)
                {
                    return Task.FromException(new System.ArgumentOutOfRangeException(nameof(MultipassIndex), "Multipass index is out of range."));
                }

                multipass.StartConnection(false, MultipassIndex);
                Debug.Log($"Fishy Handler Start Info: Client started successfully on Multipass index {MultipassIndex}.");
            }
            else
            {
                var clientResult = InstanceFinder.ClientManager.StartConnection();
                if (clientResult == false)
                {
                    return Task.FromException(new System.InvalidOperationException("Failed to start client connection."));
                }

                Debug.Log($"Fishy Handler Start Info: Client started successfully.");
            }
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        Debug.Log($"Fishy Handler Stop Info: Role: {_role}, Type: {_type}");

        if (InstanceFinder.ClientManager.Started)
        {
            InstanceFinder.ClientManager.StopConnection();
            Debug.Log($"Fishy Handler Stop Info: Client stopped successfully.");
        }
        if (InstanceFinder.ServerManager.AnyServerStarted())
        {
            InstanceFinder.ServerManager.StopConnection(true);
            Debug.Log($"Fishy Handler Stop Info: Server stopped successfully.");
        }

        return Task.CompletedTask;
    }
}

