# [FishyMultiplayerServiceHandler](https://github.com/youngwoocho02/FishyMultiplayerServiceHandler)

A handler to bridge Unity 6's new [`Multiplayer Services`](https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual) package with the [`FishNet`](https://fish-networking.gitbook.io/docs) networking solution. This allows you to use Unity's session management with FishNet without writing complex boilerplate code.

# What is Multiplayer Services Package? And Why Use It?

Unity's **[`Multiplayer Services`](https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual)** package is an integrated solution designed to dramatically simplify the development of multiplayer games. It acts as a central hub that combines various powerful Unity Gaming Services (UGS) like **[`Lobby`](https://docs.unity.com/ugs/manual/lobby/manual/get-started), [`Relay`](https://docs.unity.com/ugs/manual/relay/manual/get-started), [`Matchmaker`](https://docs.unity.com/ugs/manual/matchmaker/manual/get-started), and [`Multiplay Hosting`](https://docs.unity.com/ugs/manual/game-server-hosting/manual/welcome-to-multiplay)** into a unified, high-level API.

The core concept of this package is the **Session**. A session represents a group of players playing together, and it abstracts away the complex backend operations required to connect them.

The primary reason to use this package is **automation and simplification**. In the past, developers had to manually write "glue code" to coordinate between separate services like Lobby and Relay. For example, you would need to:
1. Create a [`Lobby`](https://docs.unity.com/ugs/manual/lobby/manual/get-started).
2. If using [`Relay`](https://docs.unity.com/ugs/manual/relay/manual/get-started), allocate a Relay server and get a join code.
3. Store that Relay join code in the lobby's data.
4. When a player joins the lobby, retrieve the join code to connect to the Relay server.

The Multiplayer Services package automates this entire workflow. By simply calling a single function to create a session (e.g., a Relay session), the package automatically handles the lobby creation, Relay allocation, and the association between them. This allows developers to focus on the core gameplay logic rather than spending weeks on complex networking boilerplate, significantly reducing development time and resources.

By using this handler with [`FishNet`](https://fish-networking.gitbook.io/docs), you can leverage all these benefits from Unity's ecosystem while still using FishNet's high-performance networking framework.

## Prerequisites

Before you begin, ensure your project has the following packages installed:

*   **[`Unity 6.0`](https://docs.unity3d.com/6000.0/Documentation/Manual/index.html)** or newer
*   **[`FishNet`](https://fish-networking.gitbook.io/docs)**: Networking Evolved (from Asset Store or GitHub)
*   **[`Unity Transport`](https://docs-multiplayer.unity3d.com/transport/current/about/)** (`com.unity.transport` version 2.0 or newer)
*   **[`Unity Multiplayer Services`](https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual)** (`com.unity.services.multiplayer` version 1.1.0 or newer)
*   **[`FishyUnityTransport`](https://github.com/ooonush/FishyUnityTransport)** (required for Unity Relay functionality)

## Installation

1.  **Install Dependencies**: Using the Unity Package Manager, install the [`Unity Transport`](https://docs-multiplayer.unity3d.com/transport/current/about/) and [`Multiplayer Services`](https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual) packages from the Unity Registry.
2.  **Install [`FishyUnityTransport`](https://github.com/ooonush/FishyUnityTransport)**: Add the package via Git URL in the Package Manager. This is required for using Unity Relay.
    ```
    https://github.com/ooonush/FishyUnityTransport.git?path=Assets/FishNet/Plugins/FishyUnityTransport
    ```
3.  **Install this Handler**: Add this package using its Git URL in the Package Manager. You will need to create a `package.json` file in the root of your package folder.
    ```
    https://github.com/youngwoocho02/FishyMultiplayerServiceHandler.git
    ```
4.  **Configure Transport (Only Using Relay)**: On your `NetworkManager` GameObject, add the `UnityTransport` component and set Protocol Type to `Relay Unity Transport`

## Usage Examples

**Create Direct Session**
```csharp
private async Task CreateDirectSession()
{
    var option = new SessionOptions()
    {
        MaxPlayers = 4,
    }.WithFishyDirectNetwork();
    // or use .WithFishyDirectNetwork(yourCustomListenAddress, yourCustomPublishAddress, yourCustomPort);
    // for custom addresses

    var result = await MultiplayerService.Instance.CreateSessionAsync(option);

    Debug.Log($"Id: {result.Id}");
    Debug.Log($"Code: {result.Code}");
}
```

**Create Relay Session**
```csharp
private async Task CreateRelaySession()
{
    // Create a session using Unity Relay
    var options = new SessionOptions()
    {
        MaxPlayers = 4,
    }.WithFishyRelayNetwork();

    var result = await MultiplayerService.Instance.CreateSessionAsync(options);

    Debug.Log($"Id: {result.Id}");
    Debug.Log($"Code: {result.Code}");
}
```

**Join Session By Id**
```csharp
private async Task JoinSessionId(string sessionId)
{
    var option = new JoinSessionOptions()
    {
        Password = null,
    }.WithFishyHandler();

    var result = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, option);
}
```

**Join Session By Code**
```csharp
private async Task JoinSessionCode(string sessionCode, string password)
{
    var option = new JoinSessionOptions()
    {
        Password = password,
    }.WithFishyHandler();

    var result = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode, option);
}
```

**Quick Join Or Create Session By Lobby**
```csharp
private async Task QuickJoinOrCreateByLobby()
{
    var quickJoinOption = new QuickJoinOptions()
    {
        Filters = new List<FilterOption>
        {
            new(FilterField.AvailableSlots, "1", FilterOperation.GreaterOrEqual),
        },
        Timeout = TimeSpan.FromSeconds(10),
        CreateSession = true,
    };

    var sessionOption = new SessionOptions()
    {
        MaxPlayers = 4,
    }.WithFishyRelayNetwork();

    var result = await MultiplayerService.Instance.MatchmakeSessionAsync(quickJoinOption, sessionOption);
}
```

## License

This project is distributed under the MIT License. See the `LICENSE` file for more information.

## Related Resources

*   [FishNet Official Documentation](https://fish-networking.gitbook.io/docs)
*   [Unity Multiplayer Services Documentation](https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual)
*   [Unity Transport Documentation](https://docs-multiplayer.unity3d.com/transport/current/about/)
*   [FishyUnityTransport GitHub Repository](https://github.com/ooonush/FishyUnityTransport)
