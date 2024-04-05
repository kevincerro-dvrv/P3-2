using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    // Network variables
    public NetworkVariable<Vector3> PlayerPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>();

    // Class components
    private MeshRenderer meshRenderer;
    private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            RequestSpawnPlayerRpc();
        }
    }

    void Awake()
    {
        gameManager = GameManager.instance;
    }

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        transform.position = PlayerPosition.Value;
        meshRenderer.material.SetColor("_Color", PlayerColor.Value);
    }

    [Rpc(SendTo.Server)]
    void RequestSpawnPlayerRpc(RpcParams rpcParams = default)
    {
        PlayerPosition.Value = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        ChangeColorRpc();
    }

    public void ChangeColor() {
        ChangeColorRpc();
    }

    [Rpc(SendTo.Server)]
    void ChangeColorRpc(RpcParams rpcParams = default)
    {
        try {
            Color newColor = GetRandomNotUsedColor();
            gameManager.RemoveColorFromUsed(PlayerColor.Value);
            PlayerColor.Value = newColor;
        } catch (NoAvailableColorException ex) {
            Debug.Log("No new color is available. Skipping ChangeColor request");
        }
    }

    Color GetRandomNotUsedColor()
    {
        List<Color> availableColors = gameManager.GetAvailableColors();

        // If no available colors, throw exception
        if (availableColors.Count == 0) {
            Debug.Log("No available colors");
            throw new NoAvailableColorException();
        }

        // Pick new random color from available colors
        Color selectedColor = availableColors[Random.Range(0, availableColors.Count)];

        // Track new and old colors
        gameManager.AddColorToUsed(selectedColor);
        
        return selectedColor;
    }
}
