using Unity.Services.Multiplayer;

public static class FishyMultiplayerServiceHandlerExtensions
{
    public static T WithFishyHandler<T>(this T options, FishyMultiplayerServiceHandler handler = null) where T : BaseSessionOptions
    {
        return options.WithNetworkHandler(handler ?? new FishyMultiplayerServiceHandler());
    }

    public static T WithFishyRelayNetwork<T>(this T options, string region = null, FishyMultiplayerServiceHandler handler = null) where T : SessionOptions
    {
        return options.WithRelayNetwork(region).WithFishyHandler(handler);
    }

    public static T WithFishyDirectNetwork<T>(this T options, string listenIp = "127.0.0.1", string publishIp = "127.0.0.1", int port = 7770, FishyMultiplayerServiceHandler handler = null) where T : SessionOptions
    {
        return options.WithDirectNetwork(listenIp, publishIp, port).WithFishyHandler(handler);
    }
}