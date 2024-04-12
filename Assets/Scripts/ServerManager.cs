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
        } else {
            StatusLabels();
        }
    
        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" : "Client";

        GUILayout.Label("Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
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
        if (!NetworkManager.Singleton.IsServer) {
            return;
        }

        Debug.Log("Player disconnected");

        if (networkManager.DisconnectReason != string.Empty)
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
