using Unity.Services.Multiplayer;

public static class FishyMultiplayerServiceHandlerExtensions
{
    public static T WithFishyHandler<T>(this T options) where T : BaseSessionOptions
    {
        return options.WithNetworkHandler(new FishyMultiplayerServiceHandler());
    }

    public static T WithFishyRelayNetwork<T>(this T options, string region = null) where T : SessionOptions
    {
        return options.WithRelayNetwork(region).WithFishyHandler();
    }

    public static T WithFishyDirectNetwork<T>(this T options, string listenIp = "127.0.0.1", string publishIp = "127.0.0.1", int port = 7770) where T : SessionOptions
    {
        return options.WithDirectNetwork(listenIp, publishIp, port).WithFishyHandler();
    }
}