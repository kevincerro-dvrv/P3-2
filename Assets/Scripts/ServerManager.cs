using ParrelSync;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public int maxPlayers = 6;

    private NetworkManager networkManager;

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        if (networkManager != null)
        {
            networkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            networkManager.ConnectionApprovalCallback = ApprovalCheck;
        }

        // Autostart as client if its clone
        if (ClonesManager.IsClone()) {
            NetworkManager.Singleton.StartClient();
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();

            RequestColorChange();
        }

        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    static void RequestColorChange()
    {
        if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Change color" : "Request Color Change"))
        {
            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient )
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<PlayerManager>().ChangeColor();
            }
            else
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<PlayerManager>();
                player.ChangeColor();
            }
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (!CanFitMorePlayers()) {
            response.Approved = false;
            response.Reason = "Server is full";
            Debug.Log("Connection denied");
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = true;
        Debug.Log("Connection approved");
    }

    private void OnClientDisconnectCallback(ulong obj)
    {
        Debug.Log("Player disconnected");

        if (!networkManager.IsServer && networkManager.DisconnectReason != string.Empty)
        {
            Debug.Log($"Reason: {networkManager.DisconnectReason}");
        }

        // Release player disconnected color
        networkManager.ConnectedClients.TryGetValue(obj, out NetworkClient disconnectedPlayer);
        if (disconnectedPlayer != null) {
            PlayerManager playerManager = disconnectedPlayer.PlayerObject.gameObject.GetComponent<PlayerManager>();
            GameManager.instance.RemoveColorFromUsed(playerManager.PlayerColor.Value);
        }
    }

    public bool CanFitMorePlayers()
    {
        return networkManager.ConnectedClients.Count < maxPlayers;
    }
}
