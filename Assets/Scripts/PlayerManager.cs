using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public float playerSpeed = 50f;

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
        //QualitySettings.vSyncCount = 1;
	    //Application.targetFrameRate = 60;

        gameManager = GameManager.instance;
    }

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        UpdatePlayerPosition(PlayerPosition.Value, PlayerPosition.Value);
        PlayerPosition.OnValueChanged += UpdatePlayerPosition;
        UpdatePlayerColor(PlayerColor.Value, PlayerColor.Value);
        PlayerColor.OnValueChanged += UpdatePlayerColor;
    }

    void FixedUpdate()
    {
        if (IsOwner) {
            RequestMovePlayer();
        }
    }

    private void UpdatePlayerPosition(Vector3 previousValue, Vector3 newValue)
    {
        transform.position = newValue;
    }

    private void UpdatePlayerColor(Color previousValue, Color newValue)
    {
        meshRenderer.material.SetColor("_Color", newValue);
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

    void RequestMovePlayer()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        RequestMovePlayerRpc(movement);
    }

    [Rpc(SendTo.Server)]
    void RequestMovePlayerRpc(Vector3 movement)
    {
        transform.position += movement * playerSpeed * Time.fixedDeltaTime;
    }
}
